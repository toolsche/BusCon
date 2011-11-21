using System;
using BusCon.PTE.DTO;
using BusCon.PTE;

namespace BusCon.ViewModels
{
    public class ConnectionQueryViewModel
    {
        private AbstractEfaProvider efa;
        public Action<QueryConnectionsResult> ConnectionsCallback;
        public Action<QueryDeparturesResult> DeparturesCallback;

        public ConnectionQueryViewModel(AbstractEfaProvider efa)
        {
            this.efa = efa;
        }

        // Connections
        public Location From { get; set; }
        public Location Via { get; set; }
        public Location To { get; set; }
        public DateTime Date { get; set; }
        public bool IsDepartureTime { get; set; }
        public string Products { get; set; }
        public WalkSpeed WalkSpeed { get; set; }
        public bool ForceReload { get; set; }

        public string MoreConnectionsUri { get; set; }

        // Departure
        public int StationId { get; set; }
        public int MaxDepartures { get; set; }
        public bool Equivs { get; set; }
        public bool ForceUpdate { get; set; }

        public void QueryConnections()
        {
            efa.QueryConnections(ConnectionsCallback, From, Via, To, Date, IsDepartureTime, Products, WalkSpeed, ForceReload);
        }

        public void QueryConnections(Action<QueryConnectionsResult> callback)
        {
            efa.QueryConnections(callback, From, Via, To, Date, IsDepartureTime, Products, WalkSpeed, ForceReload);
        }

        public void QueryMoreConnections()
        {
            efa.QueryMoreConnections(ConnectionsCallback, MoreConnectionsUri);
        }

        public void QueryDepartures()
        {
            efa.QueryDepartures(DeparturesCallback, StationId, MaxDepartures, Equivs, ForceUpdate);
        }

        public void QueryDepartures(Action<QueryDeparturesResult> callback)
        {
            efa.QueryDepartures(callback, StationId, MaxDepartures, Equivs, ForceUpdate);
        }
    }
}
