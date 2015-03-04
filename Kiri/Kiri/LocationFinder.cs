using System;
using System.Collections.Generic;
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
        public long coorLat;
        public long coorLong;

        public LocationFinder() { 
        }

        public async void OneShotLocation_Click()
        {

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                coorLat = long.Parse(geoposition.Coordinate.Latitude.ToString("0.00"));
                coorLong = long.Parse(geoposition.Coordinate.Longitude.ToString("0.00"));
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    // fromBox.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                }
            }
        }
    }
}
