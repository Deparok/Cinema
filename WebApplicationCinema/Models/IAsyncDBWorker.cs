using Microsoft.Data.SqlClient;
using System.Net;

namespace WebApplicationCinema.Models
{
    public interface IAsyncDBWorker : IDBWorker
    {
        delegate void SenderNSuccessState(object sender, bool isSuccessful);
        delegate void QueryResults(object sender, SqlDataReader reader);
        public delegate void SenderOnly(object sender);

        event SenderNSuccessState OnConnected;
        event QueryResults OnQueryExecuted;
        event SenderOnly OnDisconnected;

        Task<bool> ReConnectAsync();

        //Task<bool> ConnectAsync(string connectionString); IPAddress ip, string dbName, string login, string passwd

        //Task<bool> ConnectAsync(IPAddress ip, string dbName, string login, string passwd);

        Task<SqlDataReader> ExecuteQueryAsync(string query);

        Task<List<T>> ExecuteQueryAsync<T>(string query);

        Task<SqlDataReader> ExecuteProcedureAsync(string name, params string[] parameters);

        Task<List<T>> ExecuteProcedureAsync<T>(string name, params string[] parameters);

        //Task DisconnectAsync();
    }
}
