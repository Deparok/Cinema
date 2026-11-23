using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class User
    {
        public User(int id, string email, string password, int role, bool isActive)
        {
            ID = id;
            Email = email;
            Password = password;
            Role = role;
            IsActive = isActive;
        }

        private User(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int Role { get; set; }

        public bool IsActive { get; set; }

        public static readonly User Default = new User(-1);

        public static List<User> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<User> list = new List<User>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetBoolean(4)));
                    }
                }
                return list;
            }
        }

        public Role GetRole(List<Role> roles) => roles.Where(role => role.ID == Role).First();

        public static bool operator ==(User left, User right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(User left, User right)
        {
            return !(left == right);
        }
    }
}
