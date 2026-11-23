using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.Models
{
    public class PdfFormer : IPdfFormer
    {
        public PdfFormer(string title)
        {
            Title = title;
        }

        public string Title { get; set; }

        public MemoryStream GenerateFileStream(string movieName, string hallName, DateOnly date, TimeOnly startTime, List<SeatModel> seats, decimal price)
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = new PdfWriter(stream, new WriterProperties() { });
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            document.SetFont(PdfFontFactory.CreateFont("C:\\Windows\\Fonts\\arial.ttf", PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED));

            Paragraph header = new Paragraph(Title)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(20)
            .SimulateBold();
            document.Add(header);

            //Paragraph orderInfo = new Paragraph()
            //.Add($"Фильм: {movieName}\n")
            //.Add($"Зал: {hallName}\n")
            //.Add($"Дата: {date}\n")
            //.Add($"Время: {startTime}\n")
            //.Add($"Количество билетов: {seats.Count}\n")
            //.Add($"Стоимость билета: {price} руб.")
            //.SetTextAlignment(TextAlignment.LEFT)
            //.SetFontSize(12)
            //.SetMarginBottom(20);
            //document.Add(orderInfo);

            for (int i = 0; i < seats.Count; i++)
            {
                if (i > 0)
                {
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                }
                CreateTicket(document, movieName, hallName, seats[i].Row, seats[i].Number, seats[i].RusState, price, date, startTime, i + 1);
            }

            document.Close();   // Закрывает документ раньше чем отдает файловый поток, из-за этого поток отдается пустым,
                                // ну и все это разворачивается в ошибки типа "Can not access closed stream"
            return stream;
        }

        public byte[] GenerateFileBytes(string movieName, string hallName, DateOnly date, TimeOnly startTime, List<SeatModel> seats, decimal price)
        {
            return GenerateFileStream(movieName, hallName, date, startTime, seats, price).ToArray();
        }

        private void CreateTicket(Document document, string movieName, string hall, int row, int seat, string seatType, decimal price, DateOnly date, TimeOnly time, int ticketNumber)
        {
            Div ticket = new Div()
                .SetBorder(new iText.Layout.Borders.SolidBorder(1))
                .SetPadding(20)
                .SetMarginBottom(20);

            Paragraph ticketHeader = new Paragraph($"Билет №{ticketNumber}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16)
                .SimulateBold();
            ticket.Add(ticketHeader);

            Paragraph movieInfo = new Paragraph()
                .Add($"Фильм: {movieName}\n")
                .Add($"Зал: {hall}\n")
                .Add($"Ряд: {row}\n")
                .Add($"Место: {seat}\n")
                .Add($"Дата: {date}\n")
                .Add($"Время: {time}\n")
                .Add($"Тип: {seatType}\n")
                .Add($"Цена: {price} руб.")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(12);
            ticket.Add(movieInfo);

            //Paragraph qrPlaceholder = new Paragraph("QR-код для входа")
            //    .SetTextAlignment(TextAlignment.CENTER)
            //    .SetFontSize(10)
            //    .SimulateItalic()
            //    .SetMarginTop(10);
            //ticket.Add(qrPlaceholder);

            document.Add(ticket);
        }
    }
}
