using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace WebApplicationCinema.Models
{
    public class MSSQLConnector : IDBWorker, IAsyncDBWorker
    {
        //public delegate void SenderNSuccessState(object sender, bool isSuccessful);
        ////delegate void QueryResults(object sender, SqlDataReader reader);
        //public delegate void SenderOnly(object sender);

        public event IAsyncDBWorker.SenderNSuccessState OnConnected;
        public event IAsyncDBWorker.QueryResults OnQueryExecuted;
        public event IAsyncDBWorker.SenderOnly OnDisconnected;

        public MSSQLConnector(IPAddress ip, string? dbName, string? login, string? passwd)
        {
            if (ip == IPAddress.None || ip == IPAddress.Any || ip == IPAddress.Broadcast)
                ip = IPAddress.Loopback;

            _connector = new SqlConnection($"Server={ip};Database={dbName ?? "master"};User Id={login ?? _nLogin};Password={passwd ?? _nPwd};TrustServerCertificate=True;");
            _connectionString = _connector.ConnectionString;
            _command = _connector.CreateCommand();
        }

        public MSSQLConnector(byte[] ip, string dbName, string login, string passwd) : this(new IPAddress(ip), dbName, login, passwd)
        {

        }

        public MSSQLConnector(string ip, string dbName, string login, string passwd) : this(IPAddress.Parse(ip), dbName, login, passwd)
        {

        }

        public MSSQLConnector(IPAddress ip, string dbName) : this(ip, dbName, null, null)
        {

        }

        public MSSQLConnector(byte[] ip, string dbName) : this(new IPAddress(ip), dbName)
        {

        }

        public MSSQLConnector(string ip, string dbName) : this(IPAddress.Parse(ip), dbName)
        {

        }

        public MSSQLConnector(IPAddress ip) : this(ip, null, null, null)
        {

        }

        public MSSQLConnector(byte[] ip) : this(new IPAddress(ip))
        {

        }

        public MSSQLConnector(string ip) : this(IPAddress.Parse(ip))
        {

        }

        private readonly SqlConnection _connector;
        private readonly SqlCommand _command;
        private SqlDataReader _reader;
        private readonly string _connectionString;
        private const string _nLogin = "LNONE";
        private const string _nPwd = "PNONE";
        private int _reconnectionsAmount = 5;
        private bool _connecting = false;
        private bool _executing = false;
        private bool _autoReconnect = false;
        private bool _standartClosure = false;

        private void _connector_StateChanged(object sender, StateChangeEventArgs e)
        {
            if ((e.CurrentState == ConnectionState.Broken || e.CurrentState == ConnectionState.Closed) && !_standartClosure)
            {
                int reconnectionsAmount = _reconnectionsAmount;
                //ForceConnecting();
                while (!ReConnect() && reconnectionsAmount != 0)
                    reconnectionsAmount--;
                //ConnectionCompleted();
            } 
        }

        private bool CheckEverythingOk()
        {
            return _connecting | _executing;
        }

        private void CheckEverythingOkfNotSleep()
        {
            while (!CheckEverythingOk())
                Thread.Sleep(250);
            _executing = true;
        }

        private void ExecutionCompleted()
        {
            _executing = false;
        }

        private void ForceConnecting()
        {
            _connecting = true;
        }

        private void ConnectionCompleted()
        {
            _connecting = false;
        }

        public bool Connect()
        {
            //ForceConnecting();
            bool result = false;
            try
            {
                //Console.WriteLine("Глубокая попытка открыть подключение");
                //Console.WriteLine(_connectionString);
                _connector.Open();
                result = true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                result = false;
            }
            OnConnected?.Invoke(this, result);
            //ConnectionCompleted();
            return result;
        }

        public bool Connect(IPAddress ip, string dbName, string login, string passwd)
        {
            _connector.ConnectionString = $"Server={ip};Database={dbName ?? "master"};User Id={login ?? _nLogin};Password={passwd ?? _nPwd};";
            return Connect();
        }

        public ConnectionState CheckConnection()
        {
            return _connector.State;
        }

        public bool ReConnect()
        {
            //ForceConnecting();
            try
            {
                _connector.Open();
                return true;
            }
            catch
            {
                return false;
            }
            //finally
            //{
            //    ConnectionCompleted(); 
            //}
        }

        public void AutoReconnect()
        {
            if (!_autoReconnect)
            {
                _connector.StateChange += _connector_StateChanged;
                _autoReconnect = true;
            }
        }

        public void AutoReconnect(int reconnectionsAmount)
        {
            _reconnectionsAmount = reconnectionsAmount;
            _connector.StateChange += _connector_StateChanged;
        }

        public async Task<bool> ConnectAsync()
        {
            //ForceConnecting();
            bool result = false;
            try
            {
                await _connector.OpenAsync();
                result = true;
            }
            catch
            {
                result = false;
            }
            OnConnected?.Invoke(this, result);
            //ConnectionCompleted();
            return result;
        }

        public async Task<bool> ConnectAsync(IPAddress ip, string dbName, string login, string passwd)
        {
            _connector.ConnectionString = $"Server={ip};Database={dbName ?? "master"};User Id={login ?? _nLogin};Password={passwd ?? _nPwd}";
            return await ConnectAsync();
        }
        
        public async Task<bool> ReConnectAsync()
        {
            return await Task.Run(ReConnect);
        }

        public List<T>? MapFromReader<T>(SqlDataReader reader)
        {
            ConstructorInfo? constructor = ValidConstructor<T>(reader);
            if (constructor is null)
                return null;

            List<T> result = new List<T>();
            object[] row = new object[reader.FieldCount];

            while (reader.Read())
            {
                reader.GetValues(row);
                result.Add((T)constructor.Invoke(row));
            }
            return result;
        }

        private ConstructorInfo? ValidConstructor<T>(SqlDataReader reader)
        {
            if (reader is null) 
                return null;

            bool validConstructor = true;
            foreach (ConstructorInfo constructor in typeof(T).GetConstructors())
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                if (parameters.Length != reader.FieldCount)
                    continue;
                validConstructor = true;
                //Type fieldType;
                //Type n = typeof(int);
                //Type u = typeof(ConstructorInfo);
                for (int i = 0; i < parameters.Length; i++)
                {
                    //fieldType = reader.GetFieldType(i);
                    validConstructor &= parameters[i].ParameterType == reader.GetFieldType(i);
                }
                if (validConstructor)
                    return constructor;
            }
            return null;
        }

        public SqlDataReader ExecuteQuery(string query)
        {
            //CheckEverythingOkSleepIfNotThenExecute();
            if (!_reader?.IsClosed ?? false)
                _reader.Close();
            _command.CommandText = query;
            SqlDataReader reader = null;
            try
            {
                reader = _command.ExecuteReader();
            }
            catch { }
            _reader = reader;
            OnQueryExecuted?.Invoke(this, reader);
            //ExecutionCompleted();
            return reader;
        }

        public List<T> ExecuteQuery<T>(string query)
        {
            return MapFromReader<T>(ExecuteQuery(query)) ?? new List<T>();
        }

        public async Task<SqlDataReader> ExecuteQueryAsync(string query)
        {
            //CheckEverythingOkSleepIfNotThenExecute();
            if (!_reader?.IsClosed ?? false)
                _reader.Close();
            _command.CommandText = query;
            SqlDataReader reader = null;
            try
            {
                reader = await _command.ExecuteReaderAsync();
            }
            catch { }
            _reader = reader;
            OnQueryExecuted?.Invoke(this, reader);
            //ExecutionCompleted();
            return reader;
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string query)
        {
            return MapFromReader<T>(await ExecuteQueryAsync(query)) ?? new List<T>();
        }

        public SqlDataReader ExecuteProcedure(string name, params string[] parameters)
        {
            //CheckEverythingOkSleepIfNotThenExecute();
            // DeleteMovie(@MovieID INT) ID or -1
            // DeleteHall(@HallID INT) ID or -1
            // DeleteSession(@SessionID INT) ID or -1
            // CreateUser(@Email NVARCHAR(255), @Password NVARCHAR(255), @Role INT = 1, @IsActive BIT = 1) 0 or -1
            // UserLogin(@Email NVARCHAR(255), @Password NVARCHAR(255)) User or -1 wrong pwd or -2 no such user
            // GetAllRuningMovies(@TodaysDate DATE) * Movies
            // GetSessionsByMovie(@MovieID INT) * Sessions
            // GetAllSeatsBySession(@SessionID INT) * Seats
            // GetOcupiedTicketsBySession(@SessionID INT) * Tickets
            // GetTicketsByUser(@UserID INT) * Tickets
            // GetSessionByTicket(@TicketID INT) * Sessions
            // GetMoviesByTicket(@TicketID INT) * Movies
            // GetHallByTicket(@TicketID INT) * Halls
            // CreateHall(@Name NVARCHAR(255), @Description NVARCHAR(2000) NULL)
            // CreateSeat(@HallID INT, @Row INT, @Number INT)
            // CreateMovie(@Name NVARCHAR(255), @Runtime INT, @Price MONEY, @Year INT NULL, @Director NVARCHAR(255),
            // @RuntimeStart DATE, @RuntimeStop DATE, @PosterPath NVARCHAR(512), @Rating NVARCHAR(255) NULL)
            // CreateSession(@Date DATE, @HallID INT, @MovieID INT, @StartTime TIME, @EndTime TIME, @Overprice MONEY NULL) 0 or -1


            StringBuilder sb = new StringBuilder();

            //Console.WriteLine($"{"N'" + String.Join("', N'", parameters)}" + "'");
            try
            {
                if (!(parameters is null))
                    return ExecuteQuery($"EXEC {name} {"N'" + String.Join("', N'", parameters)}" + "'");
                return ExecuteQuery($"EXEC {name}");
            }
            catch 
            {
                return _reader;
            }
        }

        public List<T> ExecuteProcedure<T>(string name, params string[] parameters)
        {
            return MapFromReader<T>(ExecuteProcedure(name, parameters)) ?? new List<T>();
        }

        public async Task<SqlDataReader> ExecuteProcedureAsync(string name, params string[] parameters)
        {
            return await Task.Run(() => ExecuteProcedure(name, parameters));
        }

        public async Task<List<T>> ExecuteProcedureAsync<T>(string name, params string[] parameters)
        {
            return MapFromReader<T>(await ExecuteProcedureAsync(name, parameters)) ?? new List<T>();
        }

        public void Disconnect()
        {
            if (_connector.State != ConnectionState.Closed)
                Dispose();
            OnDisconnected?.Invoke(this);
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(Disconnect);
        }

        public void Dispose()
        {
            lock(this)
            {
                _standartClosure = true;
                _connector?.Close();
                _connector?.Dispose();
                _standartClosure = false;
            }
        }
    }
}
