namespace BusCon.PTE
{
    public class VvmProvider : AbstractEfaProvider
    {
        public static readonly NetworkId NETWORK_ID = NetworkId.VVM;
        private static readonly string API_BASE = "http://efa.mobilitaetsverbund.de/web/";
        private static readonly string NEARBY_STATION_URI = VvmProvider.API_BASE + 
            "XSLT_DM_REQUEST?outputFormat=XML&coordOutputFormat=WGS84&type_dm=stop&name_dm=%s&itOptionsActive=1&ptOptionsActive=1&useProxFootSearch=1&mergeDep=1&useAllStops=1&mode=direct";

        static VvmProvider()
        {
        }

        public VvmProvider()
            : base(VvmProvider.API_BASE, additionalQueryParameter: (string)null, canAcceptPoiID: false)
        {
        }

        public override NetworkId id()
        {
            return VvmProvider.NETWORK_ID;
        }
    }
}
