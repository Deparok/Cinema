using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;

namespace WebApplicationCinema.Models
{
    public interface IDBWorker : IDisposable
    {
        bool ReConnect();

        //bool Connect(string connectionString); IPAddress ip, string dbName, string login, string passwd

        //bool Connect(IPAddress ip, string dbName, string login, string passwd);

        ConnectionState CheckConnection();

        SqlDataReader ExecuteQuery(string query);

        List<T> ExecuteQuery<T>(string query);

        SqlDataReader ExecuteProcedure(string name, params string[] parameters);

        List<T> ExecuteProcedure<T>(string name, params string[] parameters);

        void AutoReconnect();

        void AutoReconnect(int reconnectionsAmount);

        //void Disconnect();
    }
}
