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
    public sealed class CommuterFare : IFare
    {
        public float WeekAdult { get; set; }

        public float WeekChild { get; set; }

        public float WeekEducation { get; set; }

        public float MonthAdult { get; set; }

        public float MonthChild { get; set; }

        public float MonthEducation { get; set; }

        public string ZonesString { get; set; }

        public CommuterFare(float weekAdult, float weekChild, float weekEducation, float monthAdult, float monthChild, float monthEducation, string zonesString)
        {
            this.WeekAdult = weekAdult;
            this.WeekChild = weekChild;
            this.WeekEducation = weekEducation;
            this.MonthAdult = monthAdult;
            this.MonthChild = monthChild;
            this.MonthEducation = monthEducation;
            this.ZonesString = zonesString;
        }

        public CommuterFare()
        {
        }
    }
}
