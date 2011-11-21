using System;

namespace BusCon.PTE.DTO
{
    public sealed class Location
    {
        public LocationType Type { get; set; }
        public int Id { get; set; }
        public int Lat { get; set; }
        public int Lon { get; set; }
        public string Place { get; set; }
        public string Name { get; set; }

        public Location()
        {
            this.Type = LocationType.ANY;
            this.Id = 0;
            this.Lat = 0;
            this.Lon = 0;
            this.Place = string.Empty;
            this.Name = string.Empty;
        }

        public Location(LocationType type, int id, int lat, int lon, string place, string name)
        {
            this.Type = type;
            this.Id = id;
            this.Lat = lat;
            this.Lon = lon;
            this.Place = place;
            this.Name = name;
        }

        public Location(LocationType type, int id, string place, string name)
        {
            this.Type = type;
            this.Id = id;
            this.Lat = 0;
            this.Lon = 0;
            this.Place = place;
            this.Name = name;

            if (type != LocationType.ADDRESS)
                return;
            //this.GeocodeAddress();
        }

        public Location(LocationType type, int id, int lat, int lon)
        {
            this.Type = type;
            this.Id = id;
            this.Lat = lat;
            this.Lon = lon;
            this.Place = null;
            this.Name = null;
        }

        public Location(LocationType type, int id)
        {
            this.Type = type;
            this.Id = id;
            this.Lat = 0;
            this.Lon = 0;
            this.Place = null;
            this.Name = null;
        }

        public Location(LocationType type, int lat, int lon)
        {
            this.Type = type;
            this.Id = 0;
            this.Lat = lat;
            this.Lon = lon;
            this.Place = null;
            this.Name = null;
        }

        public bool HasId()
        {
            return Id != 0;
        }

        public bool HasLocation()
        {
            return Lat != 0 || Lon != 0;
        }

        public override string ToString()
        {
            return Name; // invoked by AutoCompleteTextView in landscape orientation
        }

        public string ToDebugString()
        {
            return "[" + Type + " " + Id + " " + Lat + "/" + Lon + " " + (Place != null ? "'" + Place + "'" : "null") + " '" + Name + "']";
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;
            if (!(o is Location))
                return false;
            Location other = (Location) o;
            if (this.Type != other.Type)
                return false;
            if (this.Id != other.Id)
                return false;
            if (this.Id != 0)
                return true;

            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode(); // FIXME not very discriminative
        }

        //private void OnGeocodeAddressComplete(GeocodeResult location)
        //{
        //    if (location != null)
        //    {
        //        this.Lat = (int)(location.Lat * 1000000.0);
        //        this.Lon = (int)(location.Lon * 1000000.0);
        //    }
        //    this.GeocodeCompleted.Set();
        //}

        //private void GeocodeAddress()
        //{
        //    string address = this.Name.ToLower();
        //    string[] strArray = this.Name.Split(new char[1] {' '});
        //    bool flag = false;
        //    foreach (string str in strArray)
        //    {
        //        if (!flag)
        //        {
        //            if (str == "m")
        //            {
        //                address = address.Replace("m ", " ").Replace(" m", " ") + " münchen";
        //                flag = true;
        //            }
        //            else if (str == "muc")
        //            {
        //                address = address.Replace("muc ", " ").Replace(" muc", " ") + " münchen";
        //                flag = true;
        //            }
        //            else if ((double)new MatchsMaker(str, "münchen").Score >= 0.75 || (double)new MatchsMaker(str, "munich").Score >= 0.75)
        //            {
        //                address = address.Replace(str, "münchen");
        //                flag = true;
        //            }
        //        }
        //        else
        //            break;
        //    }
        //    if (!flag)
        //        address = address + " münchen";

        //    this.GeocodeCompleted.Reset();
        //    new GeocodeHelper().GeocodeAddress(new Action<GeocodeResult>(this.OnGeocodeAddressComplete), address);
        //}
    }

    public enum LocationType
    {
        ANY,
        STATION,
        POI,
        ADDRESS
    }
}
