using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Seat
    {
        public Seat(int id, int hall, int row, int number)
        {
            ID = id;
            Hall = hall;
            Row = row;
            Number = number;
        }

        private Seat(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public int Hall { get; set; }

        public int Row {  get; set; }

        public int Number {  get; set; }

        public static readonly Seat Default = new Seat(-1);

        public static List<Seat> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Seat> list = new List<Seat>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new Seat(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Seat left, Seat right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Seat left, Seat right)
        {
            return !(left == right);
        }
    }
}
