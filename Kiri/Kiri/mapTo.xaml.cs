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
using Windows.Devices.Geolocation;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;

namespace Kiri
{
    public partial class mapTo : PhoneApplicationPage
    {
        private LocationFinder lFinder;
        public string locationMapsTo;

        public mapTo()
        {
            this.lFinder = new LocationFinder();
            this.locationMapsTo = "";
            InitializeComponent();
            ShowMyLocationOnTheMap();
            MyMapTo.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(map_Tap);
        }

        private async void ShowMyLocationOnTheMap()
        {
            loadingTo.IsIndeterminate = true;
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
            this.MyMapTo.Center = myGeoCoordinate;
            this.MyMapTo.ZoomLevel = 13;
            loadingTo.IsIndeterminate = false;
        }

        private void map_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ButtonPilih.Visibility = Visibility.Visible;
            Point p = e.GetPosition(MyMapTo);
            GeoCoordinate s = MyMapTo.ConvertViewportPointToGeoCoordinate(p);
            MyMapTo.Layers.Clear();

            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0, 0);
            myLocationOverlay.GeoCoordinate = s;

            locationMapsTo = s.Latitude + ", " + s.Longitude;

            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            MyMapTo.Layers.Add(myLocationLayer);
        }

        private void pilihLokasiTujuan(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?locMapsTo=" + locationMapsTo, UriKind.Relative));
        }
    }
}