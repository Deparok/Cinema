using System.Security.Claims;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.ViewModels
{
    public class PanelViewModel : BaseViewModel
    {
        public PanelViewModel(string title, ClaimsPrincipal user, List<Hall> halls, List<Movie> movies, List<Session> sessions) : base(title, user)
        {
            Halls = halls;
            Movies = movies;
            Sessions = sessions;
        }

        public List<Hall> Halls {  get; set; }

        public List<Movie> Movies { get; set; }

        public List<Session> Sessions { get; set; }

        public IEnumerable<(Session, Hall, Movie)> SessionHallMovie => Sessions.OrderBy(session => Tuple.Create(session.Hall, session.Movie)).Zip(Halls.OrderBy(hall => hall.ID), Movies.OrderBy(movie => movie.ID));
    }
}
