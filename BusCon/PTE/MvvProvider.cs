namespace BusCon.PTE
{
    public class MvvProvider : AbstractEfaProvider
    {
        public static readonly NetworkId NETWORK_ID = NetworkId.MVV;
        private static readonly string API_BASE = "http://efa.mvv-muenchen.de/mobile/";
        private static readonly string NEARBY_STATION_URI = MvvProvider.API_BASE + 
            "XSLT_DM_REQUEST?outputFormat=XML&coordOutputFormat=WGS84&type_dm=stop&name_dm=%s&itOptionsActive=1&ptOptionsActive=1&useProxFootSearch=1&mergeDep=1&useAllStops=1&mode=direct";

        static MvvProvider()
        {
        }

        public MvvProvider()
            : base(MvvProvider.API_BASE, additionalQueryParameter: (string)null, canAcceptPoiID: false)
        {
        }

        public override NetworkId id()
        {
            return MvvProvider.NETWORK_ID;
        }
    }
}
