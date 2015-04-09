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

using System.Device.Location; //ilangin kalo buat kelas city

namespace Kiri
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private Protocol protocol;
        private LocationFinder lFinder;
        private HttpClient httpClient = new HttpClient();
        private City c;
        private String myCity;

        private BackgroundWorker backgroundWorker;
        
        public MainPage()
        {
            InitializeComponent();
            this.lFinder = new LocationFinder();
            this.c = new City();
            this.cmbCurrFrom.ItemsSource = c.city;
            this.protocol = new Protocol();
            ShowSplash();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (PhoneApplicationService.Current.State.ContainsKey("location"))
            {
                this.lFinder = (LocationFinder)PhoneApplicationService.Current.State["location"];
                //check form
                if (lFinder.coorLatFrom == 0.0 && lFinder.coorLongFrom == 0.0 && !lFinder.addressFrom.Equals("Maps") && !lFinder.addressFrom.Equals("Here")) {
                    fromBox.Text = lFinder.addressFrom;
                }else if (lFinder.coorLatFrom != 0.0 && lFinder.coorLongFrom != 0.0) {
                    if (lFinder.addressFrom.Equals("Here"))
                    {
                        fromBox.Text = "Here";
                    }
                    else 
                    {
                        fromBox.Text = "Maps";
                    }
                }
                if (lFinder.coorLatTo == 0.0 && lFinder.coorLongTo == 0.0 && !lFinder.addressTo.Equals("Maps") && lFinder.addressTo.Equals("Here")){
                    toBox.Text = lFinder.addressTo;
                }else if (lFinder.coorLatTo != 0.0 && lFinder.coorLongTo != 0.0)
                {
                    if (lFinder.addressTo.Equals("Here"))
                    {
                        toBox.Text = "Here";
                    }
                    else 
                    {
                        toBox.Text = "Maps";
                    }
                }
            }
            string forMaps = "";
            if (NavigationContext.QueryString.TryGetValue("for", out forMaps)) ;
            if (forMaps!=null)
            {
                if (forMaps.Equals("from"))
                {
                    fromBox.Text = "Maps";
                    lFinder.addressFrom = "Maps";
                }
                else
                {
                    toBox.Text = "Maps";
                    lFinder.addressTo = "Maps";
                }
            }
        }

        private async void startRoute(object sender, RoutedEventArgs e)
        {
            
            String queryFrom = fromBox.Text;
            String queryTo = toBox.Text;
            RootObjectSearchPlace from = null; //Untuk Asal
            RootObjectSearchPlace to = null; //Untuk Tujuan
            //Penting!
            //Dokumentasi zhttps://bitbucket.org/projectkiri/kiri_api/wiki/KIRI%20API%20v2%20Documentation
            //Contoh searchplace    zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=bip&apikey=97A7A1157A05ED6F
            //                      zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=pvj&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.90864,107.61108&finish=-6.88929,107.59574&presentation=mobile&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.87474,107.60491&finish=-6.88909,107.59614&presentation=mobile&apikey=97A7A1157A05ED6F    

            //Check form
            if (!queryFrom.Equals("") && !queryTo.Equals(""))
            {
                //Validate From
                Boolean routeStatus = true;
                progressFindPlace.IsIndeterminate = true;
                //Get place from query
                if ((!queryFrom.Equals("Here") || !queryFrom.Equals("Maps")) && (lFinder.coorLatFrom == 0.0 && lFinder.coorLongFrom == 0.0)) //Check get location from GPS
                {
                    //Reference zhttps://msdn.microsoft.com/en-us/library/hh191443.aspx
                    //Task<string> requestFromTask = httpClient.GetStringAsync(new Uri(protocol.getSearchPlace(queryFrom, myCity)));
                    //requestFrom = await requestFromTask;
                    //from = new RootObjectSearchPlace();
                    //from = JsonConvert.DeserializeObject<RootObjectSearchPlace>(requestFrom); //Mengubah String menjadi objek
                    from = await protocol.getRequestSearch(queryFrom, myCity); //Mengubah String menjadi objek
                    if (from.searchresult.Count() == 0)
                    {
                        MessageBox.Show("Pencarian untuk kata " + fromBox.Text + " tidak ditemukan");
                        routeStatus = false;
                    }
                }
                if ((!queryTo.Equals("Here") || !queryTo.Equals("Maps")) && (lFinder.coorLatTo == 0.0 && lFinder.coorLongTo == 0.0)) //Check get location from GPS
                {
                    //Task<string> requestToTask = httpClient.GetStringAsync(new Uri(protocol.getSearchPlace(queryTo, myCity)));
                    //requestTo = await requestToTask;
                    //to = new RootObjectSearchPlace();
                    //to = JsonConvert.DeserializeObject<RootObjectSearchPlace>(requestTo);//Mengubah String menjadi objek
                    to = await protocol.getRequestSearch(queryTo, myCity); //Mengubah String menjadi objek
                    if (to.searchresult.Count() == 0)
                    {
                        MessageBox.Show("Pencarian untuk kata " + toBox.Text + " tidak ditemukan");
                        routeStatus = false;
                    }
                }
                //Check Query
                if (routeStatus == true)
                {
                    if ((lFinder.coorLatFrom == 0.0 && lFinder.coorLongFrom == 0.0))
                    {
                        getListItem(from, "from"); //Show Listbox for location From
                    }
                    if ((lFinder.coorLatTo == 0.0 && lFinder.coorLongTo == 0.0))
                    {
                        getListItem(to, "to");  //Show Listbox for location To
                    }
                    this.findRoute();
                }
                progressFindPlace.IsIndeterminate = false;
            }
            else {
                MessageBox.Show("Harap melengkapi tempat asal dan tempat tujuan");
            }
        }

        private void changeMapFrom(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["location"] = lFinder;
            NavigationService.Navigate(new Uri("/Map.xaml?fromMapFor=from", UriKind.Relative));
            //NavigationService.Navigate(new Uri("/Map.xaml?fromMapFor=from", UriKind.Relative));    
        }
        private void changeMapTo(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["location"] = lFinder;
            NavigationService.Navigate(new Uri("/Map.xaml?fromMapFor=to", UriKind.Relative));    
        }

        private void getHereFrom(object sender, RoutedEventArgs e)
        {
            this.lFinder.setCoordinateHere("from");
            fromBox.Text = "Here";
            lFinder.addressFrom = "Here";
        }

        private void getHereTo(object sender, RoutedEventArgs e)
        {
            this.lFinder.setCoordinateHere("to");
            toBox.Text = "Here";
            lFinder.addressTo = "Here";
        }

        /* old reference: zhttps://msdn.microsoft.com/en-us/library/bb412179%28v=vs.110%29.aspx*/

        //zhttps://social.msdn.microsoft.com/forums/windowsapps/en-us/7db73c64-86f7-4c43-9fd4-faa03421ea21/popup-blocking-listbox-selection-changed
        private void getListItem(RootObjectSearchPlace request,String forRequest)
        {
            //ListBox listFrom = new ListBox();
            //ListBox listTo = new ListBox();
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (request.status.ToString().Equals("ok")) //check status
                {
                    if (request.searchresult.Count() == 1)
                    {
                        if (forRequest.Equals("from"))
                        {
                            String locFrom = request.searchresult[0].location.ToString();
                            string[] coordinate = locFrom.Split(',');
                            lFinder.coorLatFrom = Double.Parse(coordinate[0]);
                            lFinder.coorLongFrom = Double.Parse(coordinate[1]);
                        }
                        else
                        {
                            String locTo = request.searchresult[0].location.ToString();
                            string[] coordinate = locTo.Split(',');
                            lFinder.coorLatTo = Double.Parse(coordinate[0]);
                            lFinder.coorLongTo = Double.Parse(coordinate[1]);
                        }
                        this.findRoute();
                    }
                    else
                    {
                        //LayoutRoot.Children.Add(getListItem(requestFrom));
                        if (forRequest.Equals("from"))
                        {
                            for (int c = 0; c < request.searchresult.Count; c++)
                            {
                                listPlaceFrom.Items.Add(request.searchresult[c].placename);
                            }
                            panelFrom.Visibility = Visibility.Visible;
                            listPlaceFrom.DataContext = request.searchresult;
                            listPlaceFrom.SelectionChanged += ListBoxSelectedPlace;
                        }
                        else
                        {
                            panelTo.Visibility = Visibility.Visible;
                            for (int c = 0; c < request.searchresult.Count; c++)
                            {
                                listPlaceTo.Items.Add(request.searchresult[c].placename);
                            }
                            listPlaceTo.DataContext = request.searchresult;
                            listPlaceTo.SelectionChanged += ListBoxSelectedPlace;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error!");
                }
            }));
        }

        private void ListBoxSelectedPlace(object sender, SelectionChangedEventArgs e)
        {
            if (panelFrom.Visibility.Equals(Visibility.Visible))
            { //Check visibility
                if (null != (sender as ListBox).SelectedItem)
                {
                    if ((sender as ListBox).SelectedIndex >= 0)
                    {
                        String locFrom = searchCoordinatePlace((List<Searchresult>)listPlaceFrom.DataContext, listPlaceFrom.SelectedItem.ToString());  //((sender as ListBox).SelectedItem as Searchresult).placename;
                        string[] coordinate = locFrom.Split(',');
                        lFinder.coorLatFrom = Double.Parse(coordinate[0]);
                        lFinder.coorLongFrom = Double.Parse(coordinate[1]);
                        lFinder.addressFrom = listPlaceFrom.SelectedItem.ToString();
                    }
                }
                panelFrom.Children.Clear();
                panelFrom.Visibility = Visibility.Collapsed;
                //panelTo.Visibility = Visibility.Visible;
            }
            else {
                if (null != (sender as ListBox).SelectedItem)
                {
                    if ((sender as ListBox).SelectedIndex >= 0)
                    {
                        String locTo = searchCoordinatePlace((List<Searchresult>)listPlaceTo.DataContext, listPlaceTo.SelectedItem.ToString()); //((sender as ListBox).SelectedItem as Searchresult).placename;
                        string[] coordinate = locTo.Split(',');
                        lFinder.coorLatTo = Double.Parse(coordinate[0]);
                        lFinder.coorLongTo = Double.Parse(coordinate[1]);
                        lFinder.addressTo = listPlaceTo.SelectedItem.ToString();
                    }
                }
                panelTo.Children.Clear();
                panelTo.Visibility = Visibility.Collapsed;
            }
            this.findRoute();
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

        public void findRoute()
        {
            if ((lFinder.coorLatFrom != 0.0 && lFinder.coorLongFrom != 0.0) && (lFinder.coorLatTo != 0.0 && lFinder.coorLongTo != 0.0))
            {
                PhoneApplicationService.Current.State["location"] = lFinder;
                NavigationService.Navigate(new Uri("/Route.xaml?", UriKind.Relative)); //start=" + locationFrom + "&nameFrom=" + fromBox.Text + "&finish=" + locationTo + "&nameTo=" + toBox.Text
            }
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

        public void showCity() {
            int indexCity = -1;
            while (indexCity == -1)
            {
                indexCity = c.getNearby(lFinder.coorLat, lFinder.coorLong);
            }
            this.myCity = c.cityCode[indexCity];
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

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            showCity();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.cmbCurrFrom.SelectedIndex = c.getIndexFromCityCode(myCity);
                this.popup.IsOpen = false;
            });
        }
    }
}