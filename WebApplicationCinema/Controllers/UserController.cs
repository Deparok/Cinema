using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using WebApplicationCinema.Models;
using WebApplicationCinema.Models.DBInstances;
using WebApplicationCinema.ViewModels;

namespace WebApplicationCinema.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<AnonymousController> _logger;
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IAsyncDBWorker _db;

        public UserController(ILogger<AnonymousController> logger, IWebHostEnvironment env, [FromKeyedServices("user")] IAsyncDBWorker db)
        {
            _logger = logger;
            _webHostEnv = env;
            _db = db;
        }

        [Authorize]
        [Route("/profile")]
        public async Task<IActionResult> Profile(bool? d)
        {
            if (User.IsInRole("Admin"))
                return LocalRedirect("/panel");

            int userId = int.Parse(User.Claims.Where(claim => claim.Type == ClaimTypes.Sid).First().Value);
            List<Ticket> tickets = await _db.ExecuteProcedureAsync<Ticket>("GetTicketsByUser", userId.ToString());

            //List<TicketNSeatModel> ticketsNSeats = new List<TicketNSeatModel>();
            //foreach ((Ticket ticket, Seat seat) tsTuple in )
            //    ticketsNSeats.Add(new TicketNSeatModel(tsTuple.ticket, tsTuple.seat));

            List<IGrouping<(Session, Movie), Ticket>> res = tickets
                                                                        .OrderBy(ticket => ticket.Seat)
                                                                        //.Zip(seats.OrderBy(seat => seat.ID))
                                                                        .GroupBy(ticket => (First: _db.ExecuteQuery<Session>($"SELECT * FROM Seats WHERE ID={ticket.Session}").First(), Second: _db.ExecuteQuery<Movie>($"SELECT * FROM Movies AS m JOIN [Sessions] AS s ON s.Movie=m.ID WHERE s.ID={ticket.Session}").First()))
                                                                        .ToList();
            return View(new ProfileViewModel("profile", User, res));

            //List<Session> sessions = new List<Session>();
            //List<Movie> movies = new List<Movie>();
            //for (int i = 0; i < groupedTicketsNSeats.Count; i++)
            //{
            //    movies.Add((await _db.ExecuteQueryAsync<Movie>($"SELECT * FROM Movies AS m JOIN [Sessions] AS s ON s.Movie=m.ID WHERE s.ID={groupedTicketsNSeats[i].Key}")).First());
            //}
            //IEnumerable<(IGrouping<Session, (Ticket, Seat)>, Movie)> res = groupedTicketsNSeats.Zip(movies);
            //if (d ?? false)
            //await Response.WriteAsync("Все ок");
            //return View(new ProfileViewModel("profile", User));
        }
    }
}
