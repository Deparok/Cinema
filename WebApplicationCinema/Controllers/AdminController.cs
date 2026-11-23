using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplicationCinema.Models;
using WebApplicationCinema.Models.DBInstances;
using WebApplicationCinema.ViewModels;
using static System.Net.Mime.MediaTypeNames;
using static WebApplicationCinema.Models.Extensions;

namespace WebApplicationCinema.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AnonymousController> _logger;
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IAsyncDBWorker _db;

        public AdminController(ILogger<AnonymousController> logger, IWebHostEnvironment env, [FromKeyedServices("admin")] IAsyncDBWorker db)
        {
            _logger = logger;
            _webHostEnv = env;
            _db = db;
        }

        [Route("/panel")]
        public async Task<IActionResult> Panel()
        {

            List<Hall> halls = await _db.ExecuteQueryAsync<Hall>("SELECT * FROM Halls");
            List<Movie> movies = await _db.ExecuteQueryAsync<Movie>("SELECT * FROM Movies");
            List<Session> sessions = await _db.ExecuteQueryAsync<Session>("SELECT * FROM [Sessions]");
            return View(new PanelViewModel("Admin", User, halls, movies, sessions));
        }

        private record JsonMovie(int Id, string Name );

        [Route("/giveawayMovies")]
        public async Task<IActionResult> GiveawayMovies(DateOnly? date)
        {
            if (date is null)
                return BadRequest();
            List<Movie> movies = await _db.ExecuteQueryAsync<Movie>($"SELECT * FROM Movies WHERE RuntimeStart <= CAST('{date.ToSqlString()}' AS DATE) AND RuntimeStop >= CAST('{date.ToSqlString()}' AS DATE)");

            return Json(movies.Select(movie => new { id = movie.ID, name = movie.Name, year = movie.Year })); 
        }

        [Route("/giveawayHalls")]
        public async Task<IActionResult> GiveawayHalls(DateOnly? date)
        {
            if (date is null)
                return BadRequest();
            List<Hall> halls = await _db.ExecuteQueryAsync<Hall>($"SELECT * FROM Halls AS h JOIN [Sessions] AS s ON h.ID=s.Hall WHERE s.Date=CAST('{date.ToSqlString()}' AS DATE)");

            return Json(halls.Select(hall => new { id = hall.ID, name = hall.Name }));  
        }

        [Route("/todayFreeTimes")]
        public async Task<IActionResult> TodayFreeTimes(DateOnly? date, uint? hall)
        {
            if (date is null || hall is null)
                return BadRequest();
            List<Session> todaySessions = await _db.ExecuteQueryAsync<Session>($"SELECT * FROM [Sessions] WHERE Hall={hall} AND Date={date.ToSqlString()}");
            return Json(FormFreeTimes(todaySessions));
        }

        [Route("/addHall")]
        [HttpPost]
        public async Task<IActionResult> AddHall(string? chall_name, uint? chall_capacity, uint? chall_rows, string? chall_transfer, string? chall_description)
        {
            if (chall_name is null || chall_capacity is null || chall_rows is null || chall_transfer is null)
                return BadRequest();
            if (chall_transfer != "front" && chall_transfer != "back")
                return BadRequest();
            await _db.ExecuteProcedureAsync("CreateHall", chall_name, chall_description ?? "NULL");
            Hall insertedHall = (await _db.ExecuteQueryAsync<Hall>($"SELECT * FROM Halls WHERE [Name]={chall_name}")).First();
            uint columns = chall_capacity ?? 0 / chall_rows ?? 1;
            uint remaining = chall_capacity ?? 0 - (columns * chall_rows ?? 1);
            int j = 1;
            int start = 1;

            if (chall_transfer == "front")
                start++;
            for (int i = start; i < chall_rows + 1; i++)
                for (j = 1; j < columns + 1; j++)
                    await _db.ExecuteProcedureAsync("CreateSeat", insertedHall.ID.ToString(), i.ToString(), j.ToString());
            if (start != 1)
                for (int i = 0; i < remaining; i++)
                    await _db.ExecuteProcedureAsync("CreateSeat", insertedHall.ID.ToString(), "1", i.ToString());
            else
                for (int i = 0; i < remaining; i++)
                    await _db.ExecuteProcedureAsync("CreateSeat", insertedHall.ID.ToString(), j.ToString(), i.ToString());
            return LocalRedirect(Request.Path);
        }

        [Route("/addMovie")]
        [HttpPost]
        public async Task<IActionResult> AddMovie(string? movie_name, uint? movie_runtime, uint? movie_price, uint? movie_year, string? movie_director, DateOnly? movie_runtime_start, DateOnly? movie_runtime_end, FormFile? movie_poster, string? movie_rating)
        {
            if (movie_name is null || movie_runtime is null || movie_price is null || movie_director is null || movie_runtime_start is null || movie_runtime_end is null)
                return BadRequest();
            //try
            //{
            //    FileStream stream = new FileStream(Path.Combine(_webHostEnv.WebRootPath, "/images", movie_poster!.FileName), FileMode.CreateNew);
            //    movie_poster.CopyTo(stream);
            //    stream.Close();
            //}
            //catch { }
            //_webHostEnv.WebRootPath
            await _db.ExecuteProcedureAsync("CreateMovie", movie_name, movie_runtime.ToString()!, movie_price.ToString()!, movie_year?.ToString() ?? "NULL", movie_director, movie_runtime_start.ToSqlString(), movie_runtime_end.ToSqlString(), "//" , movie_rating ?? "NULL");
            return LocalRedirect(Request.Path);
        }

        [Route("/addSession")]
        [HttpPost]
        public async Task<IActionResult> AddSession(DateOnly? session_date, uint? session_halls, uint? session_movies, TimeOnly? session_start, TimeOnly? session_end, uint? session_extracharge)
        {
            if (session_date is null || session_halls is null || session_movies is null || session_start is null || session_end is null)
                return BadRequest();
            await _db.ExecuteProcedureAsync("CreateSession", session_date.ToSqlString(), session_halls.ToString()!, session_movies.ToString()!, session_start?.ToShortTimeString()!, session_end?.ToShortTimeString()!, session_extracharge?.ToString() ?? "0");
            return LocalRedirect(Request.Path);
        }

        [Route("/giveawayFilm")]
        public async Task<IActionResult> GiveawayFilm(uint? movie)
        {
            if (movie is null)
                return BadRequest();
            Movie curMovie = (await _db.ExecuteQueryAsync<Movie>($"SELECT * FROM Movies WHERE ID={movie}")).FirstOrDefault(Movie.Default);
            
            if (curMovie == Movie.Default)
                return BadRequest();
            return Json(new { time = curMovie.Runtime });
        }

        //private record Delete(List<> items)

        ////[Authorize(Roles = "Admin")]
        //[Route("/delete")]
        //public async Task<IActionResult> Delete()
        //{
        //    await Request.ReadFromJsonAsync<Movie>()
        //}

        [NonAction]
        public List<string> FormFreeTimes(List<Session> sessions)
        {
            List<string> freeTimes = new List<string>();

            for (int i = 0; i < sessions.Count; i++)
            {
                if (i == 0)
                    freeTimes.Add("00:00 - " + sessions[i].StartTime);
                freeTimes.Add(sessions[i - 1].EndTime.ToShortTimeString() + " - " + sessions[i].StartTime.ToShortTimeString());  //17 -19  20 - 22    //19 - 20
                if (i == sessions.Count - 1)
                    freeTimes.Add(sessions[i].EndTime.ToShortTimeString() + " - 23:59");
            }
            return freeTimes;
        }
    }
}
