using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kiri
{
    class Protocol
    {
        public String uri_version
        {
            get
            {
                return "version=";
            }
        }
        public String uri_mode
        {
            get
            {
                return "&mode=";
            }
        }
        public String uri_locale
        {
            get
            {
                return "&locale=";
            }
        }
        public String uri_start
        {
            get
            {
                return "&start=";
            }
        }
        public String uri_finish
        {
            get
            {
                return "&finish=";
            }
        }
        public String uri_presentation
        {
            get
            {
                return "&presentation=";
            }
        }
        public String uri_apikey
        {
            get
            {
                return "&apikey=";
            }
        }
        public String uri_region
        {
            get
            {
                return "&region=";
            }
        }
        public String uri_query
        {
            get
            {
                return "&querystring=";
            }
        }

        private static String apiKey 
        {
            get 
            { 
                return "97A7A1157A05ED6F";    
            }
        }
        private static String hostname
        {
            get
            {
                return "http://kiri.travel/";
            }
        }
        private static String handle
        {
            get
            {
                return hostname+"handle.php?";
            }
        }
        private static String iconPath
        {
            get
            {
                return hostname + "images/means/";
            }
        }
        public String iconStart
        {
            get
            {
                return hostname + "images/stepicon-walkstart.png";
            }
        }
        public String iconFinish
        {
            get
            {
                return hostname + "images/stepicon-finish.png";
            }
        }


        private static String version_2
        {
            get
            {
                return "2";
            }
        }

        private static String modeFind
        {
            get
            {
                return "searchplace";
            }
        }

        private static String modeRoute
        {
            get
            {
                return "findroute";
            }
        }
        private static String modeNearby
        {
            get
            {
                return "nearbytransport";
            }
        }
        private static String localeId
        {
            get
            {
                return "id";
            }
        }
        private static String localeEn
        {
            get
            {
                return "en";
            }
        }
        private static String presentationMobile
        {
            get
            {
                return "mobile";
            }
        }
        private static String presentationDesktop
        {
            get
            {
                return "desktop";
            }
        }

        public string getTypeTransport(string means, string meansDetail)
        {

            String uri = iconPath + means + "/baloon/" + meansDetail + ".png";
            return uri;
        }

        public string getSearchPlace(string query,string region)
        {
            String uri = handle + uri_version + version_2 + uri_mode + modeFind + uri_region + region + uri_query + query + uri_apikey + apiKey;
            return uri;
        }

        public string getFindRoute(string start, string finish)
        {
            String uri = handle + uri_version + version_2 + uri_mode + modeRoute + uri_locale + localeId + uri_start + start + uri_finish + finish + uri_presentation + presentationDesktop + uri_apikey + apiKey;
            return uri;
        }
    }
}
