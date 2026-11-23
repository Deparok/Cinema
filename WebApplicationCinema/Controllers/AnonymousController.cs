using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationCinema.Models;
using WebApplicationCinema.Models.DBInstances;
using WebApplicationCinema.ViewModels;

namespace WebApplicationCinema.Controllers
{
    public class AnonymousController : Controller
    {
        private readonly ILogger<AnonymousController> _logger;
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IAsyncDBWorker _db;
        private readonly IMailBox _mailBox;
        private readonly IPasswordHasher<string> _hasher;
        private readonly IPdfFormer _pdf;

        public AnonymousController(ILogger<AnonymousController> logger, IWebHostEnvironment env, IPasswordHasher<string> hasher, [FromKeyedServices("user")]IAsyncDBWorker db, IMailBox mailBox, IPdfFormer pdf)
        {
            _logger = logger;
            _webHostEnv= env;
            _hasher = hasher;
            _db = db;
            _mailBox = mailBox;
            _pdf = pdf;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            return View(new IndexViewModel("Hello", User, await _db.ExecuteProcedureAsync<Movie>("GetAllRuningMovies", DateOnly.FromDateTime(DateTime.Today).ToSqlString())));
        }

        [Route("/login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel("Login", User));
        }

        [Route("/login")]
        [HttpPost]
        public async Task<IActionResult> Login(string? returnUrl, string? email, string? password)
        {
            //string role = "user";

            if (User.Identity?.IsAuthenticated ?? false)
                return LocalRedirect(returnUrl ?? "/");
            if (email is null || password is null)
                return BadRequest();

            User user = (await _db.ExecuteProcedureAsync<User>("UserLogin", [email])).FirstOrDefault(Models.DBInstances.User.Default);

            if (user == Models.DBInstances.User.Default)
                return View(new LoginViewModel("Login", User, false));

            if (!user.IsActive)
                return Forbid();
            
            if(_hasher.VerifyHashedPassword(null, user.Password, password) == PasswordVerificationResult.Failed)
                return View(new LoginViewModel("Login", User, false));

            Role userRole = (await _db.ExecuteQueryAsync<Role>($"SELECT * FROM Roles WHERE ID={user.Role}")).First();

            List<Claim> claims = new List<Claim>() { new Claim(ClaimTypes.Sid, user.ID.ToString()), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, userRole.Name) };
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(principal);
            return LocalRedirect(returnUrl ?? "/");
        }

        [Route("/register")]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return LocalRedirect("/");
            return View(new BaseViewModel("registration", User));
        }

        [Route("/register")]
        [HttpPost]
        public async Task<IActionResult> Register(string? email)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return LocalRedirect("/");
            if (email is null || !_mailBox.ValidateEmail(email))
                return BadRequest();

            User user = (await _db.ExecuteProcedureAsync<User>("UserLogin", [email])).FirstOrDefault(Models.DBInstances.User.Default);
            if (user != Models.DBInstances.User.Default)
                return LocalRedirect("/login");

            string password = GeneratePassword();
            await _db.ExecuteProcedureAsync("CreateUser", [email, _hasher.HashPassword(null, password)]);
            
            if (_mailBox.Send(email, "Создание аккаунта в кинотеатре \"Эверест\"", $"{email}, \nВаш пароль для входа: \"{password}\""))
                return LocalRedirect("/login");
            else
                return LocalRedirect("/register");

        }

        [Authorize]
        [Route("/logout")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        [Route("/denied")]
        public IActionResult Denied()
        {
            return View();
        }

        [Route("/timing")]
        [HttpGet]
        public async Task<IActionResult> Timing(uint? movie)
        {
            if (movie is null)
                return BadRequest();
            //Console.WriteLine(movie?.ToString() ?? "NONE");
            Movie curMovie = (await _db.ExecuteQueryAsync<Movie>($"SELECT * FROM Movies WHERE ID={movie}")).FirstOrDefault(Movie.Default);
            if (curMovie == Movie.Default)
                return BadRequest();

            List<Session> sessions = await _db.ExecuteQueryAsync<Session>($"SELECT TOP 7 * FROM [Sessions] AS s WHERE s.Movie={movie.Value}");
            if (sessions.Count <= 0)
                return BadRequest();
            return View(new TimingViewModel("purchase", User, curMovie, sessions));
        }

        [Route("/purchase")]
        [HttpGet]
        public async Task<IActionResult> Purchase(uint? session)
        {
            if (session is null)
                return BadRequest();
            //Console.WriteLine(movie);
            //Console.WriteLine(session.ToString());

            Session curSession = (await _db.ExecuteQueryAsync<Session>($"SELECT * FROM [Sessions] WHERE ID={session}")).FirstOrDefault(Session.Default);
            Movie curMovie = (await _db.ExecuteQueryAsync<Movie>($"SELECT * FROM Movies WHERE ID={curSession.Movie}")).FirstOrDefault(Movie.Default);
            if (curMovie == Movie.Default || curSession == Session.Default)
                return BadRequest();
            List<Seat> allSeats = await _db.ExecuteProcedureAsync<Seat>("GetAllSeatsBySession", session.Value.ToString()!);
            List<Ticket> ocupiedTickets = await _db.ExecuteProcedureAsync<Ticket>("GetOcupiedTicketsBySession", session.ToString()!);
            List<SeatModel> ocupiedSeats = allSeats.Where(seat => ocupiedTickets.Any(ticket => ticket.Seat == seat.ID))
                                                    .OrderBy(seat => seat.ID)
                                                    .Zip(ocupiedTickets.OrderBy(ticket => ticket.Seat), (seat, ticket) => new SeatModel(seat.Row, seat.Number, (SeatState)ticket.Status))
                                                    .ToList();
            return View(new PurchaseViewModel("purchase", User, curMovie, curSession, SeatModel.FormNormalizedSeatTable(allSeats, ocupiedSeats))); // REMOVE
        }

        private record Reservation(string email, string action, List<string> seats, uint session);

        [Route("/reserving")]
        [HttpPost]
        public async Task<IActionResult> Reserving()
        {
            Reservation? reservation = await Request.ReadFromJsonAsync<Reservation>();
            if (reservation is null || reservation.email == "") 
                return BadRequest();
            //Console.WriteLine('\n' + reservation.action);

            if (reservation.seats.Count > 5)
                return BadRequest();
            //foreach (string seat in reservation.seats)
            //    Console.WriteLine(seat);

            bool isEmailValid = _mailBox.ValidateEmail(reservation.email);

            if (!isEmailValid)
                return Json(new { isEmailValid = isEmailValid, redirect = "#" });

            List<Seat> seats = (await _db.ExecuteProcedureAsync<Seat>("GetAllSeatsBySession", reservation.session.ToString()))
                                    .Where(seat => reservation.seats.Contains($"{seat.Row}-{seat.Number}"))
                                    .ToList();
            foreach (Seat seat in seats)
                await _db.ExecuteQueryAsync($"INSERT INTO Tickets([Session], Seat, [Status]) VALUES ({reservation.session}, {seat.ID}, {(reservation.action == "Покупка" ? 3 : 2)})");

            if (reservation.action == "Покупка")
            {
                //if (!isEmailValid)
                //    return Json(new { isEmailValid = isEmailValid, redirect = "#" });

                List<Ticket> tickets = (await _db.ExecuteQueryAsync<Ticket>($"SELECT * FROM Tickets WHERE [Session]={reservation.session}"))
                                    .Where(ticket => seats.Any(seat => seat.ID == ticket.Seat))
                                    .ToList();

                Session session = (await _db.ExecuteQueryAsync<Session>($"SELECT * FROM [Sessions] WHERE ID={reservation.session}")).First();
                Movie movie = (await _db.ExecuteQueryAsync<Movie>($"SELECT * FROM Movies WHERE ID={session.Movie}")).First();   
                foreach (Ticket ticket in tickets)
                    await _db.ExecuteQueryAsync($"INSERT INTO Payments(Ticket, Amount, ProcessedBy) VALUES ({ticket.ID}, {movie.Price + session.Overprice}, {((User.Identity?.IsAuthenticated ?? false) ? int.Parse(User.Claims.Where(claim => claim.Type == ClaimTypes.Sid).First().Value) : "NULL")})");

                StringBuilder url = new StringBuilder("/download?");
                for (int i = 0; i < tickets.Count; i++)
                    url.Append($"[{i}]={tickets[i].ID}");

                List<Attachment> attachments = new List<Attachment>();
                MemoryStream? stream = await MakePdfStream(tickets.Select(ticket => (uint?)ticket.ID).ToArray());

                if (stream is null)
                    return Json(new { isEmailValid = isEmailValid, redirect = "#" });

                attachments.Add(new Attachment(stream ?? Stream.Null , "application/pdf"));
                _mailBox.Send(reservation.email, "Билеты в кинотеатр \"Эверест\"", "Покупка", attachments);
                stream?.Close();
                return Json(new { isEmailValid = isEmailValid, redirect = url.ToString() });
            }
            return Json(new { isEmailValid = isEmailValid, redirect = isEmailValid ? "/" : "#" });
        }

        //    [Route("/Img")]
        //    [HttpGet]
        //    public Task Img()
        //    {
        //        return Response.WriteAsync(@"<!DOCTYPE html>
        //<html>
        //<head>
        //    <meta charset='utf-8' />
        //    <title>METANIT.COM</title>
        //</head>
        //<body>
        //    <h2>Login Form</h2>
        //    <form method='post'>
        //        <input type='file' name='pict' accept='image/*'/>
        //        <input type='submit' value='Отправить' />
        //    </form>
        //</body>
        //</html>");
        //    }

        [Route("/Img")]
        [HttpGet]
        public IActionResult ImgAccept()//FormFile? pic)
        {
            //System.Net.HttpWebRequest webRequest = new System.Net.HttpWebRequest();
            Console.WriteLine(_webHostEnv.WebRootPath);
            return Ok();
            //FileStream stream = new FileStream(Path.GetTempFileName(), FileMode.Open);
            //pic.CopyTo(stream);
            ////pic.FileContents.CopyTo(stream);
            //stream.Close();
            ////stream.Close();
            //return LocalRedirect("/");
        }

        [Route("/Download")]
        public async Task<IActionResult> Download(uint?[]? tickets)
        {
            MemoryStream? pdf = (await MakePdfStream(tickets));
            byte[]? file = (pdf?.ToArray() ?? null);
            pdf?.Close();
            if (file is null)
                return BadRequest();
            //return File("/images/logo_mountain.png", "image/*", "logo_mountain.png");
            return File(file, "application/pdf", "Билеты_в_кинотеатр_Эверест.pdf");
        }

        [NonAction]
        public async Task<MemoryStream?> MakePdfStream(uint?[]? tickets)
        {
            //if (!User.Identity?.IsAuthenticated ?? false)
            //    return Unauthorized();
            //return PhysicalFile(@"C:\Users\matve\OneDrive\Изображения\Rs6.png", "image/*", "cool_file.png");
            if (tickets is null || tickets.Length == 0)
                return null;
            List<SeatModel> seats = new List<SeatModel>();
            List<Ticket> curTickets = new List<Ticket>();
            Session prevSession = Session.Default;
            Movie prevMovie = Movie.Default;
            for (int i = 0; i < tickets.Length; i++)
            {
                if (tickets[i] is null)
                    continue;

                Ticket curTicket = (await _db.ExecuteQueryAsync<Ticket>($"SELECT * FROM Tickets WHERE ID={tickets[i]}")).FirstOrDefault(Ticket.Default);
                Session curSession = (await _db.ExecuteQueryAsync<Session>($"SELECT s.ID, s.[Date], s.Hall, s.Movie, s.StartTime, s.EndTime, s.Overprice FROM [Sessions] AS s JOIN Tickets AS t ON t.[Session]=s.ID WHERE t.ID={tickets[i]!.Value}")).FirstOrDefault(Session.Default);
                Movie curMovie = (await _db.ExecuteQueryAsync<Movie>($"SELECT m.ID, m.[Name], m.Runtime, m.Price, m.[Year], m.Director, m.RuntimeStart, m.RuntimeStop, m.PosterPath, m.Rating, m.[Description] FROM Movies AS m JOIN [Sessions] AS s ON s.Movie=m.ID JOIN Tickets AS t ON t.[Session]=s.ID WHERE t.ID={tickets[i]!.Value}")).FirstOrDefault(Movie.Default);
                Seat curSeat = (await _db.ExecuteQueryAsync<Seat>($"SELECT * FROM Seats WHERE ID={curTicket.Seat}")).FirstOrDefault(Seat.Default);
                if (i == 0)
                {
                    prevSession = curSession;
                    prevMovie = curMovie;
                }
                if (prevSession != curSession || prevMovie != curMovie || curTicket == Ticket.Default || curSession == Session.Default || curMovie == Movie.Default || curSeat == Seat.Default)
                    return null;

                curTickets.Add(curTicket);
                seats.Add(new SeatModel(curSeat.Row, curSeat.Number, (SeatState)curTicket.Status));
            }

            Hall hall = (await _db.ExecuteQueryAsync<Hall>($"SELECT * FROM Halls WHERE ID={prevSession.Hall}")).FirstOrDefault(Hall.Default);

            if (hall == Hall.Default)
                return null;
            return _pdf.GenerateFileStream(prevMovie.Name, hall.Name, prevSession.Date, prevSession.StartTime, seats, prevMovie.Price + prevSession.Overprice);
        }

        //    [Route("/login")]
        //    [HttpGet]
        //    public IActionResult Login(string? returnUrl)
        //    {
        //        return View();
        ////        Console.WriteLine(returnUrl ?? "N");
        ////        return Response.WriteAsync(@"<!DOCTYPE html>
        ////<html>
        ////<head>
        ////    <meta charset='utf-8' />
        ////    <title>METANIT.COM</title>
        ////</head>
        ////<body>
        ////    <h2>Login Form</h2>
        ////    <form method='post'>
        ////        <p>
        ////            <label>Email</label><br />
        ////            <input type='email' name='email' />
        ////        </p>
        ////        <p>
        ////            <label>Password</label><br />
        ////            <input type='password' name='hashedPassword' />
        ////        </p>
        ////        <input type='submit' value='Login' />
        ////    </form>
        ////</body>
        ////</html>");
        //    }

        //    [Route("/login")]
        //    [HttpPost]
        //    public IActionResult LoginPost(string? email, string? hashedPassword)
        //    {
        //        if (email is null || hashedPassword is null)
        //            return Unauthorized();
        //        List<Claim> claims = new List<Claim> { new Claim(ClaimTypes.Email, email), new Claim("Password", hashedPassword) };
        //        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        //        HttpContext.SignInAsync(claimsPrincipal);
        //        System.Security.Principal.IIdentity? user = User.Identity;
        //        //Console.WriteLine(user?.IsAuthenticated.ToString() ?? "NOOOOOOO");
        //        Console.WriteLine($"Login: {email}\nPassword: {hashedPassword}");
        //        return LocalRedirect("/");
        //    }

        [Route("/checkAuth")]
        public Task CheckAuthorization()
        {
            //List<string> s = new List<string>();
            //string result = String.Join(", ", s);
            //string res = '[' + result + ']';
            //Console.WriteLine(res);

            //Console.WriteLine(new DateOnly(2025, 6, 21).ToDateTime(new TimeOnly()) == DateTime.Today);

            //Console.WriteLine(new DateOnly(2025, 12, 21));

            return Response.WriteAsync($@"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>METANIT.COM</title>
    </head>
    <body>
        <h2>Login Form</h2>
        <br />
            {User.Identity?.IsAuthenticated ?? false}
        <br />
            {User.Claims.ToArray().Length}
    </body>
    </html>");

            /* Этот метод отправляет json вида
            {
                isAuthed : bool,
                email : email (Если пользователь авторизован иначе null)
            }

            */
        }

        //    [Route("/denied")]
        //    public IActionResult AccessDenied()
        //    {
        //        return Unauthorized();
        //    }

        //    [Route("/logout")]
        //    [Authorize]
        //    public IActionResult Logout()
        //    {
        //        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //        return LocalRedirect("/");
        //    }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[NonAction]
        //[Route("/tm/{offset}")]
        //public void WeekDaysForTiming(int offset)
        //{
        //    Console.WriteLine("Started");
        //    if (offset > 6)
        //        return;

        //    List<string> weekDays = new List<string>() { "Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
        //    List<string> result = new List<string>();

        //    for (int i = offset, j = 0; i < offset || j < 1; i++)
        //    {
        //        result.Add(weekDays[i]);
        //        if (i == weekDays.Count - 1)
        //        {
        //            i = -1;
        //            j++;
        //        }

        //        if (j > 3)
        //            break;
        //    }

        //    result.Add(result[0]);

        //    Console.WriteLine("Success?");

        //    foreach(string day in result)
        //        Console.WriteLine(day);
        //}

        //[NonAction]
        //private bool ValidateEmail(string email)
        //{
        //    //Console.WriteLine(new MailAddress(email).Address);
        //    try
        //    {
        //        string address = new MailAddress(email).Address;
        //        return address == email.Trim();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        [NonAction]
        //[Route("/pwd")]
        public string GeneratePassword()
        {
            ReadOnlySpan<char> text = new ReadOnlySpan<char>(("abcdefghijklmnopqrstuvwxyz".ToUpper() + "abcdefghijklmnopqrstuvwxyz" + "1234567890!@#$%^&*-=_/.,<>?~").ToCharArray());
            string password = RandomNumberGenerator.GetString(text, RandomNumberGenerator.GetInt32(10, 15));
            Console.WriteLine($"Password: {password}");
            return password;
        }
    }
}
