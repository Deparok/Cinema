using System.Security.Claims;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.ViewModels
{
    public class TimingViewModel : BaseViewModel
    {
        public TimingViewModel(string title, ClaimsPrincipal user, Movie movie, List<Session> sessions) : base(title, user)
        {
            Movie = movie;

            List<IGrouping<DateOnly, Session>> grouped = sessions
                //.Where((Session ses) => ses.Date >= DateOnly.FromDateTime(DateTime.Today) && ses.StartTime > TimeOnly.FromDateTime(DateTime.Now + new TimeSpan(0, 15, 0)))
                .GroupBy((Session ses) => ses.Date)
                .OrderBy((IGrouping<DateOnly, Session> gr) => gr.Key)
                .ToList();
            
            if (grouped.Count >= 7)
                Sessions = grouped.Slice(0, 7);
            Sessions = grouped;
            //grouped
            //for (int i = 0; i < Sessions.Count; i++)
            //{
            //    Console.WriteLine(Sessions[i].Key);
            //    foreach (Session ses in Sessions[i])
            //        Console.WriteLine(ses.Id);
            //}

            //foreach (IGrouping<DateOnly, Session> some in sessions.GroupBy((Session ses) => ses.Date))
            //{
            //    Console.WriteLine(some.Key);
            //    foreach (Session ses in some)
            //        Console.WriteLine(ses.Id);
            //}
        }

        public Movie Movie { get; set; }

        public List<IGrouping<DateOnly, Session>> Sessions { get; set; }

        //public List<string> Timings { get; set; }
    }
}
