using System.Reflection;
using System.Security.Claims;
using WebApplicationCinema.Models;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.ViewModels
{
    public class PurchaseViewModel : BaseViewModel
    {
        public PurchaseViewModel(string title, ClaimsPrincipal user, Movie movie, Session session, List<List<SeatModel>> seatTable) : base(title, user)
        {
            Movie = movie;
            SeatTable = SeatModel.Normalize(seatTable);
            Session = session;
            if (user.Identity?.IsAuthenticated ?? false)
                UserEmail = user.Claims.Where((claim) => claim.Type == ClaimTypes.Email)
                .FirstOrDefault(new Claim(ClaimTypes.Email, "")).Value;
            TotalCost = movie.Price + session.Overprice;
        }

        public Movie Movie { get; set; }

        public Session Session { get; set; }

        public List<List<SeatModel>> SeatTable { get; set; }

        private bool _isNormalized = false;

        public string UserEmail { get; set; } = "";

        public decimal TotalCost { get; private set; }

        public PurchaseViewModel NormalizeSeats()
        {
            if (_isNormalized)
                return this;

            List<List<SeatModel>> normalized = new List<List<SeatModel>>();

            for (int i = 0; i < SeatTable.Count; i++)
            {
                List<SeatModel> seats = new List<SeatModel>();
                for (int j = 0; j < SeatTable[i].Count; j++)
                {
                    seats.Add(null);
                }
                normalized.Add(seats);
            }

            foreach (List<SeatModel> row in SeatTable)
            {
                foreach (SeatModel seat in row)
                {
                    //Console.WriteLine($"{seat.Row}-{seat.Column}");
                    normalized[seat.Row - 1][seat.Number - 1] = seat;
                }
            }

            //Console.WriteLine("AfterNormalizing");

            //foreach (List<Seat> row in normalized)
            //    foreach (Seat seat in row)
            //        Console.WriteLine($"{seat.Row}-{seat.Column}");

            //SeatTable.Sort();

            //foreach (List<Seat> row in SeatTable)
            //    foreach (Seat seat in row)
            //        Console.WriteLine($"{seat.Row}-{seat.Column}");

            SeatTable = normalized;
            _isNormalized = true;
            return this;
        }
    }
}
