using System.Security.Claims;
using WebApplicationCinema.Models;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.ViewModels
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(string title, ClaimsPrincipal user, List<Movie> movies) : base(title, user)
        {
            Movies = movies; 
        }

        public List<Movie> Movies {  get; set; }
    }
}
