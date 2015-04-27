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
using System.Windows.Threading;
using System.Diagnostics;

namespace Kiri
{
    public partial class Route : PhoneApplicationPage
    {
        //get location
        //zhttps://msdn.microsoft.com/en-us/library/windows/apps/jj247548%28v=vs.105%29.aspx
        private Boolean listBoxStatus; //true == show, false == hide
        private LocationFinder lFinder;
        private Point[] arrayFocus;
        private String[] detailRoute;
        private int focusPointNumber;
        private MapLayer routeLayer;
        private MapLayer myLocationLayer;
        private Protocol p;
        private Boolean routeReady;

        private Geolocator geolocator = null;
        private BackgroundWorker backgroundWorker;
        private DispatcherTimer newTimer;
        private Boolean timeOut = false;
        //private Boolean routeReady;
        //private AutoResetEvent _workerCompleted = new AutoResetEvent(false);
        public string AppStatus = String.Empty;
        public string SavedData = String.Empty;

        public Route()
        {
            InitializeComponent();
            this.lFinder = null;
            this.arrayFocus = null;
            this.detailRoute = null;
            this.focusPointNumber = 0;
            this.routeLayer = new MapLayer();
            this.myLocationLayer = new MapLayer();
            this.p = new Protocol();
            this.routeReady = false;
            this.newTimer = new DispatcherTimer();
            newTimer.Interval = new TimeSpan(0, 0, 60);
            newTimer.Tick += OnTimerTick;
            newTimer.Start();

            ShowLoading();
            //PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            //this.StartLoadingData();
            //startLocation();
        }

        //old zhttp://www.geekchamp.com/articles/understanding-the-windows-phone-location-service-how-to-get-current-gps-coordinates
        //old zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff431782(v=vs.105).aspx
 
        //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff626521%28v=vs.105%29.asp
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("route");
            base.OnNavigatedTo(e);
            //if (PhoneApplicationService.Current.State.ContainsKey("route"))
            //{
            //    this.lFinder = (LocationFinder)PhoneApplicationService.Current.State["route"];
            //}else
            if (PhoneApplicationService.Current.State.ContainsKey("location"))
            {
                this.lFinder = (LocationFinder)PhoneApplicationService.Current.State["location"];
            }
            this.TrackLocation();
            //this.route.Center = new GeoCoordinate(lFinder.coorLatFrom, lFinder.coorLongFrom);
            //GeoCoordinate[] recLocation = new GeoCoordinate[]{new GeoCoordinate(lFinder.coorLatFrom, lFinder.coorLongFrom),new GeoCoordinate(lFinder.coorLatTo, lFinder.coorLongTo)};
            //LocationRectangle lr = LocationRectangle.CreateBoundingRectangle(recLocation);
            //this.route.SetView(new GeoCoordinate(lFinder.coorLatFrom, lFinder.coorLongFrom), 1, MapAnimationKind.Linear);
            
            //detect location
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            
            //Find(); // After get start coordinate and finish coordinate then call Find Method
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.lFinder.reset();
            PhoneApplicationService.Current.State["location"] = lFinder;
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            PhoneApplicationService.Current.State["location"] = lFinder;
        }

        private void TrackLocation()
        {
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
            
            //p.getRequestRoute(lFinder.coorLatFrom, lFinder.coorLongFrom, lFinder.coorLatTo, lFinder.coorLongTo);
            //String uri = p.getFindRoute(lFinder.coorLatFrom + "," + lFinder.coorLongFrom, lFinder.coorLatTo + "," + lFinder.coorLongTo);
            //Task<String> requestRouteTask = httpClient.GetStringAsync(new Uri(uri));
            //String requestRoute = await requestRouteTask;
            //RootObjectFindRoute r = JsonConvert.DeserializeObject<RootObjectFindRoute>(requestRoute); //Mengubah String menjadi objek

            RootObjectFindRoute r = await p.getRequestRoute(lFinder.coorLatFrom, lFinder.coorLongFrom, lFinder.coorLatTo, lFinder.coorLongTo);
            if (r.status.Equals("ok"))
            {
                //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn792121.aspx 
                if (!r.routingresults[0].steps[0][3].Equals("Maaf, kami tidak dapat menemukan rute transportasi publik untuk perjalanan Anda.")) //Check ditemukan atau tidak
                {
                    this.setRouteToMap(r);
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
                this.lFinder.reset();
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void setRouteToMap(RootObjectFindRoute r) {
            MapPolyline routeRoad = new MapPolyline();
            for (int i = 0; i < 1; i++) // Ambil Routing result yg pertama
            {
                this.arrayFocus = new Point[r.routingresults[i].steps.Count + 1];
                this.detailRoute = new String[r.routingresults[i].steps.Count + 1];
                for (int j = 0; j < r.routingresults[i].steps.Count; j++)
                {
                    routeRoad = new MapPolyline();
                    if (r.routingresults[i].steps[j][0].ToString().Equals("walk"))
                    {
                        //MessageBox.Show(r.routingresults[i].steps[j][0].ToString()+"  "+"walk");
                        routeRoad.StrokeColor = Color.FromArgb(255, 255, 0, 0);
                    }
                    else
                    {
                        //MessageBox.Show(r.routingresults[i].steps[j][0].ToString() + "  " + "angkot");
                        routeRoad.StrokeColor = Color.FromArgb(255, 0, 0, 255);
                    }
                    routeRoad.StrokeThickness = 4;
                    GeoCoordinate geoCoo = new GeoCoordinate();
                    //Trim character
                    String temp = r.routingresults[i].steps[j][2].ToString();
                    char[] charsToTrim = { '[', ']', ' ' };
                    String result = temp.Trim(charsToTrim);
                    result = result.Replace("\"", "");
                    //Split string coordinate to array
                    string[] coordinate = result.Split(',');
                    for (int c = 0; c < coordinate.Length; c = c + 2)
                    {
                        geoCoo = new GeoCoordinate();
                        geoCoo.Latitude = double.Parse(coordinate[c]);
                        geoCoo.Longitude = double.Parse(coordinate[c + 1]);
                        routeRoad.Path.Add(geoCoo);
                        MapOverlay overlay1 = new MapOverlay();
                        //Process add icon/pushpin
                        if (j == 0 && c == 0)
                        {
                            overlay1.Content = createNew(p.iconStart, geoCoo, "start");
                            this.route.Center = geoCoo;
                            this.route.ZoomLevel = 14;
                        }
                        else if (j == r.routingresults[i].steps.Count - 1 && c == coordinate.Length - 2)
                        {
                            //MessageBox.Show("finish");
                            overlay1.Content = createNew(p.iconFinish, geoCoo, "finish");
                        }
                        else if (c == 0)
                        {
                            String iconLoc = p.getTypeTransport(r.routingresults[i].steps[j][0].ToString(), r.routingresults[i].steps[j][1].ToString());
                            if (r.routingresults[i].steps[j][0].ToString().Equals("walk"))
                            {
                                overlay1.Content = createNew(iconLoc, geoCoo, "walk");
                            }
                            else
                            {
                                overlay1.Content = createNew(iconLoc, geoCoo, "umum");
                            }
                        }
                        overlay1.GeoCoordinate = geoCoo;
                        routeLayer.Add(overlay1);
                    }

                    //reference add image zhttp://stackoverflow.com/questions/676869/add-image-to-listbox
                    this.arrayFocus[j] = new Point(double.Parse(coordinate[0]), double.Parse(coordinate[1]));
                    this.detailRoute[j] = r.routingresults[i].steps[j][3].ToString();
                    if (j == r.routingresults[i].steps.Count - 1) // Untuk yg terakhir/sampai di tujuan
                    {
                        this.arrayFocus[j + 1] = new Point(double.Parse(coordinate[coordinate.Length - 2]), double.Parse(coordinate[coordinate.Length - 1]));
                        this.detailRoute[j + 1] = "Sampai di tujuan!";
                    }
                    //Add image 
                    Uri imgUri = new Uri(p.getTypeTransportWOBaloon(r.routingresults[i].steps[j][0].ToString(), r.routingresults[i].steps[j][1].ToString()), UriKind.RelativeOrAbsolute);
                    BitmapImage imgSourceR = new BitmapImage(imgUri);
                    Image image = new Image();
                    image.Source = imgSourceR;
                    image.Height = 30;
                    image.Width = 50;
                    //add description
                    listRoute.Items.Add(image);
                    listRoute.Items.Add(r.routingresults[i].steps[j][3].ToString()); // Add text to listBox
                    routeRoad.Path.Add(geoCoo);
                    route.MapElements.Add(routeRoad);
                }
                addFindRoute.Text = lFinder.addressFrom + " ke " + lFinder.addressTo + " ( " + r.routingresults[i].traveltime + " )";
            }
            // Add the list box to a parent container in the visual tree.
            route.Layers.Add(routeLayer);
            this.routeReady = true;
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
            this.route.ZoomLevel = 15;
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
            p.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            p.Content = new Rectangle()
            {
                Fill = imgBrush,
                Height = 30,
                Width = 50,
            };
            if (transport.Equals("start"))
            {
                p.Margin = new Thickness(-51, -35, 0, 0);
            }
            else if(transport.Equals("finish")) {
                p.Margin = new Thickness(-6, -34, 0, 0);
            }else if(transport.Equals("walk")){
                p.Margin = new Thickness(-55, -35, 0, 0);
            }else{
                p.Margin = new Thickness(-5, -35, 0, 0);
            }
            p.GeoCoordinate = geoCoordinate;
            return p;
        }

        private void drawMyLocationOnTheMap(Double latitude, Double longitude)
        {
            this.route.Layers.Remove(myLocationLayer);
            GeoCoordinate myGeoCoordinate = new GeoCoordinate(latitude, longitude);
            double myAccuracy = lFinder.accuracy; 
            //MessageBox.Show(lFinder.accuracy + "");
            double metersPerPixels = (Math.Cos(myGeoCoordinate.Latitude * Math.PI / 180) * 2 * Math.PI * 6378137) / (256 * Math.Pow(2, this.route.ZoomLevel));
            double radius = myAccuracy / metersPerPixels;

            Ellipse ellipse = new Ellipse();
            ellipse.Width = radius * 2;
            ellipse.Height = radius * 2;
            ellipse.Margin = new Thickness(-radius+10, -radius+10,0, 0);
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(75, 200, 0, 0));

            Ellipse myCircle = new Ellipse();
            myCircle.Stroke = new SolidColorBrush(Colors.Black);
            myCircle.StrokeThickness = 4;
            myCircle.Fill = new SolidColorBrush(Colors.Green);
            myCircle.Height = 25;
            myCircle.Width = 25;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlayPosition = new MapOverlay();
            myLocationOverlayPosition.Content = myCircle;
            myLocationOverlayPosition.PositionOrigin = new Point(0, 0);
            myLocationOverlayPosition.GeoCoordinate = myGeoCoordinate;

            MapOverlay myLocationOverlayAccuracy = new MapOverlay();
            myLocationOverlayAccuracy.Content = ellipse;
            myLocationOverlayAccuracy.PositionOrigin = new Point(0, 0);
            myLocationOverlayAccuracy.GeoCoordinate = myGeoCoordinate;

            myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlayPosition);
            myLocationLayer.Add(myLocationOverlayAccuracy);
            this.route.Layers.Add(myLocationLayer);
        }

        private void back(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Anda yakin mengakhiri Navigasi?","Kembali ke Menu Utama",MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.lFinder.reset();
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void ShowLoading()
        {
            this.popup.IsOpen = true;
            StartLoadingData();
        }

        private void StartLoadingData()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.RunWorkerAsync();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            //_workerCompleted.WaitOne();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.Find();
            });
            
            while (this.routeReady == false)
            {
                if (this.timeOut == true) {
                    //this.timeOutReached();
                    this.routeReady = true;
                }
            }
        }

        void OnTimerTick(Object sender, EventArgs args)
        {
            newTimer.Stop();
            this.timeOut = true;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.popup.IsOpen = false;
                if (this.timeOut == true)
                {
                    MessageBox.Show("Maaf saat ini kami tidak dapet memproses rute anda!");
                    this.lFinder.reset();
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            });
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (MessageBox.Show("Anda yakin mengakhiri Navigasi?", "Kembali ke Menu Utama?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                this.lFinder.reset();
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                e.Cancel = true;
            }
        }

        //JSON data to c# using JSON.NET
        //package from zhttp://www.nuget.org/packages/newtonsoft.json
        // dok zhttp://www.newtonsoft.com/json/help/html/SerializingJSON.htm
    }
}