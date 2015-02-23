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
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kiri
{
    public partial class test : PhoneApplicationPage
    {
        public test()
        {
            InitializeComponent();

            map.Center = new GeoCoordinate(-6.8619546, 107.614441);
            map.ZoomLevel = 13;
         
            /*
            MapPolyline line = new MapPolyline();
            line.StrokeColor = Colors.Red;
            line.StrokeThickness = 10;
            line.Path.Add(new GeoCoordinate(-6.8619546, 107.614441));
            line.Path.Add(new GeoCoordinate(-6.908693, 107.611185));
            map.MapElements.Add(line);
             */

            MapOverlay overlay = new MapOverlay
            {
                GeoCoordinate = map.Center,
                Content = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromArgb(120, 255, 0, 0)),
                    Child = new TextBlock(){Text="Pushpin"},
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromArgb(120,255,0,0)),
                    Width = 80,
                    Height = 60
                }
            };
            MapLayer layer = new MapLayer();
            layer.Add(overlay);

            map.Layers.Add(layer);

        }
    }
}