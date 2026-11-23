using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Payment
    {
        public Payment(int id, int ticket, decimal amount, int processedBy)
        {
            ID = id;
            Ticket = ticket;
            Amount = amount;
            ProcessedBy = processedBy;
        }

        private Payment(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public int Ticket { get; set; }

        public decimal Amount { get; set; }

        public int ProcessedBy { get; set; }

        public static readonly Payment Default = new Payment(-1);

        public static List<Payment> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Payment> list = new List<Payment>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new Payment(reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2), reader.GetInt32(3)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Payment left, Payment right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Payment left, Payment right)
        {
            return !(left == right);
        }
    }
}
