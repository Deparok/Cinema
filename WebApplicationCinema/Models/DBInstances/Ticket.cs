using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Ticket
    {
        public Ticket(int id, int session, int seat, int status)
        {
            ID = id;
            Session = session;
            Seat = seat;
            Status = status;
        }

        private Ticket(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public int Session { get; set; }

        public int Seat { get; set; }

        public int Status { get; set; }

        public static readonly Ticket Default = new Ticket(-1);

        public static List<Ticket> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Ticket> list = new List<Ticket>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new Ticket(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Ticket left, Ticket right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Ticket left, Ticket right)
        {
            return !(left == right);
        }
    }
}
