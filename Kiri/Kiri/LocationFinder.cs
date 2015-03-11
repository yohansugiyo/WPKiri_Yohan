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

        public LocationFinder() {
            this.myGeoCoordinate = new GeoCoordinate();
            OneShotLocation_Click();
        }

        public async void OneShotLocation_Click()
        {
            // Get my current location.
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);

            this.coorLat = (Double)myGeoCoordinate.Latitude;
            this.coorLong = (Double)myGeoCoordinate.Longitude;
        }
    }
}
