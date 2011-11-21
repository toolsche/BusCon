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
using System.Text;

namespace BusCon.PTE.DTO
{
    public sealed class Stop
    {
        public readonly Location location;
        public readonly string position;
        public readonly DateTime time;

        public Stop(Location location, string position, DateTime time)
        {
            this.location = location;
            this.position = position;
            this.time = time;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Stop(");
            stringBuilder.Append((object)this.location);
            stringBuilder.Append(",");
            stringBuilder.Append(this.position != null ? this.position : "null");
            stringBuilder.Append(",");
            stringBuilder.Append(this.time.ToString());
            stringBuilder.Append(")");
            return ((object)stringBuilder).ToString();
        }
    }
}
