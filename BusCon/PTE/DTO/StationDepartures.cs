using System.Collections.Generic;
using System.Text;

namespace BusCon.PTE.DTO
{
    public sealed class StationDepartures
    {
        public Location Location { get; set; }

        public List<Departure> Departures { get; set; }

        public List<LineDestination> Lines { get; set; }

        public StationDepartures(Location location, List<Departure> departures, List<LineDestination> lines)
        {
            this.Location = location;
            this.Departures = departures;
            this.Lines = lines;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(this.GetType().Name);
            stringBuilder.Append("[");
            if (this.Location != null)
                stringBuilder.Append(this.Location.ToDebugString());
            if (this.Departures != null)
                stringBuilder.Append(" ").Append(this.Departures.Count).Append(" departures");
            stringBuilder.Append("]");
            return ((object)stringBuilder).ToString();
        }
    }
}
