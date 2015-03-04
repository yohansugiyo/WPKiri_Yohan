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
            InitializeComponent();
            Find();
        }

        //source zhttps://msdn.microsoft.com/en-us/library/windows/apps/ff626521%28v=vs.105%29.aspx
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string start = ""; 
            string finish = "";
            if (NavigationContext.QueryString.TryGetValue("start", out start));
            startCoordinate = start;
            if (NavigationContext.QueryString.TryGetValue("finish", out finish));
            finishCoordinate = finish;
            //start = NavigationContext.QueryString["start"];
            //finish = NavigationContext.QueryString["finish"];
        }

        public void Find(){
            //Testing
            HttpClient httpClient = new HttpClient();
            Protocol p = new Protocol();
            String uri = "";

            //String uri = "http://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.87474,107.60491&finish=-6.88909,107.59614&presentation=mobile&apikey=97A7A1157A05ED6F";

            MessageBox.Show(startCoordinate);
            uri = p.getFindRoute(startCoordinate, finishCoordinate);
            
            MessageBox.Show(uri);
            Task<string> requestRoute = httpClient.GetStringAsync(new Uri(uri));
            requestRoute.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    //requestRoute = httpClient.GetStringAsync(new Uri(uri));
                    //keluaranFrom.Text = requestFrom.Result;

                    RootObjectFindRoute r = new RootObjectFindRoute(); //route

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
                    //MessageBox.Show(s);
                    r = JsonConvert.DeserializeObject<RootObjectFindRoute>(s); //Mengubah String menjadi objek
                    
                    //Pushpin pic = new Pushpin();
                    MapPolyline routeRoad = new MapPolyline();
                    routeRoad.StrokeThickness = 2;
                    
                    for (int i = 0; i < r.routingresults.Count; i++){
                        for(int j = 0;j<r.routingresults[i].steps.Count;j++){
                            //MessageBox.Show(r.routingresults[i].steps[j][0].ToString());
                            //MessageBox.Show(r.routingresults[i].steps[j][2].ToString());
                            GeoCoordinate geoCoo = new GeoCoordinate();
                            String temp = r.routingresults[i].steps[j][2].ToString();
                            temp = temp.Replace("[","");
                            temp = temp.Replace("]", "");
                            temp = temp.Replace("\"", "");
                            temp = temp.Replace(" ", "");
                            string[] coordinate = temp.Split(',');
                            for (int c = 0;c<coordinate.Length ;c=c+2 )
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

        //JSON data to c# using JSON.NET
        //package from zhttp://www.nuget.org/packages/newtonsoft.json
        // dok zhttp://www.newtonsoft.com/json/help/html/SerializingJSON.htm
        public static T Deserialize2<T>(string json)
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