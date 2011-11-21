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
    public sealed class Line
    {
        public string Label { get; set; }
        public string[] Colors { get; set; }
        public string Color { get; set; }
        public string LineType { get; set; }

        public Line()
        {
        }

        public Line(string label)
        {
            this.Label = label;
        }

        public Line(string label, string[] colors, string lineType)
        {
            this.Label = label;
            this.Colors = colors;
            this.LineType = lineType;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Line(");
            stringBuilder.Append(this.Label);
            stringBuilder.Append(")");
            return ((object)stringBuilder).ToString();
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;
            if (!(o is Line))
                return false;
            else
                return this.Label.Equals(((Line)o).Label);
        }

        public override int GetHashCode()
        {
            return this.Label.GetHashCode();
        }
    }
}
