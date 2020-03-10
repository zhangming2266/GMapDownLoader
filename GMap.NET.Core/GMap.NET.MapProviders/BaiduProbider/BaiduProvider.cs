using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders
{
   
        public abstract class BaiduMapProviderBase : GMapProvider
        {
            private string ClientKey = "1308e84a0e8a1fc2115263a4b3cf87f1";
            public BaiduMapProviderBase()
            {
                MaxZoom = null;
              //  RefererUrl = "http://map.baidu.com";
                RefererUrl = "http://www.amap.com/";
                Copyright ="";
            }

            public override PureProjection Projection
            {
                get { return MercatorProjection.Instance; }
            }

            GMapProvider[] overlays;
            public override GMapProvider[] Overlays
            {
                get
                {
                    if (overlays == null)
                    {
                        overlays = new GMapProvider[] { this };
                    }
                    return overlays;
                }
            }
        }

        public class BaiduMapProvider : BaiduMapProviderBase
        {
            public static readonly BaiduMapProvider Instance;

            readonly Guid id = new Guid("608748FC-5FDD-4d3a-9027-356F24A755E5");
            public override Guid Id
            {
                get { return id; }
            }

            readonly string name = "BaiduMap";
            public override string Name
            {
                get
                {
                    return name;
                }
            }

            static BaiduMapProvider()
            {
                Instance = new BaiduMapProvider();
            }

            public override PureImage GetTileImage(GPoint pos, int zoom)
            {
                string url = MakeTileImageUrl(pos, zoom, LanguageStr);

                return GetTileImageUsingHttp(url);
            }

            string MakeTileImageUrl(GPoint pos, int zoom, string language)
            {
                #region 百度地图
                //zoom = zoom - 1;
                //var offsetX = Math.Pow(2, zoom);
                //var offsetY = offsetX - 1;

                //var numX = pos.X - offsetX;
                //var numY = -pos.Y + offsetY;

                //zoom = zoom + 1;
                //var num = (pos.X + pos.Y) % 8 + 1;
                //var x = numX.ToString().Replace("-", "M");
                //var y = numY.ToString().Replace("-", "M");;
                //string url = string.Format(UrlFormat, x, y, zoom);
                //Console.WriteLine("url:" + url);
                //return url;
                #endregion

                #region
                var num = (pos.X + pos.Y) % 4 + 1;
                //string url = string.Format(UrlFormat, num, pos.X, pos.Y, zoom);
                string url = string.Format(UrlFormat, pos.X, pos.Y, zoom);
                return url;
                #endregion
            }

            //百度地图
          //  static readonly string UrlFormat = "http://online1.map.bdimg.com/tile/?qt=tile&x={0}&y={1}&z={2}&styles=pl";

            //高德地图
            static readonly string UrlFormat = "http://webrd01.is.autonavi.com/appmaptile?lang=zh_cn&size=1&scale=1&style=8&x={0}&y={1}&z={2}";

           // static readonly string UrlFormat = "http://127.0.0.1:8080?lang=zh_cn&size=1&scale=1&style=8&x={0}&y={1}&z={2}";
        }

}
