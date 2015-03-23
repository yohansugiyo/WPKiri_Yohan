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
        private Boolean listBoxStatus; //true == show, false == hide
        private LocationFinder lFinder;
        private Point[] arrayFocus;
        private String[] detailRoute;
        private int focusPointNumber;
        private MapLayer routeLayer;
        private MapLayer myLocationLayer;
        GeoCoordinateWatcher watcher;



        public ShowRoute()
        {
            this.lFinder = new LocationFinder();
            this.arrayFocus = null;
            this.detailRoute = null;
            this.focusPointNumber = 0;
            this.routeLayer = new MapLayer();
            this.myLocationLayer = new MapLayer();
            lFinder.OneShotLocation_Click();
            InitializeComponent();
            ShowMyLocationOnTheMap();
            //startLocation();
        }

        //zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff431782(v=vs.105).aspx
        private void startLocation()
        {
          // The watcher variable was previously declared as type GeoCoordinateWatcher. 
          if (watcher == null)
          {
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // using high accuracy
            watcher.MovementThreshold = 20; // use MovementThreshold to ignore noise in the signal
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
          }
          watcher.Start();
        } // End of the Start button Click handler.

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            ShowMyLocationOnTheMap();
            MessageBox.Show("changed");
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // The Location Service is disabled or unsupported.
                    // Check to see whether the user has disabled the Location Service.
                    if (watcher.Permission == GeoPositionPermission.Denied)
                    {
                        // The user has disabled the Location Service on their device.
                        MessageBox.Show("you have this application access to location.");
                    }
                    else
                    {
                        MessageBox.Show("location is not functioning on this device");
                    }
                    break;

                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    //startLocationButton.IsEnabled = false;
                    break;

                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    // Alert the user and enable the Stop Location button.
                    MessageBox.Show("location data is not available.");
                    watcher.Stop();
                    break;

                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    // Show the current position and enable the Stop Location button.
                    MessageBox.Show("location data is available.");
                    ShowMyLocationOnTheMap();
                    watcher.Stop();
                    break;
            }
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

            //MessageBox.Show(uri);
            Task<string> requestRouteTask = httpClient.GetStringAsync(new Uri(uri));
            loadingRoute.IsIndeterminate = true;
            string requestRoute = await requestRouteTask;
            
            //requestRoute.ContinueWith(delegate
            //{
            //    Dispatcher.BeginInvoke(new Action(delegate
            //    {

            RootObjectFindRoute r = JsonConvert.DeserializeObject<RootObjectFindRoute>(requestRoute); //Mengubah String menjadi objek
            if (r.status.Equals("ok"))
            {
                //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn792121.aspx 
                
                MapPolyline routeRoad = new MapPolyline();
                routeRoad.StrokeThickness = 3;
                if (!r.routingresults[0].steps[0][3].Equals("Maaf, kami tidak dapat menemukan rute transportasi publik untuk perjalanan Anda.")) //Check ditemukan atau tidak
                {
                    for (int i = 0; i < r.routingresults.Count; i++)
                    {
                        this.arrayFocus = new Point[r.routingresults[i].steps.Count+1];
                        this.detailRoute = new String[r.routingresults[i].steps.Count + 1];
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
                                //Process add icon/pushpin
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
                                routeLayer.Add(overlay1);   
                            }
                            
                            //reference add image zhttp://stackoverflow.com/questions/676869/add-image-to-listbox
                            this.arrayFocus[j] = new Point(double.Parse(coordinate[0]), double.Parse(coordinate[1]));
                            this.detailRoute[j] = r.routingresults[i].steps[j][3].ToString();
                            if (j == r.routingresults[i].steps.Count-1) // Untuk yg terakhir/sampai di tujuan
                            {
                                this.arrayFocus[j + 1] = new Point(double.Parse(coordinate[coordinate.Length - 2]), double.Parse(coordinate[coordinate.Length - 1]));
                                this.detailRoute[j+1] = "Sampai di tujuan!";
                            }
                            listRoute.Items.Add(r.routingresults[i].steps[j][3].ToString() + System.Environment.NewLine); // Add text to listBox
                            routeRoad.Path.Add(geoCoo);
                        }
                        //point.Add();
                        //..Items.Add(r.routingresults[c].placename);
                    }

                    // Add the list box to a parent container in the visual tree.
                    route.MapElements.Add(routeRoad);
                    route.Layers.Add(routeLayer);
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
            loadingRoute.IsIndeterminate = false;
            //    }));
            //});
        }

        public void setFocus(object sender, RoutedEventArgs e)
        {
            detailPanel.Visibility = Visibility.Visible;
            this.route.ZoomLevel = 16;
            if ((sender as Button).Name.Equals("prev"))
            {   
                this.focusPointNumber--;
                if (this.focusPointNumber < 0)
                {
                    this.focusPointNumber = arrayFocus.Length-1;
                }
                this.route.Center = new GeoCoordinate(arrayFocus[focusPointNumber].X, arrayFocus[focusPointNumber].Y);
            }
            else 
            {
                this.focusPointNumber++;
                if (this.focusPointNumber > arrayFocus.Length-1)
                {
                    this.focusPointNumber = 0;
                }
                this.route.Center = new GeoCoordinate(arrayFocus[focusPointNumber].X, arrayFocus[focusPointNumber].Y);
            }
            detailShows.Text = detailRoute[focusPointNumber];
        }

        public void toMyLocation(object sender, RoutedEventArgs e)
        {
            ShowMyLocationOnTheMap();
        }

        private void ShowList(object sender, RoutedEventArgs e)
        {
            if (listBoxStatus == false)
            {
                routePanel.Visibility = Visibility.Visible;
                //LayoutRoot.Children.Add(listRoute);
                this.listBoxStatus = true;
            }
            else 
            {
                routePanel.Visibility = Visibility.Collapsed;
                //LayoutRoot.Children.Remove(listRoute);
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
                Width = 50,
            };
            p.GeoCoordinate = geoCoordinate;
            p.Margin = new Thickness(-20, -30, 0, 0);
            return p;
        }

        //zhttps://msdn.microsoft.com/en-us/library/windows/apps/jj206956%28v=vs.105%29.aspx
        private async void ShowMyLocationOnTheMap()
        {
            this.route.Layers.Remove(myLocationLayer);
            // Get my current location.
            loadingRoute.IsIndeterminate = true;
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);

            double myAccuracy = 50.0;//Double.Parse(myGeolocator.DesiredAccuracyInMeters+"");
            double metersPerPixels = (Math.Cos(myGeoCoordinate.Latitude * Math.PI / 180) * 2 * Math.PI * 6378137) / (256 * Math.Pow(2, this.route.ZoomLevel));
            double radius = myAccuracy / metersPerPixels;

            Ellipse ellipse = new Ellipse();
            ellipse.Width = radius * 2;
            ellipse.Height = radius * 2;
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(75, 200, 0, 0));

            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Black);
            myCircle.Height = 10;
            myCircle.Width = 10;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlayPosition = new MapOverlay();
            myLocationOverlayPosition.Content = myCircle;
            myLocationOverlayPosition.PositionOrigin = new Point(0, 0);
            myLocationOverlayPosition.GeoCoordinate = myGeoCoordinate;

            MapOverlay myLocationOverlayAccuracy = new MapOverlay();
            myLocationOverlayAccuracy.Content = ellipse;
            myLocationOverlayAccuracy.PositionOrigin = new Point(0, 0);
            myLocationOverlayAccuracy.GeoCoordinate = myGeoCoordinate;

            myLocationLayer.Add(myLocationOverlayPosition);
            myLocationLayer.Add(myLocationOverlayAccuracy);

            this.route.Center = myGeoCoordinate;
            this.route.ZoomLevel = 13;
            this.route.Layers.Add(myLocationLayer);
            loadingRoute.IsIndeterminate = false;
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