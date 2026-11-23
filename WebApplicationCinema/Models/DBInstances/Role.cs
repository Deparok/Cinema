using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Role
    {
        public Role(int id, string name)
        {
            ID = id;
            Name = name;
        }

        private Role(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public static readonly Role Default = new Role(-1);

        public static List<Role> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Role> list = new List<Role>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new Role(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Role left, Role right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Role left, Role right)
        {
            return !(left == right);
        }
    }
}
