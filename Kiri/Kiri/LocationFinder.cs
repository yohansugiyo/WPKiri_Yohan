using Microsoft.Phone.Maps.Services;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;

namespace Kiri
{
    class LocationFinder
    {
        public Double coorLat;
        public Double coorLong;
        public GeoCoordinate myGeoCoordinate;
        public Geolocator geolocator;

        public LocationFinder() {
            this.geolocator = new Geolocator();
            this.geolocator.DesiredAccuracy = PositionAccuracy.High;
            this.geolocator.MovementThreshold = 100; // The units are meters.

            this.myGeoCoordinate = new GeoCoordinate();
            OneShotLocation();
        }

        public async void OneShotLocation()
        {
            // Get my current location.
            try
            {
                Geolocator myGeolocator = new Geolocator();
                Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync(
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromSeconds(10)    
                );
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
                    myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);

                    this.coorLat = (Double)myGeoCoordinate.Latitude;
                    this.coorLong = (Double)myGeoCoordinate.Longitude;
                });
            }
            catch (Exception ex)
            {
                // Couldn't get current location - location might be disabled in settings
                MessageBox.Show("Tidak dapat menemukan lokasi");
            }
        }


        public void setPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                coorLat = args.Position.Coordinate.Latitude;
                coorLong = args.Position.Coordinate.Longitude;
            });
        }

        //Get address zhttps://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn631249.aspx
        //zhttp://stackoverflow.com/questions/16685088/windows-phone-reversegeocoding-to-get-address-from-lat-and-long
        private string getAddress(Double latitude, Double longitude)
        {
            string address = "";
            List<MapLocation> locations;
            ReverseGeocodeQuery query = new ReverseGeocodeQuery();
            query.GeoCoordinate = new GeoCoordinate(latitude, longitude);
            query.QueryCompleted += (s, e) =>
            {
                if (e.Error == null && e.Result.Count > 0)
                {
                    locations = e.Result as List<MapLocation>;
                    address = locations[0].Information.Address.ToString();
                    // Do whatever you want with returned locations. 
                    // e.g. MapAddress address = locations[0].Information.Address;
                }
            };
            query.QueryAsync();
            return address;
        }
    }
}
