using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Session
    {
        public Session(int id, DateTime date, int hall, int movie, TimeSpan startTime, TimeSpan endTime, decimal overprice)
        {
            ID = id;
            Date = DateOnly.FromDateTime(date);
            Hall = hall;
            Movie = movie;
            StartTime = TimeOnly.FromTimeSpan(startTime);
            EndTime = TimeOnly.FromTimeSpan(endTime);
            Overprice = overprice;
        }

        private Session(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public DateOnly Date { get; set; }

        public int Hall {  get; set; }

        public int Movie {  get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal Overprice { get; set; }

        public static readonly Session Default = new Session(-1);

        public static List<Session> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Session> list = new List<Session>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new Session(reader.GetInt32(0), reader.GetDateTime(1), reader.GetInt32(2), 
                            reader.GetInt32(3), reader.GetTimeSpan(4), reader.GetTimeSpan(5), reader.GetDecimal(6)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Session left, Session right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Session left, Session right)
        {
            return !(left == right);
        }
    }
}
