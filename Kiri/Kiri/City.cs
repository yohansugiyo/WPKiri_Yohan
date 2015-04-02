using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiri
{
    class City
    {
        private GeoCoordinate[] centerCity;
        //String[,] city = { { "Auto", "Bandung", "Jakarta", "Malang", "Surabaya" }, { "Auto", "bdo", "cgk", "mlg", "sub" } };
        public String[] city;
        public String[] cityCode;

        public City(){
            this.city = new String[] {"Bandung", "Jakarta", "Malang", "Surabaya" };
            this.cityCode = new String[]{"bdo", "cgk", "mlg", "sub" };
            this.centerCity = new GeoCoordinate[] { new GeoCoordinate(-6.91474, 107.60981), new GeoCoordinate(-6.21154, 106.84517), new GeoCoordinate(-7.9812985, 112.6319264), new GeoCoordinate(-7.27421, 112.71908) };
        }

        public int getNearby(Double coorLat, Double coorLong)
        {
            int s = -1;
            GeoCoordinate deviceLocation = new GeoCoordinate(coorLat, coorLong);
            double distance = Double.MaxValue;
            if (!coorLat.Equals(0.0) && !coorLong.Equals(0.0))
            {
                for (int c = 0; c < centerCity.Length; c++)
                {
                    if (deviceLocation.GetDistanceTo(centerCity[c]) < distance)
                    {
                        distance = deviceLocation.GetDistanceTo(centerCity[c]);
                        s = c;
                    }
                }
            }
            return s;
        }

        public int getIndexFromCityCode(String code)
        {
            int i = -1;
            for (int c = 0; c < cityCode.Length; c++)
            {
                if (cityCode[c].Equals(code))
                {
                    i = c;
                }
            }
            return i;
        }
    }

}
