using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;

using System.Device.Location; // Provides the GeoCoordinate class.
using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;

namespace Kiri
{
    public partial class mapFrom : PhoneApplicationPage
    {
        private LocationFinder lFinder;
        public string locationMapsFrom;
        public string locationMapsTo;
        public string fromMapFor;

        public mapFrom()
        {
            this.lFinder = new LocationFinder();
            this.locationMapsFrom = "";
            InitializeComponent();
            ShowMyLocationOnTheMap();
            MyMapFrom.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(map_Tap);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NavigationContext.QueryString.TryGetValue("fromMapFor", out fromMapFor)) ;
            this.fromMapFor = fromMapFor + "";
        }

        private async void ShowMyLocationOnTheMap()
        {

            // Get my current location.
            loadingFrom.IsIndeterminate = true;
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
            this.MyMapFrom.Center = myGeoCoordinate;
            this.MyMapFrom.ZoomLevel = 13;
            loadingFrom.IsIndeterminate = false;
        }

        private void map_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ButtonPilih.Visibility = Visibility.Visible;
            Point p = e.GetPosition(MyMapFrom);
            GeoCoordinate s = MyMapFrom.ConvertViewportPointToGeoCoordinate(p);
            MyMapFrom.Layers.Clear();

            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0, 0);
            myLocationOverlay.GeoCoordinate = s;

            locationMapsFrom = (float)s.Latitude + "," +(float)s.Longitude;

            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            MyMapFrom.Layers.Add(myLocationLayer);
        }

        private void pilihLokasiAsal(object sender, RoutedEventArgs e)
        {
            if (fromMapFor.Equals("from"))
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml?locMapsFrom=" + locationMapsFrom, UriKind.Relative));
            }
            else 
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml?locMapsTo=" + locationMapsFrom, UriKind.Relative));
            }
        }
    }
}