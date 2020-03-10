using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.WindowsForms
{
    /// <summary>
    /// WGS-84 到 GCJ-02 的转换
    /// </summary>
    public class EvilTransform
    {
        const double pi = 3.14159265358979324;

        //
        // Krasovsky 1940
        //
        // a = 6378245.0, 1/f = 298.3
        // b = a * (1 - f)
        // ee = (a^2 - b^2) / a^2;
        const double a = 6378245.0;
        const double ee = 0.00669342162296594323;

        /// <summary>
        /// GCJ-02　to WGS-84
        /// </summary>
        /// <param name="GcLat"></param>
        /// <param name="GcLon"></param>
        /// <returns></returns>
        public static PointLatLng reTransform(double GcLat, double GcLon)
        {
            double lontitude = GcLon * 2 - transform(GcLat, GcLon).Lng;
            double latitude = GcLat * 2 - transform(GcLat, GcLon).Lat;
            return new PointLatLng(latitude, lontitude);
        }

        public static double PgisOffset = 1;
        /// <summary>
        /// WGS-84 to GCJ-02
        /// </summary>
        /// <param name="wgLat"></param>
        /// <param name="wgLon"></param>
        /// <returns></returns>
        public static PointLatLng transform(double wgLat, double wgLon)
        {
            double mgLat = 0;
            double mgLon = 0;
            if (outOfChina(wgLat, wgLon))
            {
                mgLat = wgLat;
                mgLon = wgLon;
                return new PointLatLng(mgLat, mgLon);
            }
            double dLat = transformLat(wgLon - 105.0, wgLat - 35.0);
            double dLon = transformLon(wgLon - 105.0, wgLat - 35.0);
            double radLat = wgLat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            mgLat = wgLat + dLat;
            mgLon = wgLon + dLon;
            return new PointLatLng(mgLat, mgLon);
        }

        static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }


        static double getLat(double lat,GMapProvider provider , double start = 26.1911, double end = 30.21635, double topOffset = 0.4663)
        {
            if (provider.Name == "PgisSLYX")
            {
                topOffset = 1.55;
                lat += -3.116;
                double step = 0;
                step = topOffset / Math.Abs(start - end);
                double prev = lat;
                lat = lat - (lat - start) * step;
                lat = Math.Atan(lat * pi / 180);
                lat = lat * 180 / pi;
                lat += Math.Sin(prev * pi / 180) * 7.5;
            }
            else if (provider.Name.Contains("Pgis"))
            {
                lat += -3.696;
                double step = 0;
                step = topOffset / Math.Abs(start - end);
                double prev = lat;
                lat = lat - (lat - start) * step;
                lat = Math.Atan(lat * pi / 180);
                lat = lat * 180 / pi;
                lat += Math.Sin(prev * pi / 180) * 9.2;
            }
            return lat;
        }
        static double getLon(double lon, GMapProvider provider , double start = 118.1291, double end = 122.2393, double realEnd = 121.0363)
        {  
            if (provider.Name == "PgisSLYX")
            {
                lon += 0.096;
                double step = (end - realEnd) / (end - start);
                lon = lon - (lon - start) * step;
                
            }
            else if (provider.Name == "Pgis")
            {
                lon -= 0.011;
            }
            return lon;
        }

        public static PointLatLng transform(PointLatLng P ,GMapProvider provider = null)
        {
            double mgLat = 0;
            double mgLon = 0;
            if (outOfChina(P.Lat, P.Lng))
            {
                mgLat = P.Lat;
                mgLon = P.Lng;
                return new PointLatLng(mgLat, mgLon);
            }
            if (provider!=null)
            {
                P.Lat = getLat(P.Lat, provider);
                P.Lng = getLon(P.Lng, provider);
            }

            double dLat = transformLat(P.Lng - 105.0, P.Lat - 35.0);
            double dLon = transformLon(P.Lng - 105.0, P.Lat - 35.0);
            double radLat = P.Lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            mgLat = P.Lat + dLat;
            mgLon = P.Lng + dLon;
            return new PointLatLng(mgLat, mgLon);
        }
    }
}
