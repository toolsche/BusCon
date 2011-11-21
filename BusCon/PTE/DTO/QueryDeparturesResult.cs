using System.Text;
using System.Collections.Generic;

namespace BusCon.PTE.DTO
{
    public sealed class QueryDeparturesResult
    {
        public QueryDeparturesResult.DepartureResultStatuses Status { get; set; }

        public List<StationDepartures> StationDepartures { get; set; }

        public QueryDeparturesResult()
            : this(QueryDeparturesResult.DepartureResultStatuses.OK)
        {
        }

        public QueryDeparturesResult(QueryDeparturesResult.DepartureResultStatuses status)
        {
            this.Status = status;
            this.StationDepartures = new List<StationDepartures>();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(this.GetType().Name);
            stringBuilder.Append("[").Append((object)this.Status);
            stringBuilder.Append(" ").Append((object)this.StationDepartures);
            stringBuilder.Append("]");
            return ((object)stringBuilder).ToString();
        }

        public enum DepartureResultStatuses
        {
            OK,
            INVALID_STATION,
            SERVICE_DOWN,
        }
    }
}
