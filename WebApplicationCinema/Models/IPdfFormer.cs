namespace WebApplicationCinema.Models
{
    public interface IPdfFormer
    {
        byte[] GenerateFileBytes(string movieName, string hallName, DateOnly date, TimeOnly startTime, List<SeatModel> seats, decimal price);

        MemoryStream GenerateFileStream(string movieName, string hallName, DateOnly date, TimeOnly startTime, List<SeatModel> seats, decimal price);
    }
}
