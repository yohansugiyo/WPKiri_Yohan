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

namespace Kiri
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private LocationFinder lFinder;
        HttpClient httpClient = new HttpClient();

        ListBox listPlaceFrom, listPlaceTo;
        String locFrom;
        String locTo;
        
        public MainPage()
        {
            InitializeComponent();

            lFinder = new LocationFinder();
            listPlaceFrom = new ListBox();
            listPlaceTo = new ListBox();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void startRoute(object sender, RoutedEventArgs e)
        {
            String queryFrom = fromBox.Text;
            String queryTo = toBox.Text;
            
            //Penting!
            //Dokumentasi zhttps://bitbucket.org/projectkiri/kiri_api/wiki/KIRI%20API%20v2%20Documentation
            //Contoh searchplace    zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=bip&apikey=97A7A1157A05ED6F
            //                      zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=pvj&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.90864,107.61108&finish=-6.88929,107.59574&presentation=mobile&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.87474,107.60491&finish=-6.88909,107.59614&presentation=mobile&apikey=97A7A1157A05ED6F    

            String phpHandle = "http://kiri.travel/handle.php?";
            String version = "2";
            String mode = "searchplace";
            String region = "bdo";

            String queryString = queryFrom;

            String apikey = "97A7A1157A05ED6F";
           
            String uri = phpHandle + "version=" + version + "&mode=" + mode + "&region=" + region + "&querystring=" + queryString + "&apikey=" + apikey;
            
            Task<string> requestFrom = httpClient.GetStringAsync(new Uri(uri));

            queryString = queryTo;
            uri = phpHandle + "version=" + version + "&mode=" + mode + "&region=" + region + "&querystring=" + queryString + "&apikey=" + apikey;
            Task<string> requestTo = httpClient.GetStringAsync(new Uri(uri));
            // String txt = request.Result;
            //bool txt = request.IsCompleted;

            String fromText = "BIP";
            String toText = "";
            requestFrom.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate{
                    //keluaranFrom.Text = requestFrom.Result;
                    RootObjectSearchPlace r = new RootObjectSearchPlace(); //Untuk Asal
                    r = Deserialize<RootObjectSearchPlace>(requestFrom.Result); //Mengubah String menjadi objek
                    //keluaranFrom.Text = ""+r.searchresult.Count;

                    // Create a new list box, add items, and add a SelectionChanged handler.
                    if (listPlaceFrom != null)
                    {
                    for (int c = 0; c < r.searchresult.Count; c++) {
                        listPlaceFrom.Items.Add(r.searchresult[c].placename);
                    }
                    listPlaceFrom.Width = 720;
                    listPlaceFrom.Height = 1280;
                    listPlaceFrom.Background = new SolidColorBrush(Colors.Black);
                    listPlaceFrom.FontSize = 30;
                    listPlaceFrom.SelectionChanged += ListBox_SelectedPlaceFrom;

                    // Add the list box to a parent container in the visual tree.
                    LayoutRoot.Children.Add(listPlaceFrom);
                    }
                }));
            });
            requestTo.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    //keluaranTo.Text = requestTo.Result;
                    RootObjectSearchPlace r = new RootObjectSearchPlace(); //Untuk Tujuan
                    r = Deserialize<RootObjectSearchPlace>(requestTo.Result);

                    if (listPlaceTo != null)
                    {
                        for (int c = 0; c < r.searchresult.Count; c++)
                        {
                            listPlaceTo.Items.Add(r.searchresult[c].placename);
                        }
                        listPlaceTo.Width = 720;
                        listPlaceTo.Height = 1280;
                        listPlaceTo.Background = new SolidColorBrush(Colors.Black);
                        listPlaceTo.FontSize = 30;
                        listPlaceTo.SelectionChanged += ListBox_SelectedPlaceTo;

                        // Add the list box to a parent container in the visual tree.
                        LayoutRoot.Children.Add(listPlaceTo);
                    }
                }));
            });
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Add code to perform some action here.
        }

        private void changeMapFrom(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/mapFrom.xaml", UriKind.Relative));    
        }
        private void changeMapTo(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/mapTo.xaml", UriKind.Relative));    
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lFinder.OneShotLocation_Click();
            fromBox.Text = lFinder.coorLat + " " + lFinder.coorLong;
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

        private void ListBox_SelectedPlaceFrom(object sender, SelectionChangedEventArgs e)
        {
            if (null != listPlaceFrom.SelectedItem)
            {
                //locFrom = (listPlace.SelectedItem as ListBoxItem).Content.ToString();
                //keluaranFrom.Text = locFrom;
                //NavigationService.Navigate(new Uri("/ShowRoute.xaml", UriKind.Relative));
                /*
                foreach (Object obj in listPlace.SelectedItems)
                {
                    MessageBox.Show(obj.ToString());
                }
                 */
            }
            LayoutRoot.Children.Remove(listPlaceFrom);
        }

        private void ListBox_SelectedPlaceTo(object sender, SelectionChangedEventArgs e)
        {
            if (null != listPlaceTo.SelectedItem)
            {
                //locFrom = (listPlace.SelectedItem as ListBoxItem).Content.ToString();
                //keluaranFrom.Text = locFrom;
                //NavigationService.Navigate(new Uri("/ShowRoute.xaml", UriKind.Relative));
                /*
                foreach (Object obj in listPlace.SelectedItems)
                {
                    MessageBox.Show(obj.ToString());
                }
                 */
            }
            LayoutRoot.Children.Remove(listPlaceTo);
            NavigationService.Navigate(new Uri("/ShowRoute.xaml", UriKind.Relative));
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