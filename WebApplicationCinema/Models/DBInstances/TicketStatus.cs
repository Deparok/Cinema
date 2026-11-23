using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class TicketStatus
    {
        public TicketStatus(int id, string name)
        {
            ID = id;
            Name = name;
        }

        private TicketStatus(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public static readonly TicketStatus Drfault = new TicketStatus(-1);

        public static List<TicketStatus> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<TicketStatus> list = new List<TicketStatus>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new TicketStatus(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(TicketStatus left, TicketStatus right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(TicketStatus left, TicketStatus right)
        {
            return !(left == right);
        }
    }
}
