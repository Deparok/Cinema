using System.Security.Claims;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public ProfileViewModel(string title, ClaimsPrincipal user, List<IGrouping<(Session, Movie), Ticket>> orderedMonster, bool defaultt = false) : base(title, user)
        {
            OrderedMonster = orderedMonster;
            //if (orderedMovies is null)
            //    OrderedMovies = new List<Movie>();
            //if (!defaultt)
            //    OrderedMovies = orderedMovies;
            //else
            //    OrderedMovies = new List<Movie>() { new Movie(), new Movie(), new Movie() };
        }

        //public ProfileViewModel(string title, ClaimsPrincipal user) : base(title, user)
        //{
        //    OrderedMovies = new List<Movie>();
        //}

        public bool HasMovies => OrderedMonster.Count != 0;

        public List<IGrouping<(Session, Movie), Ticket>> OrderedMonster { get; set; }

        public int SectionLength => 374 * (OrderedMonster.Count > 1 ? OrderedMonster.Count : 2);

        public int RepeaterLength => 355 * (OrderedMonster.Count > 1 ? OrderedMonster.Count : 2);
    }
}
