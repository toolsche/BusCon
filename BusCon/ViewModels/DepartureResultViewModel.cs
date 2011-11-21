using System;
using System.Windows.Media;

namespace BusCon.ViewModels
{
    public class DepartureResultViewModel
    {
        private int _typeIconHeight = 60;
        private int _typeIconWidth = 60;
        private int _lineIconWidth = 90;

        public string Line { get; set; }

        public string Station { get; set; }

        public int InMin { get; set; }

        public DateTime? PlannedDepartureTime { get; set; }

        public string TimeString
        {
            get
            {
                if (this.PlannedDepartureTime.HasValue)
                    return string.Format("(um {0}h)", (object)this.PlannedDepartureTime.Value.ToShortTimeString());
                else
                    return string.Format("(um {0}h)", (object)DateTime.Now.AddMinutes((double)this.InMin).ToShortTimeString());
            }
        }

        public string InMinString
        {
            get
            {
                return string.Format("in {0} Minuten", (object)this.InMin);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            DepartureResultViewModel departureResultViewModel = obj as DepartureResultViewModel;
            if (departureResultViewModel != null && this.Line == departureResultViewModel.Line && this.Station == departureResultViewModel.Station)
                return this.InMin == departureResultViewModel.InMin;
            else
                return false;
        }

        private Color GetSBahnColor(string line)
        {
            string str = line.ToLower();
            if (str == "s1" || str == "s2" || (str == "s3" || str == "s4") || (str == "s5" || str == "s6" || (str == "s7" || str == "s8")) || (str == "s20" || str == "s27" || !(str == "sa")))
                return Color.FromArgb(byte.MaxValue, (byte)26, (byte)179, (byte)226);
            else
                return Color.FromArgb(byte.MaxValue, (byte)32, (byte)32, (byte)32);
        }

        private Color GetUBahnColor(string line)
        {
            string str = line.ToLower();
            if (str == "u1" || str == "u2" || (str == "u3" || str == "u4") || (str == "u5" || !(str == "u6")))
                return Color.FromArgb(byte.MaxValue, (byte)58, (byte)113, (byte)43);
            else
                return Color.FromArgb(byte.MaxValue, (byte)0, (byte)92, (byte)172);
        }
    }
}
