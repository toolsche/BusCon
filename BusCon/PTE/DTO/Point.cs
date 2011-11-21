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

namespace BusCon.PTE.DTO
{
    public sealed class Point
    {
        public readonly int lat;
        public readonly int lon;

        public Point(int lat, int lon)
        {
            this.lat = lat;
            this.lon = lon;
        }

        public override string ToString()
        {
            return "[" + (object)this.lat + "/" + (string)(object)this.lon + "]";
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;
            if (!(o is Point))
                return false;
            Point point = (Point)o;
            if (this.lat != point.lat || this.lon != point.lon)
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            return this.lat + 27 * this.lon;
        }
    }
}
