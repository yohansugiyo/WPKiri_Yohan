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

namespace Kiri
{
    public partial class mapTo : PhoneApplicationPage
    {
        public mapTo()
        {
            InitializeComponent();
        }
        private void Map_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GeoCoordinate locFrom = this.MyMapTo.ConvertViewportPointToGeoCoordinate(e.GetPosition(this.MyMapTo));
        }
    }
}