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
        public mapFrom()
        {
            InitializeComponent();
            ShowMyLocationOnTheMap();
        }

        /*
        private void Map_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GeoCoordinate locFrom = this.MyMapFrom.ConvertViewportPointToGeoCoordinate(e.GetPosition(this.MyMapFrom));
        }
         * */

        private async void ShowMyLocationOnTheMap()
        {
            // Get my current location.
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);

            // Make my current location the center of the Map.
            this.MyMapFrom.Center = new GeoCoordinate(-6.9186295, 107.6121397);
            this.MyMapFrom.ZoomLevel = 13;

            // Create a small circle to mark the current location.
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Red);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            //Create Button
            Button addLocation = new Button();

            //Pushpin pushpin = (Pushpin)this.FindName("MyPushpin");
            //pushpin.GeoCoordinate = new GeoCoordinate(-6.9186295, 107.6121397);

            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = new GeoCoordinate(-6.9186295, 107.6121397);

            // Create a MapOverlay to contain the pushpin.
            //MapOverlay myLocationOverlay2 = new MapOverlay();
            //myLocationOverlay2.Content = pushpin;
            //myLocationOverlay2.PositionOrigin = new Point(0.5, 0.5);
            //myLocationOverlay2.GeoCoordinate = myGeoCoordinate;

            // Create a MapOverlay to contain the button addLocation.
            MapOverlay myLocationAdd = new MapOverlay();
            myLocationAdd.Content = addLocation;
            myLocationAdd.PositionOrigin = new Point(0.5, 0.5);
            myLocationAdd.GeoCoordinate = new GeoCoordinate(-6.9186295, 107.6121397);

            // Create a MapLayer to contain the MapOverlay.
            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            // Add the MapLayer to the Map.
            MyMapFrom.Layers.Add(myLocationLayer);
        }
    }
}