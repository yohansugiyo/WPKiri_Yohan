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
        public Double coorLat = 0.0; //device coordinate
        public Double coorLong = 0.0;
        public Double coorLatFrom = 0.0; //from coordinate
        public Double coorLongFrom = 0.0;
        public Double coorLatTo = 0.0; ////to coordinate
        public Double coorLongTo = 0.0;

        public string addressDevice = "";
        public string addressFrom = "";
        public string addressTo = "";

        public GeoCoordinate myGeoCoordinate;
        public Geolocator geolocator;
        private ReverseGeocodeQuery MyReverseGeocodeQuery = null;
        private GeoCoordinate MyCoordinate = null;
        public double accuracy;

        public LocationFinder() {
            this.geolocator = new Geolocator();
            this.geolocator.DesiredAccuracy = PositionAccuracy.High;
            this.geolocator.MovementThreshold = 20; // The units are meters.
            this.accuracy = 0.0;
            this.myGeoCoordinate = new GeoCoordinate();
            findLocation();
        }

        public async void findLocation()
        {
            // Get my current location.
            try
            {
                Geoposition myGeoposition = await geolocator.GetGeopositionAsync(
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromSeconds(10)    
                );
                this.accuracy = myGeoposition.Coordinate.Accuracy;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
                    myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
                    GetCurrentCoordinate((Double)myGeoCoordinate.Latitude, (Double)myGeoCoordinate.Longitude,"device");
                });
            }
            catch (Exception ex)
            {
                // Couldn't get current location - location might be disabled in settings
                MessageBox.Show("Tidak dapat menemukan lokasi");
            }
        }

        public async void updateAccuracy()
        {
            Geoposition myGeoposition = await geolocator.GetGeopositionAsync(
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromSeconds(10)
                );
            this.accuracy = myGeoposition.Coordinate.Accuracy;
        }


        public void setPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                coorLat = args.Position.Coordinate.Latitude;
                coorLong = args.Position.Coordinate.Longitude;
                updateAccuracy();
            });
        }

        //Get address zhttps://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn631249.aspx
        //old zhttp://stackoverflow.com/questions/16685088/windows-phone-reversegeocoding-to-get-address-from-lat-and-long

        //from zhttp://blogs.msdn.com/b/jrspinella/archive/2012/11/02/using-reversegeocodequery-for-windows-phone-8.aspx
        public void GetCurrentCoordinate(Double latitude, Double longitude,String paramFor)
        {
             try
             {
             MyCoordinate = new GeoCoordinate(latitude, longitude);
 
                 if (MyReverseGeocodeQuery == null || !MyReverseGeocodeQuery.IsBusy)
                 {
                     MyReverseGeocodeQuery = new ReverseGeocodeQuery();
                     MyReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(MyCoordinate.Latitude, MyCoordinate.Longitude);
                     if (paramFor.Equals("from"))
                     {
                         MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQueryFrom_QueryCompleted;
                         this.coorLatFrom = latitude;
                         this.coorLongFrom = longitude;
                     }
                     else if (paramFor.Equals("to"))
                     {
                         MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQueryTo_QueryCompleted;
                         this.coorLatTo = latitude;
                         this.coorLongTo = longitude;
                     }
                     else 
                     {
                         MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
                         this.coorLat = latitude;
                         this.coorLong = longitude;
                     }
                     MyReverseGeocodeQuery.QueryAsync();
                 }
             }
             catch (Exception ex)
             {
             }
           
        }

        private void ReverseGeocodeQueryFrom_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress add = e.Result[0].Information.Address;
                    addressFrom = add.Street;
                }
            }
        }

        private void ReverseGeocodeQueryTo_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress add = e.Result[0].Information.Address;
                    addressTo = add.Street;
                }
            }
        }

        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
             if (e.Error == null)
             {
                 if (e.Result.Count > 0)
                 {
                     MapAddress add = e.Result[0].Information.Address;
                     addressDevice = add.Street;
                 } 
             }
        } 

        public void setCoordinateHere(string paramFor){
            if (paramFor.Equals("from"))
            {
                this.coorLatFrom = this.coorLat;
                this.coorLongFrom = this.coorLong;
                this.addressFrom = this.addressDevice;
            }
            else {
                this.coorLatTo = this.coorLat;
                this.coorLongTo = this.coorLong;
                this.addressTo = this.addressDevice;
            }
        }

        public void reset() { 
            this.coorLatFrom = 0.0; //from coordinate
            this.coorLongFrom = 0.0;
            this.coorLatTo = 0.0; ////to coordinate
            this.coorLongTo = 0.0;

            this.addressDevice = "";
            this.addressFrom = "";
        }
 
    }
}
