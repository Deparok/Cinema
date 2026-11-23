using Microsoft.Data.SqlClient;

namespace WebApplicationCinema.Models.DBInstances
{
    public class Movie
    {
        public Movie()
        {
            
        }

        public Movie(int iD, string name, int runtime, decimal price, int year, string director, DateTime runtimeStart, DateTime runtimeStop, string posterPath, string rating, string description)
        {
            ID = iD;
            Name = name;
            Runtime = runtime;
            Price = price;
            Year = year;
            Director = director;
            RuntimeStart = DateOnly.FromDateTime(runtimeStart);
            RuntimeStop = DateOnly.FromDateTime(runtimeStop);
            PosterPath = posterPath;
            Rating = rating;
            Description = description;
        }

        private Movie(int id)
        {
            ID = id;
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public int Runtime { get; set; }

        public decimal Price { get; set; }

        public int Year { get; set; }

        public string Director { get; set; }

        public DateOnly RuntimeStart { get; set; }

        public DateOnly RuntimeStop { get; set; }

        public string PosterPath { get; set; }

        public string Rating { get; set; }

        public string Description { get; set; }

        public static readonly Movie Default = new Movie(-1);

        public static List<Movie> Parse(SqlDataReader reader)
        {
            using (reader)
            {
                List<Movie> list = new List<Movie>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        list.Add(new Movie(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetDecimal(3),
                            reader.GetInt32(4), reader.GetString(5), reader.GetDateTime(6), reader.GetDateTime(7), 
                            reader.GetString(8), reader.GetString(9), reader.GetString(10)));
                    }
                }
                return list;
            }
        }

        public static bool operator ==(Movie left, Movie right)
        {
            return left.ID == right.ID;
        }

        public static bool operator !=(Movie left, Movie right)
        {
            return !(left == right);
        }

    }
}
