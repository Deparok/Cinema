using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Hall
    {
        public Hall(int id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
        }

        private Hall(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public static readonly Hall Default = new Hall(-1);

        public static List<Hall> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Hall> list = new List<Hall>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new Hall(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Hall left, Hall right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Hall left, Hall right)
        {
            return !(left == right);
        }
    }
}
