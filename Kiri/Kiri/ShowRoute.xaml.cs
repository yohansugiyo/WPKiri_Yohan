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
using Newtonsoft.Json;

namespace Kiri
{
    public partial class ShowRoute : PhoneApplicationPage
    {
        private string startCoordinate;
        private string finishCoordinate;

        public ShowRoute()
        {
            //this.startCoordinate = startCoordinate;
            //this.finishCoordinate = finishCoordinate;
            /*
            string start = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("start", out start))
            {
                this.startCoordinate = start;
            }
            */
            InitializeComponent();
        }

        //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff626521%28v=vs.105%29.asp
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string start = ""; 
            string finish = "";
            if (NavigationContext.QueryString.TryGetValue("start", out start));
            this.startCoordinate = start;
            if (NavigationContext.QueryString.TryGetValue("finish", out finish));
            this.finishCoordinate = finish;

            Find(); // After get start coordinate and finish coordinate then call Find Method
        }
        
        public void Find(){
            HttpClient httpClient = new HttpClient();
            Protocol p = new Protocol();
            String uri = p.getFindRoute(startCoordinate, finishCoordinate);
            
            Task<string> requestRoute = httpClient.GetStringAsync(new Uri(uri));
            requestRoute.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    MessageBox.Show(uri);
                    RootObjectFindRoute r = JsonConvert.DeserializeObject<RootObjectFindRoute>(requestRoute.Result); //Mengubah String menjadi objek
                    if (r.status.Equals("ok"))
                    {
                        //Pushpin pic = new Pushpin();
                        MapPolyline routeRoad = new MapPolyline();
                        routeRoad.StrokeThickness = 2;

                        for (int i = 0; i < r.routingresults.Count; i++)
                        {
                            for (int j = 0; j < r.routingresults[i].steps.Count; j++)
                            {
                                GeoCoordinate geoCoo = new GeoCoordinate();
                                String temp = r.routingresults[i].steps[j][2].ToString();
                                temp = temp.Replace("[", "");
                                temp = temp.Replace("]", "");
                                temp = temp.Replace("\"", "");
                                temp = temp.Replace(" ", "");
                                string[] coordinate = temp.Split(',');
                                for (int c = 0; c < coordinate.Length; c = c + 2)
                                {
                                    geoCoo = new GeoCoordinate();
                                    geoCoo.Latitude = double.Parse(coordinate[c]);
                                    geoCoo.Longitude = double.Parse(coordinate[c + 1]);
                                    routeRoad.Path.Add(geoCoo);
                                }
                                routeRoad.Path.Add(geoCoo);
                            }
                            //point.Add();
                            //..Items.Add(r.routingresults[c].placename);
                        }

                        // Add the list box to a parent container in the visual tree.
                        MyMapFrom.MapElements.Add(routeRoad);
                    }
                    else {
                        MessageBox.Show("Rute tidak dapat ditemukan");
                    }

                }));
            });

        }

        //JSON data to c# using JSON.NET
        //package from zhttp://www.nuget.org/packages/newtonsoft.json
        // dok zhttp://www.newtonsoft.com/json/help/html/SerializingJSON.htm
        public static T Deserialize<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
        }
    }
}