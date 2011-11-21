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
using System.Device.Location;

namespace BusCon.Maps
{
    public class CustomAdressResolver : ICivicAddressResolver
    {
        GeocodeService.GeocodeServiceClient proxy = null;

        public CustomAdressResolver()
        {
            proxy = new GeocodeService.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            proxy.ReverseGeocodeCompleted += new EventHandler<GeocodeService.ReverseGeocodeCompletedEventArgs>(proxy_ReverseGeocodeCompleted);
        }

        public CivicAddress ResolveAddress(GeoCoordinate coordinate)
        {
            return new CivicAddress();
        }

        public event EventHandler<ResolveAddressCompletedEventArgs> ResolveAddressCompleted;

        public void ResolveAddressAsync(GeoCoordinate coordinate)
        {
            proxy.ReverseGeocodeAsync(
                new GeocodeService.ReverseGeocodeRequest()
                {
                    Location = new Microsoft.Phone.Controls.Maps.Platform.Location()
                    {
                        Latitude = coordinate.Latitude,
                        Longitude = coordinate.Longitude,
                        Altitude = coordinate.Altitude
                    },

                    Credentials = new Microsoft.Phone.Controls.Maps.Credentials()
                    {
                        ApplicationId = "Ai3-zu4n1zp5l69w0p6u8FHlGj9UdvBu75ChCJWK0hEu7UCA4upUUsAFyT2Ffd3R"
                    }
                }
            );
        }

        void proxy_ReverseGeocodeCompleted(object sender, GeocodeService.ReverseGeocodeCompletedEventArgs e)
        {
            // e.Result.Results[0].Address.District = "BY"
            //proxy.ReverseGeocodeCompleted -= proxy_ReverseGeocodeCompleted;

            if (ResolveAddressCompleted != null)
            {
                ResolveAddressCompleted(sender, new ResolveAddressCompletedEventArgs(
                                    e.Result != null ?
                                        new CivicAddress()
                                        {
                                            AddressLine1 = e.Result.Results[0].Address.AddressLine,
                                            AddressLine2 = e.Result.Results[0].Address.FormattedAddress,
                                            Building = "",
                                            City = e.Result.Results[0].Address.Locality,
                                            CountryRegion = e.Result.Results[0].Address.CountryRegion,
                                            FloorLevel = "0",
                                            PostalCode = e.Result.Results[0].Address.PostalCode,
                                            StateProvince = e.Result.Results[0].Address.PostalTown
                                        } : null,
                                    e.Error,
                                    e.Cancelled,
                                    e.UserState)
                );
            }
        }
    }
}
