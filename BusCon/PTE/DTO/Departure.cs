using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Text;

namespace BusCon.PTE.DTO
{
    public class Departures : ObservableCollection<Departure>
    {
    }

    public sealed class Departure
    {
        private readonly DateTime _plannedTime;

        private readonly DateTime _predictedTime;

        public readonly string lineLink;

        public readonly string position;

        public readonly int destinationId;

        public readonly string message;

        private bool _isRealTime;

        public int Countdown { get; set; }

        public string Line { get; set; }

        public string Destination { get; set; }


        public DateTime DepartureTime
        {
            get
            {
                if (this._isRealTime)
                    return this._predictedTime;
                else
                    return this._plannedTime;
            }
        }

        public Departure(DateTime? plannedTime, DateTime? predictedTime, int countdown, string line, string lineLink, string position, int destinationId, string destination, string message)
        {
            if (plannedTime.HasValue)
                this._plannedTime = plannedTime.Value;
            if (predictedTime.HasValue)
            {
                this._predictedTime = predictedTime.Value;
                this._isRealTime = true;
            }
            else
                this._isRealTime = false;
            this.Line = line;
            this.lineLink = lineLink;
            this.position = position;
            this.destinationId = destinationId;
            this.Destination = destination;
            this.message = message;
            this.Countdown = countdown;
        }

        public Departure(DateTime plannedTime, string line, string position, int destinationId, string destination)
        {
            this._plannedTime = plannedTime;
            this._predictedTime = DateTime.MinValue;
            this._isRealTime = false;
            this.Line = line;
            this.lineLink = (string)null;
            this.position = position;
            this.destinationId = destinationId;
            this.Destination = destination;
            this.message = (string)null;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Departure(");
            stringBuilder.Append(this._plannedTime.ToString());
            stringBuilder.Append(",");
            stringBuilder.Append(this._predictedTime.ToString());
            stringBuilder.Append(",");
            stringBuilder.Append(this.Line != null ? this.Line : "null");
            stringBuilder.Append(",");
            stringBuilder.Append(this.position != null ? this.position : "null");
            stringBuilder.Append(",");
            stringBuilder.Append(this.destinationId);
            stringBuilder.Append(",");
            stringBuilder.Append(this.Destination != null ? this.Destination : "null");
            stringBuilder.Append(")");
            return ((object)stringBuilder).ToString();
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;
            if (!(o is Departure))
                return false;
            Departure departure = (Departure)o;
            if (!this.nullSafeEquals((object)this._plannedTime, (object)departure._plannedTime) || !this.nullSafeEquals((object)this._predictedTime, (object)departure._predictedTime) || (!this.nullSafeEquals((object)this.Line, (object)departure.Line) || this.destinationId != departure.destinationId) || !this.Destination.Equals(departure.Destination))
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            return ((((0 + this.nullSafeHashCode((object)this._plannedTime)) * 29 + this.nullSafeHashCode((object)this._predictedTime)) * 29 + this.nullSafeHashCode((object)this.Line)) * 29 + this.destinationId) * 29 + this.Destination.GetHashCode();
        }

        private bool nullSafeEquals(object o1, object o2)
        {
            if (o1 == null && o2 == null || o1 != null && o1.Equals(o2))
                return true;
            else
                return false;
        }

        private int nullSafeHashCode(object o)
        {
            if (o == null)
                return 0;
            else
                return o.GetHashCode();
        }

        //public string Line { get; set; }
        //public string StationName { get; set; }
        //public DateTime DepartureTime { get; set; }
        //public string TargetStationName { get; set; }

        //public Departure()
        //{
        //    Line = "0";
        //    StationName = "StationsName";
        //    DepartureTime = new DateTime();
        //    TargetStationName = "ZielStation";
        //}

        //public Departure(string stationName, DateTime departureTime, string targetStationName)
        //{
        //    this.StationName = stationName;
        //    this.DepartureTime = departureTime;
        //    this.TargetStationName = targetStationName;
        //}
    }
}
