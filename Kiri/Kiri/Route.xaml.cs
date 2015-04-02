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
using System.IO.IsolatedStorage;

namespace Kiri
{
    public partial class Route : PhoneApplicationPage
    {
        //get location
        //zhttps://msdn.microsoft.com/en-us/library/windows/apps/jj247548%28v=vs.105%29.aspx
        private string startCoordinate;
        private string finishCoordinate;
        private string queryFrom;
        private string queryTo;
        private Boolean listBoxStatus; //true == show, false == hide
        private LocationFinder lFinder;
        private Point[] arrayFocus;
        private String[] detailRoute;
        private int focusPointNumber;
        private MapLayer routeLayer;
        private MapLayer myLocationLayer;

        Geolocator geolocator = null;
        bool tracking = true;

        private GeoCoordinateWatcher watcher;
        private BackgroundWorker backgroundWorker;

        public Route()
        {

            InitializeComponent();
            this.lFinder = new LocationFinder();
            this.arrayFocus = null;
            this.detailRoute = null;
            this.focusPointNumber = 0;
            this.routeLayer = new MapLayer();
            this.myLocationLayer = new MapLayer();
            ShowLoading();
            this.TrackLocation_Click();
            //this.StartLoadingData();
            //startLocation();
        }

        //zhttp://www.geekchamp.com/articles/understanding-the-windows-phone-location-service-how-to-get-current-gps-coordinates
        //zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff431782(v=vs.105).aspx
        /*
        private void startLocation()
        {
          // The watcher variable was previously declared as type GeoCoordinateWatcher. 
          if (watcher == null)
          {
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default); // using high accuracy
            watcher.MovementThreshold = 20; // use MovementThreshold to ignore noise in the signal
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
          }
          watcher.Start();
        } // End of the Start button Click handler.

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            MessageBox.Show(e.Position.Location.Latitude+","+e.Position.Location.Longitude);
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
         */
 
        //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff626521%28v=vs.105%29.asp
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string start = ""; 
            string finish = "";
            string nameFrom = "";
            string nameTo = "";
            if (NavigationContext.QueryString.TryGetValue("start", out start));
            this.startCoordinate = start;
            if (NavigationContext.QueryString.TryGetValue("finish", out finish));
            this.finishCoordinate = finish;
            if (NavigationContext.QueryString.TryGetValue("nameFrom", out nameFrom)) ;
            this.queryFrom = nameFrom;
            if (NavigationContext.QueryString.TryGetValue("nameTo", out nameTo)) ;
            this.queryTo = nameTo;

            //detect location
            
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
                {
                    // User has opted in or out of Location
                    return;
                }
                else
                {
                    MessageBoxResult result = 
                        MessageBox.Show("This app accesses your phone's location. Is that ok?", 
                        "Location",
                        MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                    }else
                    {
                        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                    }

                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
             

            //Find(); // After get start coordinate and finish coordinate then call Find Method
        }

        private void TrackLocation_Click()
        {
            /*
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }
             */
            geolocator = lFinder.geolocator;

            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
        }

        void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string status = "";
            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // the application does not have the right capability or the location master switch is off
                    status = "location is disabled in phone settings";
                    break;
                case PositionStatus.Initializing:
                    // the geolocator started the tracking operation
                    status = "initializing";
                    break;
                case PositionStatus.NoData:
                    // the location service was not able to acquire the location
                    status = "no data";
                    break;
                case PositionStatus.Ready:
                    // the location service is generating geopositions as specified by the tracking parameters
                    status = "ready";
                    break;
                case PositionStatus.NotAvailable:
                    status = "not available";
                    // not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state
                    break;
            }
            System.Console.WriteLine(status);
        }

        void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                //MessageBox.Show(args.Position.Coordinate.Latitude.ToString() + ", " + args.Position.Coordinate.Longitude.ToString());
                drawMyLocationOnTheMap(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                lFinder.setPositionChanged(sender,args);
            });
        }

        public async void Find()
        {
            //this.TrackLocation_Click();
            Boolean status = true; 
            HttpClient httpClient = new HttpClient();
            Protocol p = new Protocol();
            String uri = p.getFindRoute(startCoordinate, finishCoordinate);

            Task<string> requestRouteTask = httpClient.GetStringAsync(new Uri(uri));
            string requestRoute = await requestRouteTask;

            RootObjectFindRoute r = JsonConvert.DeserializeObject<RootObjectFindRoute>(requestRoute); //Mengubah String menjadi objek
            if (r.status.Equals("ok"))
            {
                //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn792121.aspx 
                
                MapPolyline routeRoad = new MapPolyline();
                routeRoad.StrokeThickness = 3;
                if (!r.routingresults[0].steps[0][3].Equals("Maaf, kami tidak dapat menemukan rute transportasi publik untuk perjalanan Anda.")) //Check ditemukan atau tidak
                {
                    for (int i = 0; i < 1; i++) // Ambil Routing result yg pertama
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
                                    overlay1.Content = createNew(p.iconStart, geoCoo, "start");
                                    this.route.Center = geoCoo;
                                    this.route.ZoomLevel = 14;
                                }
                                else if (j == r.routingresults[i].steps.Count - 1 && c == coordinate.Length-2)
                                {
                                    overlay1.Content = createNew(p.iconFinish, geoCoo, "finish"); 
                                }else if(c==0){
                                    String iconLoc = p.getTypeTransport(r.routingresults[i].steps[j][0].ToString(), r.routingresults[i].steps[j][1].ToString());
                                    overlay1.Content = createNew(iconLoc, geoCoo, "transport");
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
                            //Add image 
                            Uri imgUri = new Uri(p.getTypeTransport(r.routingresults[i].steps[j][0].ToString(), r.routingresults[i].steps[j][1].ToString()), UriKind.RelativeOrAbsolute);
                            BitmapImage imgSourceR = new BitmapImage(imgUri);
                            Image image = new Image();
                            image.Source = imgSourceR;
                            image.Height = 30;
                            image.Width = 50;
                            //ImageBrush imgBrush = new ImageBrush() { ImageSource = imgSourceR };
                            //add description
                            listRoute.Items.Add(image);
                            listRoute.Items.Add(r.routingresults[i].steps[j][3].ToString() + System.Environment.NewLine); // Add text to listBox
                            routeRoad.Path.Add(geoCoo);
                        }
                        //point.Add();
                        //..Items.Add(r.routingresults[c].placename);
                        addFindRoute.Text = queryFrom + " ke " + queryTo + " ( " + r.routingresults[i] .traveltime+ " )";
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
            //    }));
            //});
        }

        public void setFocus(object sender, RoutedEventArgs e)
        {
            routePanel.Visibility = Visibility.Collapsed;
            detailPanel.Visibility = Visibility.Visible;
            this.route.ZoomLevel = 18;
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
            this.route.Center = new GeoCoordinate(lFinder.coorLat, lFinder.coorLong);
            this.route.ZoomLevel = 13;
        }

        private void ShowList(object sender, RoutedEventArgs e)
        {
            detailPanel.Visibility = Visibility.Collapsed;
            if (listBoxStatus == false)
            {
                routePanel.Visibility = Visibility.Visible;
                buttonList.Content = "Tutup List";
                //LayoutRoot.Children.Add(listRoute);
                this.listBoxStatus = true;
            }
            else 
            {
                routePanel.Visibility = Visibility.Collapsed;
                buttonList.Content = "Lihat List";
                //LayoutRoot.Children.Remove(listRoute);
                this.listBoxStatus = false;
            }
        }

        public Pushpin createNew(string uri,GeoCoordinate geoCoordinate,String transport) {
            Pushpin p = new Pushpin();
            Uri imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            BitmapImage imgSourceR = new BitmapImage(imgUri);
            ImageBrush imgBrush = new ImageBrush() { ImageSource = imgSourceR };
            if (transport.Equals("start"))
            {
                p.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                p.Content = new Rectangle()
                {
                    Fill = imgBrush,
                    Height = 30,
                    Width = 50,
                };
                p.Margin = new Thickness(-50, -37, 0, 0);
            }
            else if(transport.Equals("finish")) {
                p.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                p.Content = new Rectangle()
                {
                    Fill = imgBrush,
                    Height = 30,
                    Width = 50,
                };
                p.Margin = new Thickness(-10, -37, 0, 0);
            }else{
                p.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                p.Content = new Rectangle()
                {
                    Fill = imgBrush,
                    Height = 20,
                    Width = 30,
                };
                p.Margin = new Thickness(-8, -60, 0, 0);
            }
            p.GeoCoordinate = geoCoordinate;
            return p;
        }

        //zhttps://msdn.microsoft.com/en-us/library/windows/apps/jj206956%28v=vs.105%29.aspx
        private async void ShowMyLocationOnTheMap()
        {
            this.route.Layers.Remove(myLocationLayer);
            // Get my current location.
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
        }

        private void drawMyLocationOnTheMap(Double latitude, Double longitude)
        {
            this.route.Layers.Remove(myLocationLayer);
            GeoCoordinate myGeoCoordinate = new GeoCoordinate(latitude, longitude);
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
            this.route.Layers.Add(myLocationLayer);
        }

        private void ShowLoading()
        {
            this.popup.IsOpen = true;
            StartLoadingData();
        }

        private void StartLoadingData()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                Find();
            });
            
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.popup.IsOpen = false;
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