using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Device.Location; // Provides the GeoCoordinate class.
using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;

//optional
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Kiri
{
    public partial class ShowRoute : PhoneApplicationPage
    {
        public ShowRoute()
        {
            InitializeComponent();
            Find();
        }

        public void Find(){
            //Testing
            HttpClient httpClient = new HttpClient();

            String uri = "http://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.87474,107.60491&finish=-6.88909,107.59614&presentation=desktop&apikey=97A7A1157A05ED6F";
            Task<string> requestRoute = httpClient.GetStringAsync(new Uri(uri));

            requestRoute.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    //keluaranFrom.Text = requestFrom.Result;

                    RootObjectFindRoute r = new RootObjectFindRoute(); //Untuk Asal

                    //buang array
                    int counter=0;
                    string s= requestRoute.Result;
                    /*
                    for (int c = 0; c < s.Length; c++) {
                        if(s[c]=='['&&counter==0){
                            s = s.Remove(c,1);
                            counter++;
                        }
                    }
                    counter = 0;
                    for (int c = s.Length-1; c >=0; c--)
                    {
                        if (s[c] == ']' && counter == 0)
                        {
                            s = s.Remove(c, 1);
                            counter++;
                        }
                    }
                    MessageBox.Show(s);
                     */
                    MessageBox.Show(s);
                    r = Deserialize<RootObjectFindRoute>(s); //Mengubah String menjadi objek

                    
                    //Pushpin pic = new Pushpin();
                    Polyline routeRoad = new Polyline();
                    routeRoad.StrokeThickness = 2;

                    PointCollection point = new PointCollection();
                    for (int i = 0; i < r.routingresults.Count; i++){
                        //for(int j = 0;j<r.routingresults[i].steps.Count;j++){
                            MessageBox.Show(r.routingresults[i].steps.ToString());
                        //}
                        //point.Add();..Items.Add(r.routingresults[c].placename);
                    }

                    // Add the list box to a parent container in the visual tree.
                    routeRoad.Points =  point;
                    LayoutRoot.Children.Add(routeRoad);
                    
                }));
            });

        }

        public static T Deserialize<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                //var serializer = new DataContractJsonSerializer(obj.GetType());
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(ms);
                return obj;
            }
        }
    }
}