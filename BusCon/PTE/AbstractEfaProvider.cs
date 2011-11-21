using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using BusCon.PTE.DTO;
using BusCon.Utility;

namespace BusCon.PTE
{
    public abstract class AbstractEfaProvider : NetworkProvider
    {
        private static readonly string P_LINE_IRE = "IRE\\d+";
        private static readonly string P_LINE_RE = "RE\\d+";
        private static readonly string P_LINE_RB = "RB\\d+";
        private static readonly string P_LINE_VB = "VB\\d+";
        private static readonly string P_LINE_OE = "OE\\d+";
        private static readonly string P_LINE_R = "R\\d+(/R\\d+|\\(z\\))?";
        private static readonly string P_LINE_U = "U\\d+";
        private static readonly string P_LINE_S = "^(?:%)?(S\\d+)";
        private static readonly string P_LINE_NUMBER = "\\d+";
        private static readonly string P_LINE_Y = "\\d+Y";
        private static string P_STATION_NAME_WHITESPACE = "\\s+";
        private static readonly string P_PLATFORM = "#?(\\d+)";
        private static readonly string P_PLATFORM_NAME = "(?:Gleis|Gl\\.|Bstg\\.)?\\s*(\\d+)\\s*(?:([A-Z])\\s*(?:-\\s*([A-Z]))?)?";
        protected static Dictionary<WalkSpeed, string> WALKSPEED_MAP = new Dictionary<WalkSpeed, string>()
            {
              { WalkSpeed.SLOW, "slow" },
              { WalkSpeed.NORMAL, "normal" },
              { WalkSpeed.FAST, "fast" }
            };
        private readonly string _apiBase;
        private readonly string _additionalQueryParameter;
        private readonly bool _canAcceptPoiID;
        private NumberFormatInfo _nfi;

        static AbstractEfaProvider()
        {
        }

        public AbstractEfaProvider() : this(apiBase: (string)null, additionalQueryParameter: (string)null)
        {
        }

        public AbstractEfaProvider(string apiBase, string additionalQueryParameter) : this(apiBase, additionalQueryParameter, canAcceptPoiID: false)
        {
        }

        public AbstractEfaProvider(string apiBase, string additionalQueryParameter, bool canAcceptPoiID)
        {
            this._apiBase = apiBase;
            this._additionalQueryParameter = additionalQueryParameter;
            this._canAcceptPoiID = canAcceptPoiID;
            this._nfi = new NumberFormatInfo();
            this._nfi.NumberDecimalSeparator = ".";
        }

        private void AppendCommonRequestParams(StringBuilder uri)
        {
            uri.Append("?outputFormat=XML");
            uri.Append("&coordOutputFormat=WGS84");

            if (this._additionalQueryParameter == null)
                return;

            uri.Append('&').Append(this._additionalQueryParameter);
        }

        private string ProcessItdOdvPlace(XElement element)
        {
            XElement itdOdvPlaceElement = element.Element("itdOdvPlace");
            if (itdOdvPlaceElement == null)
                throw new Exception("expecting <itdOdvPlace />");

            string state = itdOdvPlaceElement.Attribute("state").Value;
            string locationName = null;

            if ("identified".Equals(state))
            {
                XElement odvPlaceElemElement = itdOdvPlaceElement.Element("odvPlaceElem");
                if (odvPlaceElemElement != null)
                    locationName = this.NormalizeLocationName(odvPlaceElemElement.Value);
            }
            return locationName;
        }

        private string GetAttributeValue(XElement element, string attribute, string message)
        {
            if (element.Attribute(attribute) != null)
                return element.Attribute(attribute).Value;
            else
                throw new Exception(message);
        }

        private string GetAttributeValueOrNull(XElement element, string attribute)
        {
            if (element.Attribute(attribute) != null)
                return element.Attribute(attribute).Value;
            else
                return (string)null;
        }

        private Location ProcessOdvNameElem(XElement odvNameElem, string defaultPlace)
        {
            string anyType = this.GetAttributeValueOrNull(odvNameElem, "anyType");
            string idValue = this.GetAttributeValueOrNull(odvNameElem, "id");
            string stopId = this.GetAttributeValueOrNull(odvNameElem, "stopID");
            string poiId = this.GetAttributeValueOrNull(odvNameElem, "poiID");
            string streetId = this.GetAttributeValueOrNull(odvNameElem, "streetID");
            string placeName = !"loc".Equals(anyType) ? this.NormalizeLocationName(this.GetAttributeValueOrNull(odvNameElem, "locality")) : null;
            string objectName = this.NormalizeLocationName(this.GetAttributeValueOrNull(odvNameElem, "objectName"));
            int lat = 0;
            int lon = 0;

            if (odvNameElem.Attribute("mapName") != null && "WGS84".Equals(odvNameElem.Attribute("mapName").Value))
            {
                lat = this.ParseLatLon(odvNameElem.Attribute("y").Value);
                lon = this.ParseLatLon(odvNameElem.Attribute("x").Value);
            }

            LocationType type;
            int id;
            if ("stop".Equals(anyType))
            {
                type = LocationType.STATION;
                id = int.Parse(idValue);
            }
            else if ("poi".Equals(anyType) || "poiHierarchy".Equals(anyType))
            {
                type = LocationType.POI;
                id = int.Parse(idValue);
            }
            else if ("loc".Equals(anyType))
            {
                type = LocationType.ANY;
                id = 0;
            }
            else if ("postcode".Equals(anyType) || "street".Equals(anyType) || ("crossing".Equals(anyType) || "address".Equals(anyType)) || ("singlehouse".Equals(anyType) || "buildingname".Equals(anyType)))
            {
                type = LocationType.ADDRESS;
                id = 0;
            }
            else if (stopId != null)
            {
                type = LocationType.STATION;
                id = int.Parse(stopId);
            }
            else if (poiId != null)
            {
                type = LocationType.POI;
                id = int.Parse(poiId);
            }
            else if (stopId == null && idValue == null && (lat != 0 || lon != 0))
            {
                type = LocationType.ADDRESS;
                id = 0;
            }
            else if (streetId != null)
            {
                type = LocationType.ADDRESS;
                id = int.Parse(streetId);
            }
            else
                throw new ArgumentException("unknown type: " + anyType + " " + idValue + " " + stopId);

            string odvName = this.NormalizeLocationName(odvNameElem.Value);

            return new Location(type, id, lat, lon, placeName ?? defaultPlace, objectName ?? odvName);
        }

        private Location ProcessItdOdvAssignedStop(XElement itdOdvAssignedStop)
        {
            int id = int.Parse(this.GetAttributeValue(itdOdvAssignedStop, "stopID", "cannot find attribute stopID on itdOdvAssignedStop"));
            int lat = 0;
            int lon = 0;

            if ("WGS84".Equals(this.GetAttributeValueOrNull(itdOdvAssignedStop, "mapName")))
            {
                lat = int.Parse(this.GetAttributeValue(itdOdvAssignedStop, "y", "cannot find attribute y on itdOdvAssignedStop"));
                lon = int.Parse(this.GetAttributeValue(itdOdvAssignedStop, "x", "cannot find attribute x on itdOdvAssignedStop"));
            }

            string place = this.NormalizeLocationName(this.GetAttributeValueOrNull(itdOdvAssignedStop, "place"));
            string name = this.NormalizeLocationName(itdOdvAssignedStop.Value);

            return new Location(LocationType.STATION, id, lat, lon, place, name);
        }

        protected string ParseLine(string mot, string name, string longName, string noTrainName)
        {
            if (mot == null)
            {
                if (noTrainName != null)
                {
                    string str = name != null ? name : "";
                    if (noTrainName.Equals("S-Bahn"))
                        return 'S' + str;
                    if (noTrainName.Equals("U-Bahn"))
                        return 'U' + str;
                    if (noTrainName.Equals("Straßenbahn") || noTrainName.Equals("Badner Bahn"))
                        return 'T' + str;
                    if (noTrainName.Equals("Stadtbus") || noTrainName.Equals("Citybus") || (noTrainName.Equals("Regionalbus") || noTrainName.Equals("ÖBB-Postbus")) || 
                        noTrainName.Equals("Autobus") || noTrainName.Equals("Discobus") || (noTrainName.Equals("Nachtbus") || noTrainName.Equals("Anrufsammeltaxi")) || 
                        noTrainName.Equals("Ersatzverkehr") || noTrainName.Equals("Vienna Airport Lines"))
                        return 'B' + str;
                }

                throw new Exception("cannot normalize mot '" + mot + "' name '" + name + "' long '" + longName + "' noTrainName '" + noTrainName + "'");
            }
            else
            {
                switch (int.Parse(mot))
                {
                    case 0:
                        string[] parts = longName.Split(' ');
                        string input = parts[0];
                        string num = parts.Length >= 2 ? parts[1] : null;
                        string str = input + (num ?? "");

                        if ((input.Equals("EC") // Eurocity
                            || input.Equals("EN") // Euronight
                            || input.Equals("IC") // Intercity
                            || input.Equals("ICE")) // Intercity Express
                            || input.Equals("X") // InterConnex
                            || input.Equals("CNL") // City Night Line
                            || input.Equals("THA") // Thalys
                            || input.Equals("TGV") // TGV
                            || input.Equals("RJ") // railjet
                            || input.Equals("OEC") // ÖBB-EuroCity
                            || input.Equals("OIC") // ÖBB-InterCity
                            || input.Equals("HT") // First Hull Trains, GB
                            || input.Equals("MT") // Müller Touren, Schnee Express
                            || input.Equals("HKX") // Hamburg-Koeln-Express
                            || input.Equals("DNZ")) // Nachtzug Basel-Moskau
                            return 'I' + str;

                        if (input.Equals("IR") // Interregio
                            || input.Equals("IRE") // Interregio-Express
                            || Regex.Match(input, AbstractEfaProvider.P_LINE_IRE).Success 
                            || input.Equals("RE") // Regional-Express
                            || input.Equals("R-Bahn") // Regional-Express, VRR
                            || input.Equals("REX") // RegionalExpress, Österreich
                            || input.Equals("EZ") // ÖBB ErlebnisBahn
                            || Regex.Match(input, AbstractEfaProvider.P_LINE_RE).Success
                            || input.Equals("RB") // Regionalbahn
                            || Regex.Match(input, AbstractEfaProvider.P_LINE_RB).Success
                            || input.Equals("R") // Regionalzug
                            || Regex.Match(input, AbstractEfaProvider.P_LINE_R).Success 
                            || input.Equals("Bahn") 
                            || input.Equals("Regionalbahn")
                            || input.Equals("D") // Schnellzug
                            || input.Equals("E") // Eilzug
                            || input.Equals("S") // ~Innsbruck
                            || input.Equals("WFB") || "Westfalenbahn".Equals(input) // Westfalenbahn
                            || input.Equals("NWB") || input.Equals("NordWestBahn") // NordWestBahn
                            || input.Equals("ME") // Metronom
                            || input.Equals("ERB") // eurobahn
                            || input.Equals("CAN") // cantus
                            || input.Equals("HEX") // Veolia Verkehr Sachsen-Anhalt
                            || input.Equals("EB") // Erfurter Bahn
                            || input.Equals("MRB") // Mittelrheinbahn
                            || input.Equals("ABR") // ABELLIO Rail NRW
                            || input.Equals("NEB") // Niederbarnimer Eisenbahn
                            || input.Equals("OE") // Ostdeutsche Eisenbahn
                            || Regex.Match(input, AbstractEfaProvider.P_LINE_OE).Success 
                            || input.Equals("MR") // Märkische Regiobahn
                            || input.Equals("OLA") // Ostseeland Verkehr
                            || input.Equals("UBB") // Usedomer Bäderbahn
                            || input.Equals("EVB") // Elbe-Weser
                            || input.Equals("PEG") // Prignitzer Eisenbahngesellschaft
                            || input.Equals("RTB") // Rurtalbahn
                            || input.Equals("STB") // Süd-Thüringen-Bahn
                            || input.Equals("HTB") // Hellertalbahn
                            || input.Equals("VBG") || input.Equals("VB") // Vogtlandbahn
                            || Regex.Match(input, AbstractEfaProvider.P_LINE_VB).Success 
                            || input.Equals("VX") // Vogtland Express
                            || input.Equals("CB") // City-Bahn Chemnitz
                            || input.Equals("VEC") // VECTUS Verkehrsgesellschaft
                            || input.Equals("HzL") // Hohenzollerische Landesbahn
                            || input.Equals("OSB") // Ortenau-S-Bahn
                            || input.Equals("SBB") // SBB
                            || input.Equals("MBB") // Mecklenburgische Bäderbahn Molli
                            || input.Equals("OS") // Regionalbahn
                            || input.Equals("SP") 
                            || input.Equals("Dab") // Daadetalbahn
                            || input.Equals("FEG") // Freiberger Eisenbahngesellschaft
                            || input.Equals("ARR") // ARRIVA
                            || input.Equals("HSB") // Harzer Schmalspurbahn
                            || input.Equals("SBE") // Sächsisch-Böhmische Eisenbahngesellschaft
                            || input.Equals("ALX") // Arriva-Länderbahn-Express
                            || input.Equals("EX") // ALX verwandelt sich
                            || input.Equals("MEr") // metronom regional
                            || input.Equals("AKN") // AKN Eisenbahn
                            || input.Equals("ZUG") // Regionalbahn
                            || input.Equals("SOE") // Sächsisch-Oberlausitzer Eisenbahngesellschaft
                            || input.Equals("VIA") // VIAS
                            || input.Equals("BRB") // Bayerische Regiobahn
                            || input.Equals("BLB") // Berchtesgadener Land Bahn
                            || input.Equals("HLB") // Hessische Landesbahn
                            || input.Equals("NOB") // NordOstseeBahn
                            || input.Equals("WEG") // Wieslauftalbahn
                            || input.Equals("NBE") // Nordbahn Eisenbahngesellschaft
                            || input.Equals("VEN") // Rhenus Veniro
                            || input.Equals("DPN") // Nahreisezug
                            || input.Equals("SHB") // Schleswig-Holstein-Bahn
                            || input.Equals("RBG") // Regental Bahnbetriebs GmbH
                            || input.Equals("BOB") // Bayerische Oberlandbahn
                            || input.Equals("SWE") // Südwestdeutsche Verkehrs AG
                            || input.Equals("VE") // Vetter
                            || input.Equals("SDG") // Sächsische Dampfeisenbahngesellschaft
                            || input.Equals("PRE") // Pressnitztalbahn
                            || input.Equals("VEB") // Vulkan-Eifel-Bahn
                            || input.Equals("neg") // Norddeutsche Eisenbahn Gesellschaft
                            || input.Equals("AVG") // Felsenland-Express
                            || input.Equals("ABG") // Anhaltische Bahngesellschaft
                            || input.Equals("LGB") // Lößnitzgrundbahn
                            || input.Equals("LEO") // Chiemgauer Lokalbahn
                            || input.Equals("WTB") // Weißeritztalbahn
                            || input.Equals("P") // Kasbachtalbahn, Wanderbahn im Regental, Rhön-Zügle
                            || input.Equals("ÖBA") // Eisenbahn-Betriebsgesellschaft Ochsenhausen
                            || input.Equals("MBS") // Montafonerbahn
                            || input.Equals("EGP") || input.Equals("SBS") || input.Equals("SES") // EGP - die Städtebahn GmbH
                            || input.Equals("agi") || input.Equals("ag") // agilis
                            || input.Equals("TLX") // Trilex (Vogtlandbahn)
                            || input.Equals("BE") // Grensland-Express, Niederlande
                            || input.Equals("MEL") // Museums-Eisenbahn Losheim
                            || input.Equals("Abellio-Zug") // Abellio
                            || input.Equals("KBS") // Kursbuchstrecke
                            || input.Equals("Zug") 
                            || input.Equals("ÖBB") 
                            || input.Equals("CAT") // City Airport Train Wien
                            || input.Equals("DZ") // Dampfzug, STV
                            || input.Equals("CD") 
                            || input.Equals("PR") 
                            || input.Equals("KD") // Koleje Dolnośląskie (Niederschlesische Eisenbahn)
                            || input.Equals("VIAMO")
                            || input.Equals("SE") // Southeastern, GB
                            || input.Equals("SW") // South West Trains, GB
                            || input.Equals("SN") // Southern, GB
                            || input.Equals("NT") // Northern Rail, GB
                            || input.Equals("CH") // Chiltern Railways, GB
                            || input.Equals("EA") // National Express East Anglia, GB
                            || input.Equals("FC") // First Capital Connect, GB
                            || input.Equals("GW") // First Great Western, GB
                            || input.Equals("XC") // Cross Country, GB, evtl. auch highspeed?
                            || input.Equals("HC") // Heathrow Connect, GB
                            || input.Equals("HX") || input.Equals("GX") // Heathrow Express, GB
                            || input.Equals("GX") // Gatwick Express, GB
                            || input.Equals("C2C") // c2c, GB
                            || input.Equals("LM") // London Midland, GB
                            || input.Equals("EM") // East Midlands Trains, GB
                            || input.Equals("VT") // Virgin Trains, GB, evtl. auch highspeed?
                            || input.Equals("SR") // ScotRail, GB, evtl. auch long-distance?
                            || input.Equals("AW") // Arriva Trains Wales, GB
                            || input.Equals("WS") // Wrexham & Shropshire, GB
                            || input.Equals("TP") // First TransPennine Express, GB, evtl. auch long-distance?
                            || input.Equals("GC") // Grand Central, GB
                            || input.Equals("IL") // Island Line, GB
                            || input.Equals("BR") // ??, GB
                            || input.Equals("OO") // ??, GB
                            || input.Equals("XX") // ??, GB
                            || input.Equals("XZ")) // ??, GB
                            return 'R' + str;

                        if (input.Equals("DB-Zug") || input.Equals("Regionalexpress") // VRR
                            || "CAPITOL".Equals(name) // San Francisco
                            || "Train".Equals(noTrainName) || "Train".Equals(input) // San Francisco
                            || "Regional Train".Equals(noTrainName) // Melbourne
                            || input.Equals("ATB") // Autoschleuse Tauernbahn
                            || input.Equals("Chiemsee-Bahn")) 
                            return "R" + name;

                        if ("Regional Train :".Equals(longName))
                            return "R";

                        if (input.Equals("BSB") // Breisgau-S-Bahn
                            || input.Equals("RER") // Réseau Express Régional, Frankreich
                            || input.Equals("LO") // London Overground, GB
                            || "A".Equals(name) || "B".Equals(name) || "C".Equals(name)) // SES
                            return 'S' + str;

                        if (Regex.Match(input, AbstractEfaProvider.P_LINE_U).Success
                            || "Underground".Equals(input)) // London Underground, GB
                            return 'U' + str;

                        // San Francisco, BART
                        if ("Millbrae / Richmond".Equals(name)
                            || "Richmond / Millbrae".Equals(name) 
                            || "Fremont / RIchmond".Equals(name) 
                            || "Richmond / Fremont".Equals(name) 
                            || "Pittsburg Bay Point / SFO".Equals(name) 
                            || "SFO / Pittsburg Bay Point".Equals(name) 
                            || "Dublin Pleasanton / Daly City".Equals(name) 
                            || "Daly City / Dublin Pleasanton".Equals(name) 
                            || "Fremont / Daly City".Equals(name) 
                            || "Daly City / Fremont".Equals(name))
                            return 'U' + name;

                        if (input.Equals("RT") // RegioTram
                            || input.Equals("STR")) // Nordhausen
                            return 'T' + str;

                        // San Francisco
                        if ("California Cable Car".Equals(name) 
                            || "Muni".Equals(input) 
                            || "Cable".Equals(input) 
                            || "Muni Rail".Equals(noTrainName) 
                            || "Cable Car".Equals(noTrainName))
                            return 'T' + name;

                        if (input.Equals("BUS")
                            || input.Equals("SEV-Bus"))
                            return 'B' + str;

                        if (input.Length == 0 || Regex.Match(input, AbstractEfaProvider.P_LINE_NUMBER).Success)
                            return "?";

                        if (Regex.Match(input, AbstractEfaProvider.P_LINE_Y).Success)
                            return "?" + name;

                        throw new Exception("cannot normalize mot '" + mot + "' name '" + name + "' long '" + longName + "' noTrainName '" + noTrainName + "' type '" + input + "' str '" + str + "'");
                    case 1:
                        Match match = Regex.Match(name, AbstractEfaProvider.P_LINE_S);
                        if (match.Groups.Count >= 2)
                            return 'S' + match.Groups[1].Value;
                        else
                            return 'S' + name;
                    case 2:
                        return 'U' + name;
                    case 3:
                    case 4:
                        return 'T' + name;
                    case 5:
                    case 6:
                    case 7:
                    case 10:
                        if (name.Equals("Schienenersatzverkehr"))
                            return "BSEV";
                        else
                            return name; //return 'B' + name;
                    case 8:
                        return 'C' + name;
                    case 9:
                        return 'F' + name;
                    case 11:
                    case -1:
                        return '?' + name;
                    default:
                        throw new Exception("cannot normalize mot '" + mot + "' name '" + name + "' long '" + longName + "' noTrainName '" + noTrainName + "'");
                }
            }
        }

        public void QueryDepartures(Action<QueryDeparturesResult> callback, int stationId, int maxDepartures, bool equivs, bool forceUpdate)
        {
            StringBuilder uri = new StringBuilder(this._apiBase);
            uri.Append("XSLT_DM_REQUEST");
            this.AppendCommonRequestParams(uri);
            uri.Append("&type_dm=stop&useRealtime=1&mode=direct");
            uri.Append("&name_dm=").Append(stationId);
            uri.Append("&deleteAssignedStops_dm=").Append(equivs ? '0' : '1');

            if (maxDepartures > 0)
                uri.Append("&limit=").Append(maxDepartures);

            if (forceUpdate)
                uri.Append("&guid=" + Guid.NewGuid().ToString());

            new FileDownloader().Download(uri.ToString(), (Action<string>)(result =>
            {
                // no result
                if (string.IsNullOrEmpty(result))
                {
                    if (callback == null)
                        return;
                    callback(null);
                }
                else
                {
                    AsyncHelper.SafeExecute((Action)(() =>
                    {
                        try
                        {
                            QueryDeparturesResult queryDeparturesResult = this.QueryDepartures(uri.ToString(), result);

                            if (callback == null)
                                return;
                            callback(queryDeparturesResult);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }));
                }
            }), false);
        }

        private QueryDeparturesResult QueryDepartures(string uri, string xml)
        {
            XElement itdRequestElement = XDocument.Parse(xml).Element("itdRequest");
            if (itdRequestElement == null)
                throw new Exception("cannot find <itdRequest />");

            XElement itdDepartureMonitorRequestElement = itdRequestElement.Element("itdDepartureMonitorRequest");
            if (itdDepartureMonitorRequestElement == null)
                throw new Exception("cannot find <itdDepartureMonitorRequest />");

            XElement itdOdvElement = itdDepartureMonitorRequestElement.Element("itdOdv");
            if (itdOdvElement == null)
                throw new Exception("cannot find <itdOdv />");

            if (!"dm".Equals(this.GetAttributeValue(itdOdvElement, "usage", "cannot find attribute usage of itdOdv")))
                throw new Exception("cannot find <itdOdv usage=\"dm\" />");

            string defaultPlace = this.ProcessItdOdvPlace(itdOdvElement);

            XElement itdOdvNameElement = itdOdvElement.Element("itdOdvName");
            if (itdOdvNameElement == null)
                throw new Exception("cannot find <itdOdvName />");

            string itdOdvNameState = this.GetAttributeValue(itdOdvNameElement, "state", "cannot find attribute state of itdOdvName");
            if ("identified".Equals(itdOdvNameState))
            {
                QueryDeparturesResult departuresResult = new QueryDeparturesResult();
                Location location = this.ProcessOdvNameElem(itdOdvNameElement.Element("odvNameElem"), defaultPlace);

                // add departure
                departuresResult.StationDepartures.Add(new StationDepartures(location, new List<Departure>(), new List<LineDestination>()));

                XElement itdOdvAssignedStopsElement = itdOdvElement.Element("itdOdvAssignedStops");
                if (itdOdvAssignedStopsElement != null)
                {
                    foreach (XElement element in itdOdvAssignedStopsElement.Elements("itdOdvAssignedStop"))
                    {
                        Location assignedLocation = this.ProcessItdOdvAssignedStop(element);
                        if (this.FindStationDepartures(departuresResult.StationDepartures, assignedLocation.Id) == null)
                            departuresResult.StationDepartures.Add(new StationDepartures(assignedLocation, new List<Departure>(), new List<LineDestination>()));
                    }
                }

                DateTime? predictedTime = new DateTime?();
                XElement servingLinesElement = itdDepartureMonitorRequestElement.Element("itdServingLines");

                if (servingLinesElement != null)
                {
                    foreach (XElement itdServingLineElement in servingLinesElement.Elements("itdServingLine"))
                    {
                        string assignedStopIDAttr = this.GetAttributeValueOrNull(itdServingLineElement, "assignedStopID");
                        int assignedStopID = assignedStopIDAttr != null ? int.Parse(assignedStopIDAttr) : 0;

                        string destination = this.NormalizeLocationName(this.GetAttributeValueOrNull(itdServingLineElement, "direction"));

                        string destIDAttr = this.GetAttributeValueOrNull(itdServingLineElement, "destID");
                        int destinationId = destIDAttr == null || destIDAttr.Length <= 0 ? 0 : int.Parse(destIDAttr);

                        LineDestination lineDestination = new LineDestination(this.ProcessItdServingLine(itdServingLineElement), destinationId, destination);

                        StationDepartures stationDepartures = assignedStopID != 0 ? this.FindStationDepartures(departuresResult.StationDepartures, assignedStopID) : departuresResult.StationDepartures[0];

                        if (stationDepartures == null)
                            stationDepartures = new StationDepartures(new Location(LocationType.STATION, assignedStopID), new List<Departure>(), new List<LineDestination>());

                        if (!stationDepartures.Lines.Contains(lineDestination))
                            stationDepartures.Lines.Add(lineDestination);
                    }
                }

                XElement departureListElement = itdDepartureMonitorRequestElement.Element("itdDepartureList");
                if (departureListElement != null)
                {
                    foreach (XElement itdDepartureElement in departureListElement.Elements("itdDeparture"))
                    {
                        int id = int.Parse(this.GetAttributeValue(itdDepartureElement, "stopID", "cannot find attribute stopID of itdDeparture"));

                        StationDepartures stationDepartures = this.FindStationDepartures(departuresResult.StationDepartures, id);
                        if (stationDepartures == null)
                        {
                            string mapNameAttr = this.GetAttributeValueOrNull(itdDepartureElement, "mapName");
                            if (mapNameAttr == null || !"WGS84".Equals(mapNameAttr))
                                throw new Exception("unknown mapName: " + mapNameAttr);

                            int lon = int.Parse(this.GetAttributeValue(itdDepartureElement, "x", "cannot find attribute x of itdDeparture"));
                            int lat = int.Parse(this.GetAttributeValue(itdDepartureElement, "y", "cannot find attribute y of itdDeparture"));

                            stationDepartures = new StationDepartures(new Location(LocationType.STATION, id, lat, lon), new List<Departure>(), new List<LineDestination>());
                        }

                        string position = AbstractEfaProvider.NormalizePlatform(this.GetAttributeValueOrNull(itdDepartureElement, "platform"), this.GetAttributeValueOrNull(itdDepartureElement, "platformName"));
                        int countdown = int.Parse(this.GetAttributeValue(itdDepartureElement, "countdown", "cannot find attribute countdown of itdDeparture"));

                        XElement itdDateTime1 = itdDepartureElement.Element("itdDateTime");
                        if (itdDateTime1 == null)
                            throw new Exception("cannot find <itdDateTime />");

                        DateTime? plannedTime = new DateTime?(this.ProcessItdDateTime(itdDateTime1));

                        XElement itdRTDateTime = itdDepartureElement.Element("itdRTDateTime");
                        if (itdRTDateTime != null)
                            predictedTime = new DateTime?(this.ProcessItdDateTime(itdRTDateTime));

                        XElement itdServingLineElement = ((XContainer)itdDepartureElement).Element("itdServingLine");
                        if (itdServingLineElement == null)
                            throw new Exception("cannot find <itdServingLine />");

                        bool flag = "1".Equals(this.GetAttributeValueOrNull(itdServingLineElement, "realtime"));
                        string destination = this.NormalizeLocationName(this.GetAttributeValue(itdServingLineElement, "direction", "cannot find attribute direction of itdServingLine"));
                        int destinationId = int.Parse(this.GetAttributeValue(itdServingLineElement, "destID", "cannot find attribute destID of itdServingLine"));
                        string line = this.ProcessItdServingLine(itdServingLineElement);

                        if (flag && !predictedTime.HasValue)
                            predictedTime = plannedTime;

                        Departure departure = new Departure(plannedTime, predictedTime, countdown, line, (string)null, position, destinationId, destination, (string)null);
                        stationDepartures.Departures.Add(departure);
                    }
                }
                return departuresResult;
            }
            else if ("notidentified".Equals(itdOdvNameState))
                return new QueryDeparturesResult(QueryDeparturesResult.DepartureResultStatuses.INVALID_STATION);
            else
                throw new Exception("unknown nameState '" + itdOdvNameState + "' on " + uri);
        }

        private StationDepartures FindStationDepartures(List<StationDepartures> stationDepartures, int id)
        {
            foreach (StationDepartures stationDeparture in stationDepartures)
            {
                if (stationDeparture.Location.Id == id)
                    return stationDeparture;
            }
            return (StationDepartures)null;
        }

        private DateTime ProcessItdDateTime(XElement itdDateTime)
        {
            XElement itdDateElement = itdDateTime.Element("itdDate");
            int year = int.Parse(itdDateElement.Attribute("year").Value);
            int month = int.Parse(itdDateElement.Attribute("month").Value);
            int day = int.Parse(itdDateElement.Attribute("day").Value);

            if (year == 0)
                return new DateTime();

            XElement itdTimeElement = itdDateTime.Element("itdTime");
            int hour = int.Parse(itdTimeElement.Attribute("hour").Value);
            int minute = int.Parse(itdTimeElement.Attribute("minute").Value);
            if (hour == 24)
            {
                hour = 0;
                ++day;
            }
            return new DateTime(year, month, day, hour, minute, 0);
        }

        private string ProcessItdServingLine(XElement itdServingLine)
        {
            string motType = this.GetAttributeValueOrNull(itdServingLine, "motType");
            string number = this.GetAttributeValueOrNull(itdServingLine, "number");
            string noTrainName = null;

            XElement itdNoTrainElement = itdServingLine.Element("itdNoTrain");
            if (itdNoTrainElement != null)
                noTrainName = this.GetAttributeValueOrNull(itdNoTrainElement, "name");

            return this.ParseLine(motType, number, number, noTrainName);
        }

        protected string NormalizeLocationName(string name)
        {
            if (name == null || name.Length == 0)
                return null;
            else
                return Regex.Replace(name, AbstractEfaProvider.P_STATION_NAME_WHITESPACE, " ");
        }

        private string XsltTripRequest2Uri(Location from, Location via, Location to, DateTime date, bool dep, string products, WalkSpeed walkSpeed, bool forceReload)
        {
            string dateFormat = "yyyyMMdd";
            string timeFormat = "HHmm";
            StringBuilder uri = new StringBuilder(this._apiBase);
            uri.Append("XSLT_TRIP_REQUEST2");
            this.AppendCommonRequestParams(uri);
            uri.Append("&sessionID=0");
            uri.Append("&requestID=0");
            uri.Append("&language=de");

            if (forceReload)
                uri.Append("&guid=" + Guid.NewGuid().ToString());

            AbstractEfaProvider.AppendCommonXsltTripRequest2Params(uri);
            this.AppendLocation(uri, from, "origin");
            this.AppendLocation(uri, to, "destination");

            if (via != null)
                this.AppendLocation(uri, via, "via");

            uri.Append("&itdDate=").Append(Uri.EscapeUriString(date.ToString(dateFormat)));
            uri.Append("&itdTime=").Append(Uri.EscapeUriString(date.ToString(timeFormat)));
            uri.Append("&itdTripDateTimeDepArr=").Append(dep ? "dep" : "arr");
            uri.Append("&ptOptionsActive=1");
            uri.Append("&changeSpeed=").Append(AbstractEfaProvider.WALKSPEED_MAP[walkSpeed]);

            if (products != null)
            {
                uri.Append("&includedMeans=checkbox");
                bool flag = false;
                foreach (char ch in products.ToCharArray())
                {
                    if ((int)ch == 73 || (int)ch == 82)
                    {
                        uri.Append("&inclMOT_0=on");
                        if ((int)ch == 73)
                            flag = true;
                    }
                    if ((int)ch == 83)
                        uri.Append("&inclMOT_1=on");
                    if ((int)ch == 85)
                        uri.Append("&inclMOT_2=on");
                    if ((int)ch == 84)
                        uri.Append("&inclMOT_3=on&inclMOT_4=on");
                    if ((int)ch == 66)
                        uri.Append("&inclMOT_5=on&inclMOT_6=on&inclMOT_7=on");
                    if ((int)ch == 80)
                        uri.Append("&inclMOT_10=on");
                    if ((int)ch == 70)
                        uri.Append("&inclMOT_9=on");
                    if ((int)ch == 67)
                        uri.Append("&inclMOT_8=on");
                    uri.Append("&inclMOT_11=on");
                }
                if (!flag)
                    uri.Append("&lineRestriction=403");
            }
            uri.Append("&locationServerActive=1");
            uri.Append("&useRealtime=1");
            uri.Append("&useProxFootSearch=1");

            return uri.ToString();
        }

        private static string EscapeUmlauts(string uri)
        {
            return uri.Replace("ß", "%DF").Replace("ü", "%FC").Replace("ö", "%F6").Replace("ä", "%E4");
        }

        private string CommandLink(string sessionId, string requestId, string command)
        {
            StringBuilder uri = new StringBuilder(this._apiBase);
            uri.Append("XSLT_TRIP_REQUEST2");
            uri.Append("?sessionID=").Append(sessionId);
            uri.Append("&requestID=").Append(requestId);

            AbstractEfaProvider.AppendCommonXsltTripRequest2Params(uri);

            uri.Append("&command=").Append(command);
            uri.Append("&guid=").Append(Guid.NewGuid().ToString());

            return uri.ToString();
        }

        private static void AppendCommonXsltTripRequest2Params(StringBuilder uri)
        {
            uri.Append("&coordListOutputFormat=STRING");
            uri.Append("&calcNumberOfTrips=4");
        }

        public void QueryMoreConnections(Action<QueryConnectionsResult> callback, string uri)
        {
            new FileDownloader().Download(uri, (Action<string>)(result =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    if (callback == null)
                        return;
                    callback((QueryConnectionsResult)null);
                }
                else
                    AsyncHelper.SafeExecute((Action)(() =>
                    {
                        try
                        {
                            QueryConnectionsResult queryConnectionsResult = this.QueryConnections(uri, result);
                            if (callback == null)
                                return;
                            callback(queryConnectionsResult);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }));
            }), false);
        }

        public void QueryConnections(Action<QueryConnectionsResult> callback, Location from, Location via, Location to, DateTime date, bool dep, string products, WalkSpeed walkSpeed, bool forceReload)
        {
            string uri = this.XsltTripRequest2Uri(from, via, to, date, dep, products, walkSpeed, forceReload);
            new FileDownloader().Download(uri, (Action<string>)(result =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    if (callback == null)
                        return;
                    callback((QueryConnectionsResult)null);
                }
                else
                    AsyncHelper.SafeExecute((Action)(() =>
                    {
                        try
                        {
                            QueryConnectionsResult queryConnectionsResult = this.QueryConnections(uri, result);
                            if (callback == null)
                                return;
                            callback(queryConnectionsResult);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }));
            }), false);
        }

        private QueryConnectionsResult QueryConnections(string uri, string xml)
        {
            if (xml.Contains("Your session has expired."))
                return new QueryConnectionsResult(QueryConnectionsResult.Status.SESSION_EXPIRED);

            XDocument xdocument;

            try
            {
                xdocument = XDocument.Parse(xml);
            }
            catch (NotSupportedException ex)
            {
                return new QueryConnectionsResult(QueryConnectionsResult.Status.SESSION_EXPIRED);
            }

            XElement itdRequestElement = xdocument.Element("itdRequest");
            if (itdRequestElement == null)
                throw new Exception("cannot find <itdRequest />");

            string sessionId = this.GetAttributeValue(itdRequestElement, "sessionID", "cannot find attribute sessionID of itdRequest");
            XElement itdTripRequestElement = Enumerable.FirstOrDefault<XElement>(xdocument.Descendants("itdTripRequest"));

            if (itdTripRequestElement == null)
                throw new Exception("cannot find <itdTripRequest />");

            string requestId = this.GetAttributeValue(itdTripRequestElement, "requestID", "cannot find attribute requestID of itdTripRequest");
            if (itdTripRequestElement.Element("itdMessage") != null && int.Parse(itdTripRequestElement.Element("itdMessage").Attribute("code").Value) == -4000)
                return new QueryConnectionsResult(QueryConnectionsResult.Status.NO_CONNECTIONS);

            List<Location> ambiguousFrom = null;
            List<Location> ambiguousTo = null;
            List<Location> ambiguousVia = null;
            Location from = null;
            Location via = null;
            Location to = null;

            foreach (XElement itdOdvElement in itdTripRequestElement.Elements("itdOdv"))
            {
                string usage = this.GetAttributeValue(itdOdvElement, "usage", "cannot find attribute usage of itdOdv");
                string defaultPlace = this.ProcessItdOdvPlace(itdOdvElement);
                XElement itdOdvNameElement = itdOdvElement.Element("itdOdvName");

                if (itdOdvNameElement == null)
                    throw new Exception("cannot find <itdOdvName /> inside " + usage);

                string itdOdvNameState = this.GetAttributeValue(itdOdvNameElement, "state", "cannot find attribute state of itdOdvName");
                if ("list".Equals(itdOdvNameState))
                {
                    if ("origin".Equals(usage))
                    {
                        ambiguousFrom = new List<Location>();

                        foreach (XElement odvNameElement in itdOdvNameElement.Elements("odvNameElem"))
                        {
                            ambiguousFrom.Add(this.ProcessOdvNameElem(odvNameElement, defaultPlace));
                        }
                    }
                    else if ("via".Equals(usage))
                    {
                        ambiguousVia = new List<Location>();

                        foreach (XElement odvNameElement in itdOdvNameElement.Elements("odvNameElem"))
                        {
                            ambiguousVia.Add(this.ProcessOdvNameElem(odvNameElement, defaultPlace));
                        }
                    }
                    else
                    {
                        if (!"destination".Equals(usage))
                            throw new Exception("unknown usage: " + usage);

                        ambiguousTo = new List<Location>();

                        foreach (XElement odvNameElement in itdOdvNameElement.Elements("odvNameElem"))
                        {
                            ambiguousTo.Add(this.ProcessOdvNameElem(odvNameElement, defaultPlace));
                        }
                    }
                }
                else if ("identified".Equals(itdOdvNameState))
                {
                    XElement odvNameElement = itdOdvNameElement.Element("odvNameElem");

                    if (odvNameElement == null)
                        throw new Exception("cannot find <odvNameElem /> inside " + usage);

                    if ("origin".Equals(usage))
                    {
                        from = this.ProcessOdvNameElem(odvNameElement, defaultPlace);
                    }
                    else if ("via".Equals(usage))
                    {
                        via = this.ProcessOdvNameElem(odvNameElement, defaultPlace);
                    }
                    else
                    {
                        if (!"destination".Equals(usage))
                            throw new Exception("unknown usage: " + usage);

                        to = this.ProcessOdvNameElem(odvNameElement, defaultPlace);
                    }
                }
            }

            if (ambiguousFrom != null || ambiguousTo != null || ambiguousVia != null)
                return new QueryConnectionsResult(ambiguousFrom, ambiguousVia, ambiguousTo);

            XElement itdTripDateTimeElement = itdTripRequestElement.Element("itdTripDateTime");
            if (itdTripDateTimeElement == null)
                throw new Exception("cannot find <itdTripDateTime />");

            XElement itdDateTimeElement = itdTripDateTimeElement.Element("itdDateTime");
            if (itdDateTimeElement == null)
                throw new Exception("cannot find <itdDateTime />");

            XElement itdDateElement = itdDateTimeElement.Element("itdDate");
            if (itdDateElement == null)
                throw new Exception("cannot find <itdDate />");

            XElement itdMessageElement = itdDateElement.Element("itdMessage");
            if (itdMessageElement != null && itdMessageElement.Value.Equals("invalid date"))
                return new QueryConnectionsResult(QueryConnectionsResult.Status.INVALID_DATE);

            DateTime dateTime = new DateTime();
            List<Connection> connections = new List<Connection>();

            XElement itdRouteListElement = Enumerable.FirstOrDefault(itdTripRequestElement.Descendants("itdRouteList"));
            if (itdRouteListElement == null)
                return new QueryConnectionsResult(QueryConnectionsResult.Status.NO_CONNECTIONS);

            foreach (XElement itdRouteElement in itdRouteListElement.Elements("itdRoute"))
            {
                string routeTripIndex = (this.GetAttributeValueOrNull(itdRouteElement, "routeIndex") ?? "") + "-" + (this.GetAttributeValueOrNull(itdRouteElement, "routeTripIndex") ?? "");
                List<Connection.Part> parts = new List<Connection.Part>();
                Location departureLocation = (Location)null;
                DateTime? departureTime = new DateTime?();
                Location arrivalLocation = (Location)null;
                DateTime? arrivalTime = new DateTime?();

                foreach (XElement itdPartialRouteElement in itdRouteElement.Descendants("itdPartialRoute"))
                {
                    if (itdPartialRouteElement.Elements("itdPoint").Count() != 2)
                        throw new Exception("Departure and Arrival points expected in route");

                    XElement itdPointDeparture = Enumerable.ToList(itdPartialRouteElement.Elements("itdPoint"))[0];
                    XElement itdPointArrival = Enumerable.ToList(itdPartialRouteElement.Elements("itdPoint"))[1];
                    if (!"departure".Equals(this.GetAttributeValue(itdPointDeparture, "usage", "cannot find attribute usage of itdPointDeparture")))
                        throw new Exception("first point should be departure");

                    int departureStopId = int.Parse(this.GetAttributeValue(itdPointDeparture, "stopID", "cannot find attribute stopID of itdPointDeparture"));
                    string departureName = this.NormalizeLocationName(this.GetAttributeValue(itdPointDeparture, "name", "cannot find attribute name of itdPointDeparture"));
                    int latDeparture;
                    int lonDeparture;

                    if ("WGS84".Equals(this.GetAttributeValueOrNull(itdPointDeparture, "mapName")))
                    {
                        latDeparture = this.ParseLatLon(this.GetAttributeValue(itdPointDeparture, "y", "cannot find attribute y of itdPointDeparture"));
                        lonDeparture = this.ParseLatLon(this.GetAttributeValue(itdPointDeparture, "x", "cannot find attribute x of itdPointDeparture"));
                    }
                    else
                    {
                        latDeparture = 0;
                        lonDeparture = 0;
                    }

                    Location departure = new Location(LocationType.STATION, departureStopId, latDeparture, lonDeparture, (string)null, departureName);
                    if (departureLocation == null)
                        departureLocation = departure;

                    string departurePosition = AbstractEfaProvider.NormalizePlatform(
                        this.GetAttributeValue(itdPointDeparture, "platform", "cannot find attribute platform of itdPointDeparture"), 
                        this.GetAttributeValue(itdPointDeparture, "platformName", "cannot find attribute platformName of itdPointDeparture"));
                            
                    XElement itdDateTimeDeparture = itdPointDeparture.Element("itdDateTime");
                    if (itdDateTimeDeparture == null)
                        throw new Exception("cannot find <itdDateTime /> in itdPointDeparture");

                    DateTime depTime = this.ProcessItdDateTime(itdDateTimeDeparture);
                    if (!departureTime.HasValue)
                        departureTime = new DateTime?(depTime);

                    XElement itdDateTimeTargetDeparture = itdPointDeparture.Element("itdDateTimeTarget");

                    if (itdDateTimeTargetDeparture != null)
                        dateTime = this.ProcessItdDateTime(itdDateTimeTargetDeparture);

                    if (!"arrival".Equals(this.GetAttributeValue(itdPointArrival, "usage", "cannot find attribute usage of itdPointArrival")))
                        throw new Exception("second point should be arrival");

                    int arrivalStopId = int.Parse(this.GetAttributeValue(itdPointArrival, "stopID", "cannot find attribute stopID of itdPointArrival"));
                    string arrivalName = this.NormalizeLocationName(this.GetAttributeValue(itdPointArrival, "name", "cannot find attribute name of itdPointArrival"));
                    int latArrival;
                    int lonArrival;

                    if ("WGS84".Equals(this.GetAttributeValueOrNull(itdPointArrival, "mapName")))
                    {
                        latArrival = this.ParseLatLon(this.GetAttributeValue(itdPointArrival, "y", "cannot find attribute y of itdPointArrival"));
                        lonArrival = this.ParseLatLon(this.GetAttributeValue(itdPointArrival, "x", "cannot find attribute x of itdPointArrival"));
                    }
                    else
                    {
                        latArrival = 0;
                        lonArrival = 0;
                    }

                    Location arrival = new Location(LocationType.STATION, arrivalStopId, latArrival, lonArrival, (string)null, arrivalName);
                    arrivalLocation = arrival;

                    string arrivalPosition = AbstractEfaProvider.NormalizePlatform(this.GetAttributeValue(itdPointArrival, "platform", "cannot find attribute platform of itdPointArrival"), 
                        this.GetAttributeValue(itdPointArrival, "platformName", "cannot find attribute platformName of itdPointArrival"));

                    XElement itdDateTimeArrival = itdPointArrival.Element("itdDateTime");
                    if (itdDateTimeArrival == null)
                        throw new Exception("cannot find <itdDateTime /> in itdPointArrival");

                    DateTime arrTime = this.ProcessItdDateTime(itdDateTimeArrival);
                    arrivalTime = new DateTime?(arrTime);

                    XElement itdDateTimeTargetArrival = itdPointArrival.Element("itdDateTimeTarget");
                    if (itdDateTimeTargetArrival != null)
                        dateTime = this.ProcessItdDateTime(itdDateTimeTargetArrival);

                    XElement itdMeansOfTransport = itdPartialRouteElement.Element("itdMeansOfTransport");
                    if (itdMeansOfTransport == null)
                        throw new Exception("cannot find <itdMeansOfTransport /> in itdPartialRoute");

                    string productName = this.GetAttributeValueOrNull(itdMeansOfTransport, "productName");
                    if (productName.Equals("Fussweg") || productName.Equals("Taxi"))
                    {
                        int minutes = (arrTime - depTime).Minutes;
                        List<Point> path = null;

                        XElement itdPathCoordinates = itdMeansOfTransport.Element("itdPathCoordinates");
                        if (itdPathCoordinates != null)
                            path = this.ProcessItdPathCoordinates(itdPathCoordinates);

                        if (parts.Count > 0 && parts[parts.Count - 1] is Connection.Footway)
                        {
                            Connection.Footway footway = (Connection.Footway)parts[parts.Count - 1];
                            parts.RemoveAt(parts.Count - 1);
                            if (path != null && footway.path != null)
                                footway.path.AddRange(path);
                            parts.Add((Connection.Part)new Connection.Footway(footway.Min + minutes, footway.Departure, footway.DepartureTime, arrival, arrTime, footway.path));
                        }
                        else
                            parts.Add((Connection.Part)new Connection.Footway(minutes, departure, depTime, arrival, arrTime, path));
                    }
                    else if ("gesicherter Anschluss".Equals(productName))
                    {
                        parts.Add((Connection.Part)new Connection.SpecialTripPart(AppResources.GesicherterAnschluss, departure, arrival, (List<Point>)null));
                    }
                    else if ("nicht umsteigen".Equals(productName))
                    {
                        parts.Add((Connection.Part)new Connection.SpecialTripPart(AppResources.NichtUmsteigen, departure, arrival, (List<Point>)null));
                    }
                    else
                    {
                        string motDestId = this.GetAttributeValue(itdMeansOfTransport, "destID", "cannot find attribute destID of itdMeansOfTransport");
                        string motDestination = this.NormalizeLocationName(this.GetAttributeValue(itdMeansOfTransport, "destination", "cannot find attribute destination of itdMeansOfTransport"));

                        Location destination = motDestId.Length > 0 ?
                            new Location(LocationType.STATION, int.Parse(motDestId), (string)null, motDestination) :
                            new Location(LocationType.ANY, 0, (string)null, motDestination);

                        Line line = new Line(!"AST".Equals(this.GetAttributeValue(itdMeansOfTransport, "symbol", "cannot find attribute symbol of itdMeansOfTransport")) ?
                            this.ParseLine(this.GetAttributeValue(itdMeansOfTransport, "motType", "cannot find attribute motType of itdMeansOfTransport"),
                            this.GetAttributeValue(itdMeansOfTransport, "shortname", "cannot find attribute shortname of itdMeansOfTransport"),
                            this.GetAttributeValue(itdMeansOfTransport, "name", "cannot find attribute name of itdMeansOfTransport"), null) : "BAST");

                        List<Stop> intermediateStops = null;
                        XElement itdStopSequence = itdMeansOfTransport.Element("itdStopSeq");
                        if (itdStopSequence != null)
                        {
                            intermediateStops = new List<Stop>();

                            foreach (XElement itdPoint in itdStopSequence.Elements("itdPoint"))
                            {
                                int itdPointStopId = int.Parse(this.GetAttributeValue(itdPoint, "stopID", "cannot find attribute stopID of itdPoint"));
                                string itdPointName = this.NormalizeLocationName(this.GetAttributeValue(itdPoint, "name", "cannot find attribute name of itdPoint"));
                                int latStop;
                                int lonStop;

                                if ("WGS84".Equals(this.GetAttributeValue(itdPoint, "mapName", "cannot find attribute mapName of itdPoint")))
                                {
                                    latStop = int.Parse(this.GetAttributeValue(itdPoint, "y", "cannot find attribute y of itdPoint"));
                                    lonStop = int.Parse(this.GetAttributeValue(itdPoint, "x", "cannot find attribute x of itdPoint"));
                                }
                                else
                                {
                                    latStop = 0;
                                    lonStop = 0;
                                }

                                string position = AbstractEfaProvider.NormalizePlatform(this.GetAttributeValue(itdPoint, "platform", "cannot find attribute platform of itdPoint"),
                                    this.GetAttributeValue(itdPoint, "platformName", "cannot find attribute platformName of itdPoint"));

                                XElement itdDateTime = itdPoint.Element("itdDateTime");
                                if (itdDateTime != null)
                                {
                                    DateTime time = this.ProcessItdDateTime(itdDateTime);
                                    intermediateStops.Add(new Stop(new Location(LocationType.STATION, itdPointStopId, latStop, lonStop, null, itdPointName), position, time));
                                }
                            }

                            int count = intermediateStops.Count;
                            if (count >= 2)
                            {
                                if (intermediateStops[count - 1].location.Id != arrivalStopId)
                                    throw new Exception();

                                intermediateStops.RemoveAt(count - 1);

                                if (intermediateStops[0].location.Id != departureStopId)
                                    throw new Exception();

                                intermediateStops.RemoveAt(0);
                            }
                        }

                        List<Point> path = null;

                        XElement itdPathCoordinates = itdMeansOfTransport.Element("itdPathCoordinates");
                        if (itdPathCoordinates != null)
                            path = this.ProcessItdPathCoordinates(itdPathCoordinates);

                        parts.Add(new Connection.Trip(line, destination, depTime, departurePosition, departure, arrTime, arrivalPosition, arrival, intermediateStops, path));
                    }
                }

                List<IFare> listOfFare = new List<IFare>();
                XElement itdFareElement = Enumerable.FirstOrDefault<XElement>(itdTripRequestElement.Descendants("itdFare"));

                if (itdFareElement != null)
                {
                    XElement itdSingleTicketElement = Enumerable.FirstOrDefault<XElement>(itdFareElement.Descendants("itdSingleTicket"));
                    if (itdSingleTicketElement != null)
                    {
                        string network = this.GetAttributeValueOrNull(itdSingleTicketElement, "net");
                        string currency = this.GetAttributeValueOrNull(itdSingleTicketElement, "currency");
                        string adultPrice = this.GetAttributeValueOrNull(itdSingleTicketElement, "fareAdult");
                        string childPrice = this.GetAttributeValueOrNull(itdSingleTicketElement, "fareChild");
                        string unitName = this.GetAttributeValueOrNull(itdSingleTicketElement, "unitName");
                        string adultUnits = this.GetAttributeValueOrNull(itdSingleTicketElement, "unitsAdult").Trim();
                        string childUnits = this.GetAttributeValueOrNull(itdSingleTicketElement, "unitsChild").Trim();
                        string str = (string)null;

                        if (itdSingleTicketElement.HasElements)
                        {
                            XElement itdGenericTicketElement = (from ticket in itdSingleTicketElement.Descendants("itdGenericTicket")
                                                               where ticket.Element("ticket") != null && ticket.Element("ticket").Value == "MVV_NRTOUCHEDZONES"
                                                               select ticket).FirstOrDefault();

                            if (itdGenericTicketElement != null)
                            {
                                XElement itdGenericTicket = itdGenericTicketElement.Element("value");
                                if (itdGenericTicket != null && !string.IsNullOrEmpty(itdGenericTicket.Value))
                                    str = itdGenericTicket.Value.Trim();
                            }
                        }
                            
                        if (!string.IsNullOrEmpty(adultPrice))
                        {
                            listOfFare.Add(new Fare(network, 
                                                    Fare.SINGLE_TICKET, 
                                                    currency, 
                                                    float.Parse(adultPrice, (IFormatProvider)this._nfi), 
                                                    float.Parse(childPrice, (IFormatProvider)this._nfi), 
                                                    "Zonen", str, str));
                        }
                            
                        if (itdSingleTicketElement.HasElements)
                        {
                            foreach (XElement itdGenericTicketGroup in itdSingleTicketElement.Descendants("itdGenericTicketGroup"))
                            {
                                Fare fare = this.ProcessItdGenericTicketGroup(itdGenericTicketGroup, network, currency, unitName, adultUnits, childUnits);
                                if (fare != null)
                                    listOfFare.Add(fare);
                            }
                        }
                    }

                    XElement itdCommuterFares = ((XContainer)itdFareElement).Element("itdCommuterFares");
                    if (itdCommuterFares != null)
                    {
                        float weekAdult = float.Parse(this.GetAttributeValue(itdCommuterFares, "weekAdult", "Cannot find attribute weekAdult on itdCommuterFares"), this._nfi);
                        float weekChild = float.Parse(this.GetAttributeValue(itdCommuterFares, "weekChild", "Cannot find attribute weekChild on itdCommuterFares"), this._nfi);
                        float monthAdult = float.Parse(this.GetAttributeValue(itdCommuterFares, "monthAdult", "Cannot find attribute monthAdult on itdCommuterFares"), this._nfi);
                        float monthChild = float.Parse(this.GetAttributeValue(itdCommuterFares, "monthChild", "Cannot find attribute monthChild on itdCommuterFares"), this._nfi);
                        float weekEducation = float.Parse(this.GetAttributeValue(itdCommuterFares, "weekEducation", "Cannot find attribute weekEducation on itdCommuterFares"), this._nfi);
                        float monthEducation = float.Parse(this.GetAttributeValue(itdCommuterFares, "monthEducation", "Cannot find attribute monthEducation on itdCommuterFares"), this._nfi);
                        
                        XElement itdTariffzones = itdFareElement.Element("itdTariffzones");
                        List<int> zonesList = new List<int>();
                        string zonesString = null;

                        if (itdTariffzones != null)
                        {
                            foreach (XElement zoneElem in itdTariffzones.Descendants("zoneElem"))
                            {
                                zonesList.Add(int.Parse(zoneElem.Value));
                            }
                            zonesList.Sort();
                            if (zonesList.Count > 1)
                            {
                                zonesString = zonesList[0].ToString() 
                                    + " - " + zonesList[zonesList.Count - 1].ToString() 
                                    + " (" + (1 + zonesList[zonesList.Count - 1] - zonesList[0]).ToString() 
                                    + " " + AppResources.Rings + ")";
                            }
                            else if (zonesList.Count > 0)
                                zonesString = zonesList[0].ToString() + " (1 " + AppResources.Ring + ")";
                        }
                        CommuterFare commuterFare = new CommuterFare(weekAdult, weekChild, weekEducation, monthAdult, monthChild, monthEducation, zonesString);
                        listOfFare.Add(commuterFare);
                    }
                }
                connections.Add(new Connection(routeTripIndex, uri, departureTime.Value, arrivalTime.Value, departureLocation, arrivalLocation, parts, listOfFare.Count <= 0 ? null : listOfFare));
            }
            return new QueryConnectionsResult(uri, from, via, to, this.CommandLink(sessionId, requestId, "tripPrev"), this.CommandLink(sessionId, requestId, "tripNext"), connections);
        }

        private int ParseLatLon(string stringValue)
        {
            int result = 0;

            if (stringValue.Contains("."))
                stringValue = stringValue.Substring(0, stringValue.IndexOf("."));

            int.TryParse(stringValue, out result);

            return result;
        }

        private List<Point> ProcessItdPathCoordinates(XElement itdPathCoordinates)
        {
            List<Point> pointList = new List<Point>();
            string coordEllipsoid = itdPathCoordinates.Element("coordEllipsoid").Value;

            if (!"WGS84".Equals(coordEllipsoid))
                throw new Exception("unknown ellipsoid: " + coordEllipsoid);

            string coordType = itdPathCoordinates.Element("coordType").Value;

            if (!"GEO_DECIMAL".Equals(coordType))
                throw new Exception("unknown type: " + coordType);

            string itdCoordinateString = itdPathCoordinates.Element("itdCoordinateString").Value;

            foreach (string coordParts in itdCoordinateString.Split(' '))
            {
                string[] latLonParts = coordParts.Split(',');
                pointList.Add(new Point(int.Parse(latLonParts[1]), int.Parse(latLonParts[0])));
            }
            return pointList;
        }

        private Fare ProcessItdGenericTicketGroup(XElement itdGenericTicketGroup, string net, string currency, string unitName, string adultUnits, string childUnits)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (XElement itdGenericTicketElement in itdGenericTicketGroup.Elements("itdGenericTicket"))
            {
                XElement ticketElement = itdGenericTicketElement.Element("ticket");
                if (ticketElement == null)
                    throw new Exception("cannot find element ticket in itdGenericTicket");

                string ticketInfoKey = ticketElement.Value;
                string ticketInfoValue = null;

                XElement valueElement = itdGenericTicketElement.Element("value");
                if (valueElement == null)
                    throw new Exception("Cannot find element value in itdGenericTicket");

                if (!string.IsNullOrEmpty(valueElement.Value))
                    ticketInfoValue = valueElement.Value.Trim();

                if (!string.IsNullOrEmpty(ticketInfoKey) && !string.IsNullOrEmpty(ticketInfoValue))
                    dictionary.Add(ticketInfoKey, ticketInfoValue);
            }

            float adultPrice = 0.0f;
            float childPrice = 0.0f;

            if (dictionary.ContainsKey("FARE_ADULT"))
                adultPrice = float.Parse(dictionary["FARE_ADULT"], (IFormatProvider)this._nfi);

            if (dictionary.ContainsKey("FARE_CHILD"))
                childPrice = float.Parse(dictionary["FARE_CHILD"], (IFormatProvider)this._nfi);

            if (!dictionary.ContainsKey("TICKETTYPE"))
                return (Fare)null;

            Fare fare = new Fare(net, dictionary["TICKETTYPE"], currency, adultPrice, childPrice, null, null, null);
            if (dictionary["TICKETTYPE"] == Fare.STREIFENPREIS)
            {
                fare.UnitName = unitName;
                fare.AdultUnits = adultUnits;
                fare.ChildUnits = childUnits;
            }
            return fare;
        }

        private static string NormalizePlatform(string platform, string platformName)
        {
            if (platform != null && platform.Length > 0)
            {
                Match match = Regex.Match(platform, AbstractEfaProvider.P_PLATFORM);
                if (match.Success)
                    return match.Groups[1].Value;
                else
                    return platform;
            }
            else
            {
                if (platformName == null || platformName.Length <= 0)
                    return (string)null;

                Match match = Regex.Match(platformName, AbstractEfaProvider.P_PLATFORM_NAME);
                if (!match.Success)
                    return platformName;
                string str = match.Groups[1].Value;
                if (match.Groups[2] != null && match.Groups[3] != null)
                    return str + match.Groups[2] + "-" + match.Groups[3];
                else if (match.Groups[2] != null)
                    return str + match.Groups[2];
                else
                    return str;
            }
        }

        private void AppendLocation(StringBuilder uri, Location location, string paramSuffix)
        {
            if (this._canAcceptPoiID && location.Type == LocationType.POI && location.HasId())
            {
                uri.Append("&type_").Append(paramSuffix).Append("=poiID");
                uri.Append("&name_").Append(paramSuffix).Append("=").Append(location.Id);
            }
            else if ((location.Type == LocationType.POI || location.Type == LocationType.ADDRESS) && location.HasLocation())
            {
                NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
                numberFormatInfo.NumberDecimalSeparator = ".";
                uri.Append("&type_").Append(paramSuffix).Append("=coord")
                    .Append("&name_").Append(paramSuffix).Append("=")
                    .Append(string.Format(numberFormatInfo, "{0:0.######}:{1:0.######}", location.Lon / 1000000.0, location.Lat / 1000000.0 ))
                    .Append(":WGS84");
            }
            else
            {
                uri.Append("&type_").Append(paramSuffix).Append("=").Append(AbstractEfaProvider.LocationTypeValue(location));
                uri.Append("&name_").Append(paramSuffix).Append("=").Append(Uri.EscapeUriString(AbstractEfaProvider.LocationValue(location)));
            }
        }

        protected static string LocationTypeValue(Location location)
        {
            LocationType type = location.Type;
            switch (type)
            {
                case LocationType.STATION:
                    return "stop";
                case LocationType.ADDRESS:
                    return "any";
                case LocationType.POI:
                    return "poi";
                case LocationType.ANY:
                    return "any";
                default:
                    throw new ArgumentException(type.ToString());
            }
        }

        protected static string LocationValue(Location location)
        {
            string str = string.Empty;
            return AbstractEfaProvider.EscapeUmlauts(location.Type != LocationType.STATION && location.Type != LocationType.POI || !location.HasId() ? 
                                                     location.Name : location.Id.ToString());
        }

        public abstract NetworkId id();
    }
}
