using System.Collections;
using System.Runtime.InteropServices;
using WebApplicationCinema.Models.DBInstances;

namespace WebApplicationCinema.Models
{
    public enum SeatState
    {
        None = 0, Free = 1, Booked = 2, Bought = 3
    }

    public class SeatModel
    {
        public SeatModel(int row, int number, SeatState state)
        {
            Row = row;
            Number = number;
            InternalNumber = number;
            State = state;
        }

        public int Row { get; set; }

        public int Number { get; set; }

        public int InternalNumber { get; private set; }

        public SeatState State { get; set; }

        public string RusState => ToRusState(State);

        public static string ToRusState(SeatState state)
        {
            switch (state)
            {
                case SeatState.Free:
                    return "Свободно";
                case SeatState.Booked:
                    return "Бронь";
                case SeatState.Bought:
                    return "Покупка";
                default:
                    return "";
            }
        }

        public override string ToString()
        {
            return $"{Row}-{Number}";
        }

        public static List<List<SeatModel>> TESTSEATTABLE()
        {
            List<List<SeatModel>> seats = new List<List<SeatModel>>();

            for (int i = 0; i < new Random().Next(1, 11); i++)
            {
                List<SeatModel> row = new List<SeatModel>();

                for (int j = 0; j < new Random().Next(1, 21); j++)
                {
                    row.Add(new SeatModel(i, j, (SeatState)new Random().Next(0, 4)));
                }
                seats.Add(row);
            }
            seats = Normalize(seats);
            Console.WriteLine(seats.Count);
            foreach(List<SeatModel> seat in seats)
                Console.WriteLine(seat.Count);
            return seats;
        }

        public static List<List<SeatModel>> FormSeatTable(List<Seat> allSeats, List<SeatModel> ocupiedSeats)
        {
            List<SeatModel> _allSeats = allSeats.Select(seat => new SeatModel(seat.Row, seat.Number, SeatState.Free)).ToList();
            List<List<SeatModel>> seatTable = new List<List<SeatModel>>();
            IEnumerable<SeatModel> freeSeats = _allSeats.ExceptBy(ocupiedSeats.Select(seat => Tuple.Create(seat.Row, seat.Number)), free => Tuple.Create(free.Row, free.Number));
            List<SeatModel> resulting = new List<SeatModel>();
            for (int i = 1; i < allSeats.Max(seat => seat.Row) + 1; i++)
            {
                resulting = new List<SeatModel>(freeSeats.Where(seat => seat.Row == i));
                resulting.AddRange(ocupiedSeats.Where(seat => seat.Row == i));
            }
            seatTable.Add(resulting);
            return seatTable;
        }

        public static List<List<SeatModel>> FormNormalizedSeatTable(List<Seat> allSeats, List<SeatModel> ocupiedSeats)
        {
            return Normalize(FormSeatTable(allSeats, ocupiedSeats));
        }

        public static List<List<SeatModel>> Normalize(List<List<SeatModel>> seatTable)
        {
            List<List<SeatModel>> normalized = new List<List<SeatModel>>();
            foreach (List<SeatModel> row in seatTable)
            {
                normalized.Add(NormalizeList(row));
            }
            return normalized;
        }

        private static List<SeatModel> NormalizeList(List<SeatModel> collection) 
        {
            List<SeatModel> orderedCollection = collection.OrderBy(seat => seat.InternalNumber).ToList();

            if (orderedCollection.Count == 20)
                return orderedCollection;

            List<SeatModel> normalized = new List<SeatModel>();

            int secondToAdd = 20 - (10 - (orderedCollection.Count / 2));
            int firstToAdd = 10 - (orderedCollection.Count - (orderedCollection.Count / 2));

            for (int i = 0; i < 20; i++)
            {
                if (i < firstToAdd || i >= secondToAdd)
                    normalized.Add(new SeatModel(orderedCollection[0].Row, i, SeatState.None));
                else
                {
                    SeatModel seat = orderedCollection[i - firstToAdd];
                    seat.InternalNumber = i;
                    normalized.Add(seat);
                }
            }

            return normalized;
        }

        //public static bool operator >(Seat left, Seat right)
        //{
        //    if (left.Row == right.Row)
        //        return left.Column > right.Column;
        //    return left.Row > right.Row;
        //}

        //public static bool operator <(Seat left, Seat right)
        //{
        //    if (left.Row == right.Row)
        //        return left.Column < right.Column;
        //    return left.Row < right.Row;
        //}
    }
}
