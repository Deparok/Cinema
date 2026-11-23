using System.Security.Claims;

namespace WebApplicationCinema.ViewModels
{
    public class BaseViewModel
    {
        public BaseViewModel(string title, ClaimsPrincipal user)
        {
            string loginPath = "/login";
            string loginText = "Войти";
            string logoutPath = "/logout";
            string logoutText = "Выйти";

            string profilePath = "/profile";
            string profileText = "Профиль";
            string panelPath = "/panel";
            string panelText = "Администрирование";

            string adminRoleName = "Admin";

            Title = title;
            if (user.Identity?.IsAuthenticated ?? false)
            {
                Login_outPage = logoutPath;
                Login_outText = logoutText;
            }
            else
            {
                Login_outPage = loginPath;
                Login_outText = loginText;
            }

            if (user.IsInRole(adminRoleName))
            {
                Profile_panelPage = panelPath;
                Profile_panelText = panelText;
            }
            else
            {
                Profile_panelPage = profilePath;
                Profile_panelText = profileText;
            }
        }

        public string Title { get; set; }

        public string Login_outPage { get; set; }

        public string Login_outText {  get; set; }

        public string Profile_panelPage { get; set; }

        public string Profile_panelText { get; set; }
    }
}
