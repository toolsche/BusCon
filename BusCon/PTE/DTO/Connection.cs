using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Linq;
using System.Text;

namespace BusCon.PTE.DTO
{
    public class Connections : ObservableCollection<Connection>
    {
    }

    public sealed class Connection
    {
        public int OriginId { get; set; }
        public int DestinationId { get; set; }
        public DateTime Time { get; set; }

        public string Id { get; set; }
        public string PublicDuration { get; set; }
        public string Link { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public Location From { get; set; }
        public Location To { get; set; }
        public List<Connection.Part> Parts { get; set; }
        public List<IFare> Fares { get; set; }

        #region Constructors

        public Connection()
        {
        }

        public Connection(int origin, int destination, DateTime time)
        {
            this.OriginId = origin;
            this.DestinationId = destination;
            this.Time = time;
        }

        // old
        public Connection(string id, string publicDuration, DateTime departureTime, DateTime arrivalTime, Location from, Location to, List<Connection.Part> parts, List<IFare> fares)
        {
            this.Id = id;
            this.PublicDuration = publicDuration;
            this.DepartureTime = departureTime;
            this.ArrivalTime = arrivalTime;
            this.From = from;
            this.To = to;
            this.Parts = parts;
            this.Fares = fares;
        }

        //public Connection(string id, string link, DateTime departureTime, DateTime arrivalTime, Location from, Location to, List<Connection.Part> parts, List<IFare> fares)
        //{
        //    this.Id = id;
        //    this.Link = link;
        //    this.DepartureTime = departureTime;
        //    this.ArrivalTime = arrivalTime;
        //    this.From = from;
        //    this.To = to;
        //    this.Parts = parts;
        //    this.Fares = fares;
        //}

        #endregion

        #region DoNotExport

        [XmlIgnore]
        public TimeSpan Duration
        {
            get
            {
                return this.ArrivalTime - this.DepartureTime;
            }
        }

        [XmlIgnore]
        public int NumberChanges
        {
            get
            {
                return this.Parts.Count - 1;
            }
        }

        [XmlIgnore]
        public List<Connection.Part> SummaryParts
        {
            get
            {
                return Enumerable.ToList<Connection.Part>(Enumerable.Where<Connection.Part>((IEnumerable<Connection.Part>)this.Parts, (Func<Connection.Part, bool>)(p => !(p is Connection.SpecialTripPart))));
            }
        }

        #endregion

        public override string ToString()
        {
            string format = "HH:mm";
            return this.Id + " " + this.DepartureTime.ToString(format) + "-" + this.ArrivalTime.ToString(format);
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;
            if (!(o is Connection))
                return false;
            else
                return this.Id.Equals(((Connection)o).Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }


        /// <summary>
        /// PART
        /// </summary>
        [XmlInclude(typeof(Connection.Trip))]
        [XmlInclude(typeof(Connection.SpecialTripPart))]
        [XmlInclude(typeof(Connection.Footway))]
        public abstract class Part
        {
            [XmlIgnore]
            public List<Point> path;

            public Location Departure { get; set; }

            public Location Arrival { get; set; }

            //public abstract string TypeIcon { get; }

            public Part(Location departure, Location arrival, List<Point> path)
            {
                this.Departure = departure;
                this.Arrival = arrival;
                this.path = path;
            }

            public Part()
            {
            }
        }


        /// <summary>
        /// TRIP
        /// </summary>
        public sealed class Trip : Connection.Part
        {
            public Line Line { get; set; }

            public Location Destination { get; set; }

            public DateTime DepartureTime { get; set; }

            public string DeparturePosition { get; set; }

            public DateTime ArrivalTime { get; set; }

            public string ArrivalPosition { get; set; }

            public List<Stop> IntermediateStops { get; set; }

            public Trip(Line line, Location destination, DateTime departureTime, string departurePosition, Location departure, DateTime arrivalTime,
                string arrivalPosition, Location arrival, List<Stop> intermediateStops, List<Point> path)
                : base(departure, arrival, path)
            {
                this.Line = line;
                this.Destination = destination;
                this.DepartureTime = departureTime;
                this.DeparturePosition = departurePosition;
                this.ArrivalTime = arrivalTime;
                this.ArrivalPosition = arrivalPosition;
                this.IntermediateStops = intermediateStops;
            }

            public Trip()
            {
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder(this.GetType().Name + "[");
                stringBuilder.Append("line=").Append((object)this.Line);
                stringBuilder.Append(",");
                stringBuilder.Append("destination=").Append(this.Destination.ToDebugString());
                stringBuilder.Append(",");
                stringBuilder.Append("departure=").Append((object)this.DepartureTime).Append("/").Append(this.DeparturePosition).Append("/").Append(this.Departure.ToDebugString());
                stringBuilder.Append(",");
                stringBuilder.Append("arrival=").Append((object)this.ArrivalTime).Append("/").Append(this.ArrivalPosition).Append("/").Append(this.Arrival.ToDebugString());
                stringBuilder.Append("]");
                return ((object)stringBuilder).ToString();
            }
        }

        /// <summary>
        /// FOOTWAY
        /// </summary>
        public sealed class Footway : Connection.Part
        {
            public int Min { get; set; }

            //[XmlIgnore]
            //public override string TypeIcon
            //{
            //    get
            //    {
            //        return "/Images/walk.png";
            //    }
            //}

            public DateTime ArrivalTime { get; set; }

            public DateTime DepartureTime { get; set; }

            public Footway(int min, Location departure, DateTime departureTime, Location arrival, DateTime arrivalTime, List<Point> path)
                : base(departure, arrival, path)
            {
                this.Min = min;
                this.ArrivalTime = arrivalTime;
                this.DepartureTime = departureTime;
            }

            public Footway()
            {
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder(this.GetType().Name + "[");
                stringBuilder.Append("min=").Append(this.Min);
                stringBuilder.Append(",");
                stringBuilder.Append("departure=").Append(this.Departure.ToDebugString());
                stringBuilder.Append(",");
                stringBuilder.Append("arrival=").Append(this.Arrival.ToDebugString());
                stringBuilder.Append("]");
                return ((object)stringBuilder).ToString();
            }
        }


        /// <summary>
        /// SPECIALTRIPPART
        /// </summary>
        public sealed class SpecialTripPart : Connection.Part
        {
            public string Name { get; set; }

            //[XmlIgnore]
            //public override string TypeIcon
            //{
            //    get
            //    {
            //        return string.Empty;
            //    }
            //}

            public SpecialTripPart(string name, Location departure, Location arrival, List<Point> path)
                : base(departure, arrival, path)
            {
                this.Name = name;
            }

            public SpecialTripPart()
            {
            }
        }
    }
}