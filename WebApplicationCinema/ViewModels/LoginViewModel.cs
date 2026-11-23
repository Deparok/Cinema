using System.Security.Claims;

namespace WebApplicationCinema.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(string title, ClaimsPrincipal user, bool userExists = true) : base(title, user)
        {
            IsAuthenticeted = user.Identity?.IsAuthenticated ?? false;
            UserExists = userExists;
        }

        public bool IsAuthenticeted { get; set; }

        public bool UserExists { get; set; }
    }
}
