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

namespace Kiri
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private Protocol p;
        private LocationFinder lFinder;
        HttpClient httpClient = new HttpClient();

        ListBox listPlaceFrom, listPlaceTo;
        String locFrom;
        String locTo;
        
        public MainPage()
        {
            InitializeComponent();

            p = new Protocol();
            lFinder = new LocationFinder();
            listPlaceFrom = new ListBox();
            listPlaceTo = new ListBox();
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

            //RootObjectSearchPlace from = toObjectSearchPlace(p.getSearchPlace(queryFrom)); //Create object searchplace Kiri for From
            //RootObjectSearchPlace to = toObjectSearchPlace(p.getSearchPlace(queryTo)); //Create object searchplace Kiri for To
            //MessageBox.Show("test1");

            /*
            if (from.status.ToString().Equals("ok") && to.status.ToString().Equals("ok")) //check status
            {
                MessageBox.Show("masuk 1");
                if(from.searchresult.Count()==0){ //check From search
                    MessageBox.Show("Pencarian untuk kata " + queryFrom + " tidak ditemuakan");
                }
                else if (from.searchresult.Count() == 1)
                {
                    locFrom = from.searchresult[0].location.ToString();
                }
                else 
                {
                    LayoutRoot.Children.Add(getListItem(from));
                    MessageBox.Show("masuk from");
                }

                if (to.searchresult.Count() == 0)
                { //check From search
                    MessageBox.Show("Pencarian untuk kata " + queryTo + " tidak ditemuakan");
                }
                else if (to.searchresult.Count() == 1)
                {
                    locFrom = to.searchresult[0].location.ToString();
                }
                else
                {
                    LayoutRoot.Children.Add(getListItem(to));
                    MessageBox.Show("masuk to");
                }
            }
            else 
            {
                MessageBox.Show("Error!");
            }
            */

            Task<string> requestFrom = httpClient.GetStringAsync(new Uri(p.getSearchPlace(queryFrom)));
            Task<string> requestTo = httpClient.GetStringAsync(new Uri(p.getSearchPlace(queryTo)));
            // String txt = request.Result;
            //bool txt = request.IsCompleted;

            //String fromText = "BIP";
            //String toText = "";
            /*
            requestFrom.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate{
                    //keluaranFrom.Text = requestFrom.Result;
                    RootObjectSearchPlace r1 = new RootObjectSearchPlace(); //Untuk Asal
                    r1 = Deserialize<RootObjectSearchPlace>(requestFrom.Result); //Mengubah String menjadi objek
                    listPlaceFrom = getListItem(r1);
                    LayoutRoot.Children.Add(listPlaceFrom);
                    //keluaranFrom.Text = ""+r.searchresult.Count;
                    //MessageBox.Show("Error1");
                    // Create a new list box, add items, and add a SelectionChanged handler.
                    
                }));
            });
             * */
            requestTo.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    //keluaranTo.Text = requestTo.Result;
                    RootObjectSearchPlace r1 = new RootObjectSearchPlace(); //Untuk Asal
                    r1 = Deserialize<RootObjectSearchPlace>(requestFrom.Result); //Mengubah String menjadi objek
                    //getListItem(r1);
                    //LayoutRoot.Children.Add(listPlaceFrom);
                    
                    RootObjectSearchPlace r2 = new RootObjectSearchPlace(); //Untuk Tujuan
                    r2 = Deserialize<RootObjectSearchPlace>(requestTo.Result);
                    getListItem(r1,r2);
                    //LayoutRoot.Children.Add(listPlaceTo);
                    
                    //MessageBox.Show("Error2");
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
                        locFrom = requestFrom.searchresult[0].location.ToString();
                    }
                    else
                    {
                        //LayoutRoot.Children.Add(getListItem(requestFrom));
                        for (int c = 0; c < requestFrom.searchresult.Count; c++)
                        {
                            listPlaceFrom.Items.Add(requestFrom.searchresult[c].placename);
                        }
                        listPlaceFrom.Width = 720;
                        listPlaceFrom.Height = 1280;
                        listPlaceFrom.Background = new SolidColorBrush(Colors.Black);
                        listPlaceFrom.FontSize = 30;
                        listPlaceFrom.SelectionChanged += ListBox_SelectedPlaceFrom;
                        LayoutRoot.Children.Add(listPlaceFrom);
                        MessageBox.Show("masuk from");
                    }

                    if (requestTo.searchresult.Count() == 0)
                    { //check From search
                        MessageBox.Show("Pencarian untuk kata " + toBox.Text + " tidak ditemuakan");
                    }
                    else if (requestTo.searchresult.Count() == 1)
                    {
                        locFrom = requestTo.searchresult[0].location.ToString();
                    }
                    else
                    {
                        //LayoutRoot.Children.Add(getListItem(requestTo));
                        for (int c = 0; c < requestTo.searchresult.Count; c++)
                        {
                            listPlaceTo.Items.Add(requestTo.searchresult[c].placename);
                        }
                        listPlaceTo.Width = 720;
                        listPlaceTo.Height = 1280;
                        listPlaceTo.Background = new SolidColorBrush(Colors.Black);
                        listPlaceTo.FontSize = 30;
                        listPlaceTo.SelectionChanged += ListBox_SelectedPlaceTo;
                        LayoutRoot.Children.Add(listPlaceTo);
                        MessageBox.Show("masuk to");
                    }
                }
            }));
            
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
            NavigationService.Navigate(new Uri("/ShowRoute.xaml?start=-6.87474,107.60491&finish=-6.88909,107.59614", UriKind.Relative));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
        
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