using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Kiri.Resources;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;

using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Threading;

namespace Kiri
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private Protocol protocol;
        private LocationFinder lFinder;
        HttpClient httpClient = new HttpClient();

        ListBox listPlaceFrom, listPlaceTo;
        String locationFrom, locationTo;
        Point centerOfBandung, centerOfJakarta, centerOfMalang, centerOfSurabaya;

        //String[,] city = { { "Auto", "Bandung", "Jakarta", "Malang", "Surabaya" }, { "Auto", "bdo", "cgk", "mlg", "sub" } };
        String[] city;
        String myCity;

        private BackgroundWorker backgroundWorker;
        
        public MainPage()
        {
            InitializeComponent();
            ShowSplash();
            this.city = new String[] { "Auto", "Bandung", "Jakarta", "Malang", "Surabaya" };
            this.cmbCurrFrom.ItemsSource = city;
            this.myCity = "bdo";
            this.protocol = new Protocol();
            this.lFinder = new LocationFinder();
            this.listPlaceFrom = new ListBox();
            this.listPlaceTo = new ListBox();
            this.locationFrom = null;
            this.locationTo = null;
            this.centerOfBandung = new Point(0.0 , 0.0);
            this.centerOfJakarta = new Point(0.0, 0.0);
            this.centerOfMalang = new Point(0.0, 0.0);
            this.centerOfSurabaya = new Point(0.0, 0.0);
        }

        private async void startRoute(object sender, RoutedEventArgs e)
        {
            String queryFrom = fromBox.Text;
            String queryTo = toBox.Text;
            
            //Penting!
            //Dokumentasi zhttps://bitbucket.org/projectkiri/kiri_api/wiki/KIRI%20API%20v2%20Documentation
            //Contoh searchplace    zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=bip&apikey=97A7A1157A05ED6F
            //                      zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=pvj&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.90864,107.61108&finish=-6.88929,107.59574&presentation=mobile&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.87474,107.60491&finish=-6.88909,107.59614&presentation=mobile&apikey=97A7A1157A05ED6F    

            //Task.Run(() => ComputeNextMove(queryFrom, queryTo));

            //Reference zhttps://msdn.microsoft.com/en-us/library/hh191443.aspx
            Task<string> requestFromTask = httpClient.GetStringAsync(new Uri(protocol.getSearchPlace(queryFrom)));
            Task<string> requestToTask = httpClient.GetStringAsync(new Uri(protocol.getSearchPlace(queryTo)));

            progressFindPlace.IsIndeterminate = true;
            string requestFrom = await requestFromTask;
            string requestTo = await requestToTask;
            progressFindPlace.IsIndeterminate = false;

            //Task<String>[] arrGetString = new Task<string>[]{ requestFrom, requestTo };

            //Task.WhenAll(arrGetString);

            //requestTo.ContinueWith(delegate
            //{
                //Dispatcher.BeginInvoke(new Action(delegate
                //{
                    //keluaranTo.Text = requestTo.Result;
            RootObjectSearchPlace r1 = new RootObjectSearchPlace(); //Untuk Asal
            r1 = Deserialize<RootObjectSearchPlace>(requestFrom); //Mengubah String menjadi objek
            //getListItem(r1);
            //LayoutRoot.Children.Add(listPlaceFrom);
                    
            RootObjectSearchPlace r2 = new RootObjectSearchPlace(); //Untuk Tujuan
            r2 = Deserialize<RootObjectSearchPlace>(requestTo);
            getListItem(r1,r2);
                    //LayoutRoot.Children.Add(listPlaceTo);
                    
                    //MessageBox.Show("Error2");
                //}));
            //});
            
            /*
            ListBox listBox1 = new ListBox();
            listBox1.Items.Add(fromText);
            listBox1.Width = 400;
            listBox1.Background = new SolidColorBrush(Colors.Gray);
            listBox1.SelectionChanged += ListBox_SelectionChanged;

            // Add the list box to a parent container in the visual tree.
            ContentPanel.Children.Add(listBox1);
             */ 
        }

        public RootObjectSearchPlace toObjectSearchPlace(string uri) {
            RootObjectSearchPlace sp = null;
            Task<string> request = httpClient.GetStringAsync(new Uri(uri));
            request.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    sp = new RootObjectSearchPlace();
                    sp = JsonConvert.DeserializeObject<RootObjectSearchPlace>(request.Result);
                }));
            });
            return sp;
        }

        private void changeMapFrom(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/mapFrom.xaml", UriKind.Relative));    
        }
        private void changeMapTo(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/mapTo.xaml", UriKind.Relative));    
        }

        private void getHereFrom(object sender, RoutedEventArgs e)
        {
            lFinder.OneShotLocation_Click();
            //MessageBox.Show(lFinder.coorLat + " " + lFinder.coorLong);
            fromBox.Text = lFinder.coorLat + " " + lFinder.coorLong;
        }

        private void getHereTo(object sender, RoutedEventArgs e)
        {
            lFinder.OneShotLocation_Click();
            //MessageBox.Show(lFinder.coorLat + " " + lFinder.coorLong);
            toBox.Text = lFinder.coorLat + " " + lFinder.coorLong;
        }

        /*
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
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
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
         */

        /*reference: zhttps://msdn.microsoft.com/en-us/library/bb412179%28v=vs.110%29.aspx*/
        public static T Deserialize<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                return obj;
            }
        }

        public static string Serialize<T>(T obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray(),0,(int)ms.Length);
            }
        }

        //zhttps://social.msdn.microsoft.com/forums/windowsapps/en-us/7db73c64-86f7-4c43-9fd4-faa03421ea21/popup-blocking-listbox-selection-changed
        private void getListItem(RootObjectSearchPlace requestFrom, RootObjectSearchPlace requestTo)
        {
            //ListBox listFrom = new ListBox();
            //ListBox listTo = new ListBox();
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (requestFrom.status.ToString().Equals("ok") && requestTo.status.ToString().Equals("ok")) //check status
                {
                    if (requestFrom.searchresult.Count() == 0)
                    { //check From search
                        MessageBox.Show("Pencarian untuk kata " + fromBox.Text + " tidak ditemuakan");
                    }
                    else if (requestFrom.searchresult.Count() == 1)
                    {
                        this.locationFrom = requestFrom.searchresult[0].location.ToString();
                    }
                    else
                    {
                        TextBlock textAsal = new TextBlock();
                        textAsal.FontSize = 40;
                        textAsal.Text = "Pilih Tempat Asal";
                        //LayoutRoot.Children.Add(getListItem(requestFrom));
                        for (int c = 0; c < requestFrom.searchresult.Count; c++)
                        {
                            listPlaceFrom.Items.Add(requestFrom.searchresult[c].placename);
                        }
                        listPlaceFrom.Width = System.Windows.Application.Current.Host.Content.ActualWidth;
                        listPlaceFrom.Height = System.Windows.Application.Current.Host.Content.ActualHeight;
                        listPlaceFrom.Background = new SolidColorBrush(Colors.Black);
                        listPlaceFrom.FontSize = 30;
                        listPlaceFrom.DataContext = requestFrom.searchresult;
                        listPlaceFrom.SelectionChanged += ListBoxSelectedPlaceFrom;
                        //LayoutRoot.Children.Add(listPlaceFrom);
                        panelFrom.Children.Add(textAsal);
                        panelFrom.Children.Add(listPlaceFrom);
                        panelFrom.Visibility = Visibility.Visible;
                    }

                    if (requestTo.searchresult.Count() == 0)
                    { //check From search
                        MessageBox.Show("Pencarian untuk kata " + toBox.Text + " tidak ditemuakan");
                    }
                    else if (requestTo.searchresult.Count() == 1)
                    {
                        this.locationTo = requestTo.searchresult[0].location.ToString();
                    }
                    else
                    {
                        TextBlock textTujuan = new TextBlock();
                        textTujuan.FontSize = 40;
                        textTujuan.Text = "Pilih Tempat Tujuan";
                        //LayoutRoot.Children.Add(getListItem(requestTo));
                        for (int c = 0; c < requestTo.searchresult.Count; c++)
                        {
                            listPlaceTo.Items.Add(requestTo.searchresult[c].placename);
                        }
                        listPlaceTo.Width = System.Windows.Application.Current.Host.Content.ActualWidth;
                        listPlaceTo.Height = System.Windows.Application.Current.Host.Content.ActualHeight;
                        listPlaceTo.Background = new SolidColorBrush(Colors.Black);
                        listPlaceTo.FontSize = 30;
                        listPlaceTo.DataContext = requestTo.searchresult;
                        listPlaceTo.SelectionChanged += ListBoxSelectedPlaceTo;
                        panelTo.Children.Add(textTujuan);
                        panelTo.Children.Add(listPlaceTo);
                        //LayoutRoot.Children.Add(listPlaceTo);
                    }
                }
                else
                {
                    MessageBox.Show("Error!");
                    MessageBox.Show("Error!");
                }
            }));
            
        }

        private void ListBoxSelectedPlaceFrom(object sender, SelectionChangedEventArgs e)
        {
            if (null != listPlaceFrom.SelectedItem)
            {
                if ((sender as ListBox).SelectedIndex >= 0)
                {
                    this.locationFrom = searchCoordinatePlace((List<Searchresult>)listPlaceFrom.DataContext, listPlaceFrom.SelectedItem.ToString());  //((sender as ListBox).SelectedItem as Searchresult).placename;
                }
            }
            panelFrom.Children.Clear();
            panelTo.Visibility = Visibility.Visible;
            if (this.locationFrom != null && this.locationTo != null)
            {
                NavigationService.Navigate(new Uri("/ShowRoute.xaml?start=" + locationFrom + "&finish=" + locationTo + "", UriKind.Relative));
            }
        }

        private void ListBoxSelectedPlaceTo(object sender, SelectionChangedEventArgs e)
        {
            if (null != listPlaceTo.SelectedItem)
            {
                this.locationTo = searchCoordinatePlace((List<Searchresult>)listPlaceTo.DataContext, listPlaceTo.SelectedItem.ToString()); //((sender as ListBox).SelectedItem as Searchresult).placename;
            }
            panelTo.Children.Clear();
            if (this.locationFrom != null && this.locationTo != null)
            {
                NavigationService.Navigate(new Uri("/ShowRoute.xaml?start=" + locationFrom + "&finish=" + locationTo + "", UriKind.Relative));
            }
        }

        public string searchCoordinatePlace(List<Searchresult> listResult, string place) { //Pasti ketemu
            String coordinate = "0.0";
            for (int c = 0; c < listResult.Count; c++)
            {
                if (place.Equals(listResult[c].placename))
                {
                    coordinate = listResult[c].location;
                    c = listResult.Count;
                }
            }
            return coordinate;
        }

        private void changeCity(object sender, SelectionChangedEventArgs e)
        {
            if (!cmbCurrFrom.SelectedItem.Equals(null))
            {
                switch (cmbCurrFrom.SelectedItem.ToString())
                {
                    case "Bandung":
                        this.myCity="bdo";
                        break;
                    case "Jakarta":
                        this.myCity="cgk";
                        break;
                    case "Malang":
                        this.myCity="mlg";
                        break;
                    case "Surabaya":
                        this.myCity="sub";
                        break;     
                    default:
                        this.myCity="bdo"; //Finder Auto
                        break;
                }
            }
        }

        private void ShowSplash()
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

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(3000);
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.popup.IsOpen = false;
            });
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}