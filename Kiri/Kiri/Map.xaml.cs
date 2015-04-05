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

using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;

namespace Kiri
{
    public partial class Map : PhoneApplicationPage
    {
        private LocationFinder lFinder;
        public string fromMapFor;
        public string address;

        public Map()
        {
            this.lFinder = null ;
            InitializeComponent();
            MyMap.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(map_Tap);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NavigationContext.QueryString.TryGetValue("fromMapFor", out fromMapFor)) ;
            this.fromMapFor = fromMapFor + "";
            if (PhoneApplicationService.Current.State.ContainsKey("location"))
            {
                this.lFinder = (LocationFinder)PhoneApplicationService.Current.State["location"];
            }
            ShowMyLocationOnTheMap();
        }

        private void ShowMyLocationOnTheMap()
        {

            // Get my current location.
            loadingFrom.IsIndeterminate = true;
            GeoCoordinate myGeoCoordinate = new GeoCoordinate(lFinder.coorLat, lFinder.coorLong);
            this.MyMap.Center = myGeoCoordinate;
            this.MyMap.ZoomLevel = 13;
            loadingFrom.IsIndeterminate = false;
        }

        private void map_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ButtonPilih.Visibility = Visibility.Visible;
            Point p = e.GetPosition(MyMap);
            GeoCoordinate s = MyMap.ConvertViewportPointToGeoCoordinate(p);
            MyMap.Layers.Clear();

            Ellipse myCircle = new Ellipse();
            myCircle.Stroke = new SolidColorBrush(Colors.Black);
            myCircle.StrokeThickness = 4;
            myCircle.Fill = new SolidColorBrush(Colors.Green);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0, 0);
            myLocationOverlay.GeoCoordinate = s;

            if (fromMapFor.Equals("from"))
            {
                this.lFinder.GetCurrentCoordinate(s.Latitude, s.Longitude,"from");
            }
            else {
                this.lFinder.GetCurrentCoordinate(s.Latitude, s.Longitude, "to");
            }
            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            MyMap.Layers.Add(myLocationLayer);
        }

        private void pilihLokasi(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["location"] = lFinder;
            if (fromMapFor.Equals("from"))
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml?for=from", UriKind.Relative));
            }
            else 
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml?for=to", UriKind.Relative));
            }
        }
    }
}