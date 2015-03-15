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
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Threading;

namespace Kiri
{
    public partial class ShowRoute : PhoneApplicationPage
    {
        
        private string startCoordinate;
        private string finishCoordinate;
        private ListBox listRoute;
        private Boolean listBoxStatus; //true == show, false == hide
        private LocationFinder lFinder;

        public ShowRoute()
        {
            this.listRoute = new ListBox();
            this.listRoute.Width = 600;
            this.listRoute.Height = 300;
            this.listRoute.Background = new SolidColorBrush(Colors.Black);
            this.listRoute.FontSize = 30;
            this.listBoxStatus = false;

            this.lFinder = new LocationFinder();
            lFinder.OneShotLocation_Click();
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

        public async void Find()
        {
            Boolean status = true; 
            HttpClient httpClient = new HttpClient();
            Protocol p = new Protocol();
            String uri = p.getFindRoute(startCoordinate, finishCoordinate);

            this.route.Center = new GeoCoordinate(lFinder.coorLat, lFinder.coorLong);
            this.route.ZoomLevel = 13;

            //MessageBox.Show(uri);
            Task<string> requestRouteTask = httpClient.GetStringAsync(new Uri(uri));
            loadingRoute.IsIndeterminate = true;
            string requestRoute = await requestRouteTask;
            loadingRoute.IsIndeterminate = false;
            //requestRoute.ContinueWith(delegate
            //{
            //    Dispatcher.BeginInvoke(new Action(delegate
            //    {
            RootObjectFindRoute r = JsonConvert.DeserializeObject<RootObjectFindRoute>(requestRoute); //Mengubah String menjadi objek
            if (r.status.Equals("ok"))
            {
                //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn792121.aspx 
                MapLayer layer = new MapLayer();
                MapPolyline routeRoad = new MapPolyline();
                routeRoad.StrokeThickness = 3;

                if (!r.routingresults[0].steps[0][3].Equals("Maaf, kami tidak dapat menemukan rute transportasi publik untuk perjalanan Anda.")) //Check ditemukan atau tidak
                {
                    for (int i = 0; i < r.routingresults.Count; i++)
                    {
                        for (int j = 0; j < r.routingresults[i].steps.Count; j++)
                        {
                            if (r.routingresults[i].steps[j][0].ToString().Equals("walk"))
                            {
                                //MessageBox.Show(r.routingresults[i].steps[j][0].ToString()+"  "+"walk");
                                routeRoad.StrokeColor = Color.FromArgb(255, 255, 0, 0);
                            }
                            else {
                                //MessageBox.Show(r.routingresults[i].steps[j][0].ToString() + "  " + "angkot");
                                routeRoad.StrokeColor = Color.FromArgb(255, 0, 255, 255);
                            }
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

                                MapOverlay overlay1 = new MapOverlay();
                                if(j==0  && c==0){
                                    overlay1.Content = createNew(p.iconStart, geoCoo,false);
                                    this.route.Center = geoCoo;
                                    this.route.ZoomLevel = 13;
                                }
                                else if (j == r.routingresults[i].steps.Count - 1 && c == coordinate.Length-2)
                                {
                                    overlay1.Content = createNew(p.iconFinish, geoCoo,false); 
                                }else if(c==0){
                                    String iconLoc = p.getTypeTransport(r.routingresults[i].steps[j][0].ToString(), r.routingresults[i].steps[j][1].ToString());
                                    overlay1.Content = createNew(iconLoc, geoCoo,true);
                                }
                                overlay1.GeoCoordinate = geoCoo;
                                layer.Add(overlay1);
                                        
                            }

                            listRoute.Items.Add(r.routingresults[i].steps[j][3].ToString());
                            routeRoad.Path.Add(geoCoo);
                        }
                        //point.Add();
                        //..Items.Add(r.routingresults[c].placename);
                    }

                    // Add the list box to a parent container in the visual tree.
                    route.MapElements.Add(routeRoad);
                    route.Layers.Add(layer);
                }
                else {
                    MessageBox.Show("Maaf, kami tidak dapat menemukan rute transportasi publik untuk perjalanan Anda.");
                    status = false; 
                }
            }
            else {
                MessageBox.Show("Error");
                status = false; 
            }

            if (status == false)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            //    }));
            //});
        }


        private void ShowList(object sender, RoutedEventArgs e)
        {
            if (listBoxStatus == false)
            {
                LayoutRoot.Children.Add(listRoute);
                this.listBoxStatus = true;
            }
            else 
            {
                LayoutRoot.Children.Remove(listRoute);
                this.listBoxStatus = false;
            }
        }

        public Pushpin createNew(string uri,GeoCoordinate geoCoordinate,Boolean backgroundStatus) {
            Pushpin p = new Pushpin();
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSourceR = new BitmapImage(imgUri);
            ImageBrush imgBrush = new ImageBrush() { ImageSource = imgSourceR };
            if (backgroundStatus == true)
            {
                p.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            else {
                p.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            p.Content = new Rectangle()
            {
                Fill = imgBrush,
                Height = 30,
                Width = 50
            };
            p.GeoCoordinate = geoCoordinate;
            return p;
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