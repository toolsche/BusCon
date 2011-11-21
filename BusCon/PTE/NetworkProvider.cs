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
using BusCon.PTE.DTO;

namespace BusCon.PTE
{
    public interface NetworkProvider
    {
        NetworkId id();

        void QueryConnections(Action<QueryConnectionsResult> callback, Location from, Location via, Location to, DateTime date, bool dep, string products, WalkSpeed walkSpeed, bool forceReload);
    }
}
