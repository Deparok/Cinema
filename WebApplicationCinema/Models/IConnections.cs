namespace WebApplicationCinema.Models
{
    public interface IUserConnection : IDBWorker
    {
    }

    public interface IAdminConnection : IDBWorker
    {
    }

    public interface IAsyncUserConnection : IAsyncDBWorker
    {
    }

    public interface IAsyncAdminConnection : IAsyncDBWorker
    {
    }
}
