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
using System.Globalization;
using System.Xml.Serialization;

namespace BusCon.PTE.DTO
{
    public class Fare : IFare
    {
        public string Network { get; set; }
        public string TicketType { get; set; }
        public string Currency { get; set; }
        public float AdultPrice { get; set; }
        public float ChildPrice { get; set; }
        public string UnitName { get; set; }
        public string AdultUnits { get; set; }
        public string ChildUnits { get; set; }

        //public enum Type
        //{
        //    ADULT, CHILD, YOUTH, STUDENT, MILITARY, SENIOR, DISABLED
        //}

        //public string network;
        //public Type type;
        //public CultureInfo cultureInfo;
        //public float fare;
        //public string unitName;
        //public string units;

        public Fare(string network, string ticketType, string currency, float adultPrice, float childPrice, string unitName, string adultUnits, string childUnits)
        {
            this.Network = network;
            this.TicketType = ticketType;
            this.Currency = currency;
            this.AdultPrice = adultPrice;
            this.ChildPrice = childPrice;
            this.UnitName = unitName;
            this.AdultUnits = adultUnits;
            this.ChildUnits = childUnits;
        }

        public Fare()
        {
        }

        //////////////////////////////////////////////////////////////////////
        // TODO: Anpassen
        /// <summary>
        /// Info zu Ticketpreisen
        /// </summary>
        /// 
        [XmlIgnore]
        public float FinalAdultPrice
        {
            get
            {
                if (this.TicketType == Fare.STREIFENPREIS)
                    return this.AdultPrice * float.Parse(this.AdultUnits);
                else
                    return this.AdultPrice;
            }
        }

        [XmlIgnore]
        public float FinalChildPrice
        {
            get
            {
                if (this.TicketType == Fare.STREIFENPREIS)
                    return this.ChildPrice * float.Parse(this.ChildUnits);
                else
                    return this.ChildPrice;
            }
        }

        [XmlIgnore]
        public bool HasUnits
        {
            get
            {
                if (this.TicketType == Fare.STREIFENPREIS && !string.IsNullOrEmpty(this.AdultUnits))
                    return !string.IsNullOrEmpty(this.ChildUnits);
                else
                    return false;
            }
        }

        //[XmlIgnore]
        //public string AdultUnitText
        //{
        //    get
        //    {
        //        string str = this.UnitName;
        //        if (this.UnitName == "Streifen")
        //            str = !(this.AdultUnits == "1") ? AppResources.Stripes : AppResources.Stripe;
        //        return "(" + this.AdultUnits + " " + str + ")";
        //    }
        //}

        //[XmlIgnore]
        //public string ChildUnitText
        //{
        //    get
        //    {
        //        string str = this.UnitName;
        //        if (this.UnitName == "Streifen")
        //            str = !(this.ChildUnits == "1") ? AppResources.Stripes : AppResources.Stripe;
        //        return "(" + this.ChildUnits + " " + str + ")";
        //    }
        //}

        [XmlIgnore]
        public bool HasAdultPrice
        {
            get
            {
                return (double)this.AdultPrice > 0.0;
            }
        }

        [XmlIgnore]
        public bool HasChildPrice
        {
            get
            {
                return (double)this.ChildPrice > 0.0;
            }
        }

        public static string SINGLE_TICKET = "SINGLE_TICKET";
        public static string STREIFENPREIS = "STREIFENPREIS";
        public static string TAGESKARTE_INNENRAUM = "TAGESKARTE_INNENRAUM";
        public static string TAGESKARTE_GESAMTRAUM = "TAGESKARTE_GESAMTRAUM";
        public static string TAGESKARTE_AUSSENRAUM = "TAGESKARTE_AUSSENRAUM";
        public static string TAGESKARTE_INNENRAUM_PARTNER = "TAGESKARTE_INNENRAUM_PARTNER";
        public static string TAGESKARTE_GESAMTRAUM_PARTNER = "TAGESKARTE_GESAMTRAUM_PARTNER";
        public static string TAGESKARTE_AUSSENRAUM_PARTNER = "TAGESKARTE_AUSSENRAUM_PARTNER";
        public static string ISARCARD9UHR_INNENRAUM = "ISARCARD9UHR_INNENRAUM";
        public static string ISARCARD9UHR_GESAMTRAUM = "ISARCARD9UHR_GESAMTRAUM";
        public static string ISARCARD9UHR_AUSSENRAUM = "ISARCARD9UHR_AUSSENRAUM";
        public static string ISARCARD60_INNENRAUM = "ISARCARD60_INNENRAUM";
        public static string ISARCARD60_GESAMTRAUM = "ISARCARD60_GESAMTRAUM";
        public static string ISARCARD60_AUSSENRAUM = "ISARCARD60_AUSSENRAUM";
        public static string TAGESKARTE_XXL = "TAGESKARTE_XXL";
        public static string TAGESKARTE_XXL_PARTNER = "TAGESKARTE_XXL_PARTNER";

        //[XmlIgnore]
        //public string TicketTypeText
        //{
        //    get
        //    {
        //        string ticketType = this.TicketType;
        //        if (ticketType == Fare.STREIFENPREIS)
        //            return AppResources.STREIFENPREIS;
        //        if (ticketType == Fare.SINGLE_TICKET)
        //        {
        //            if (string.IsNullOrEmpty(this.AdultUnits) || string.IsNullOrEmpty(this.UnitName))
        //                return AppResources.SINGLE_TICKET;
        //            string str = AppResources.Zones;
        //            if (this.AdultUnits == "K")
        //                return AppResources.SINGLE_TICKET + " (" + AppResources.ShortTrip + ")";
        //            if (this.AdultUnits == "1")
        //                str = AppResources.Zone;
        //            return AppResources.SINGLE_TICKET + " (" + this.AdultUnits + " " + str + ")";
        //        }
        //        else
        //        {
        //            if (ticketType == Fare.TAGESKARTE_INNENRAUM)
        //                return AppResources.TAGESKARTE_INNENRAUM;
        //            if (ticketType == Fare.TAGESKARTE_AUSSENRAUM)
        //                return AppResources.TAGESKARTE_AUSSENRAUM;
        //            if (ticketType == Fare.TAGESKARTE_GESAMTRAUM)
        //                return AppResources.TAGESKARTE_GESAMTRAUM;
        //            if (ticketType == Fare.TAGESKARTE_INNENRAUM_PARTNER)
        //                return AppResources.TAGESKARTE_INNENRAUM_PARTNER;
        //            if (ticketType == Fare.TAGESKARTE_AUSSENRAUM_PARTNER)
        //                return AppResources.TAGESKARTE_AUSSENRAUM_PARTNER;
        //            if (ticketType == Fare.TAGESKARTE_GESAMTRAUM_PARTNER)
        //                return AppResources.TAGESKARTE_GESAMTRAUM_PARTNER;
        //            if (ticketType == Fare.ISARCARD9UHR_INNENRAUM)
        //                return AppResources.ISARCARD9UHR_INNENRAUM;
        //            if (ticketType == Fare.ISARCARD9UHR_AUSSENRAUM)
        //                return AppResources.ISARCARD9UHR_AUSSENRAUM;
        //            if (ticketType == Fare.ISARCARD9UHR_GESAMTRAUM)
        //                return AppResources.ISARCARD9UHR_GESAMTRAUM;
        //            if (ticketType == Fare.ISARCARD60_INNENRAUM)
        //                return AppResources.ISARCARD60_INNENRAUM;
        //            if (ticketType == Fare.ISARCARD60_AUSSENRAUM)
        //                return AppResources.ISARCARD60_AUSSENRAUM;
        //            if (ticketType == Fare.ISARCARD60_GESAMTRAUM)
        //                return AppResources.ISARCARD60_GESAMTRAUM;
        //            if (ticketType == Fare.TAGESKARTE_XXL)
        //                return AppResources.TAGESKARTE_XXL;
        //            if (ticketType == Fare.TAGESKARTE_XXL_PARTNER)
        //                return AppResources.TAGESKARTE_XXL_PARTNER;
        //            else
        //                return ticketType;
        //        }
        //    }
        //}
    }
}
