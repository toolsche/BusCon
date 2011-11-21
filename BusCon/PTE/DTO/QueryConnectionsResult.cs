using System.Collections.Generic;
using System.Text;

namespace BusCon.PTE.DTO
{
    public sealed class QueryConnectionsResult
    {
        public static readonly QueryConnectionsResult TOO_CLOSE = new QueryConnectionsResult(QueryConnectionsResult.Status.TOO_CLOSE);
        public static readonly QueryConnectionsResult UNRESOLVABLE_ADDRESS = new QueryConnectionsResult(QueryConnectionsResult.Status.UNRESOLVABLE_ADDRESS);
        public static readonly QueryConnectionsResult NO_CONNECTIONS = new QueryConnectionsResult(QueryConnectionsResult.Status.NO_CONNECTIONS);
        public static readonly QueryConnectionsResult INVALID_DATE = new QueryConnectionsResult(QueryConnectionsResult.Status.INVALID_DATE);
        public readonly QueryConnectionsResult.Status status;
        public readonly List<Location> ambiguousFrom;
        public readonly List<Location> ambiguousVia;
        public readonly List<Location> ambiguousTo;
        public readonly string queryUri;
        public readonly Location from;
        public readonly Location via;
        public readonly Location to;
        public readonly List<Connection> connections;

        public string CommandEarlier { get; set; }

        public string CommandLater { get; set; }

        static QueryConnectionsResult()
        {
        }

        public QueryConnectionsResult(string queryUri, Location from, Location via, Location to, string commandEarlier, string commandLater, List<Connection> connections)
        {
            this.status = QueryConnectionsResult.Status.OK;
            this.queryUri = queryUri;
            this.from = from;
            this.via = via;
            this.to = to;
            this.CommandEarlier = commandEarlier;
            this.CommandLater = commandLater;
            this.connections = connections;
            this.ambiguousFrom = (List<Location>)null;
            this.ambiguousVia = (List<Location>)null;
            this.ambiguousTo = (List<Location>)null;
        }

        public QueryConnectionsResult(List<Location> ambiguousFrom, List<Location> ambiguousVia, List<Location> ambiguousTo)
        {
            this.status = QueryConnectionsResult.Status.AMBIGUOUS;
            this.ambiguousFrom = ambiguousFrom;
            this.ambiguousVia = ambiguousVia;
            this.ambiguousTo = ambiguousTo;
            this.queryUri = (string)null;
            this.from = (Location)null;
            this.via = (Location)null;
            this.to = (Location)null;
            this.CommandEarlier = (string)null;
            this.connections = (List<Connection>)null;
        }

        public QueryConnectionsResult(QueryConnectionsResult.Status status)
        {
            this.status = status;
            this.ambiguousFrom = (List<Location>)null;
            this.ambiguousVia = (List<Location>)null;
            this.ambiguousTo = (List<Location>)null;
            this.queryUri = (string)null;
            this.from = (Location)null;
            this.via = (Location)null;
            this.to = (Location)null;
            this.CommandEarlier = (string)null;
            this.connections = (List<Connection>)null;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(this.GetType().Name);
            stringBuilder.Append("[").Append((object)this.status).Append(": ");
            if (this.connections != null)
                stringBuilder.Append(this.connections.Count).Append(" connections " + (object)this.connections + ", ");
            if (this.ambiguousFrom != null)
                stringBuilder.Append(this.ambiguousFrom.Count).Append(" ambiguous from, ");
            if (this.ambiguousVia != null)
                stringBuilder.Append(this.ambiguousVia.Count).Append(" ambiguous via, ");
            if (this.ambiguousTo != null)
                stringBuilder.Append(this.ambiguousTo.Count).Append(" ambiguous to, ");
            string str = ((object)stringBuilder).ToString();
            if (str.Substring(str.Length - 2).Equals(", "))
                str.Substring(0, str.Length - 2);
            stringBuilder.Append("]");
            return ((object)stringBuilder).ToString();
        }

        public enum Status
        {
            OK,
            AMBIGUOUS,
            TOO_CLOSE,
            UNRESOLVABLE_ADDRESS,
            NO_CONNECTIONS,
            INVALID_DATE,
            SERVICE_DOWN,
            SESSION_EXPIRED,
        }
    }
}
