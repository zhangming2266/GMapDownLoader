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
    public abstract class PgisProviderBase : GMapProvider
    {
        private string ClientKey = "1308e84a0e8a1fc2115263a4b3cf87f1";
        public PgisProviderBase()
        {
            MaxZoom = null;
            RefererUrl = Ip;
            RefererPort = "8080";
            Copyright = "";
        }

        //public static string Ip = "41.188.16.233";
        public static string Ip = "127.0.0.1";

        protected string MapKind = "PGISSL";
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

    public abstract class PgisProviderNewBase : PgisProviderBase
    {
        public static readonly PgisProviderNewBase Instance;


        protected string version = "1.0.0";
        protected string name = "PgisNew";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            pos.Y -= (int)Math.Pow(2, zoom - 2);
            if (pos.Y < 0)
                return null;
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);
            return GetTileImageUsingHttp(url);
        }


        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            #region

            string url = string.Format(ImageUrl, pos.X, pos.Y, zoom);
            return url;
            #endregion
        }

        string ImageUrl
        {
            get
            {
                return
                    string.Format("http://{0}:{2}/PGIS_S_TileMapServer/Maps/{1}/EzMap?Service=getImage&Type=RGB&", RefererUrl, MapKind, RefererPort) + "ZoomOffset=0&Col={0}&Row={1}&Zoom={2}&V=" + version;
            }
        }
    }
    /// <summary>
    /// 导航
    /// </summary>
    public class PgisProviderGDDH : PgisProviderNewBase
    {
        public static readonly PgisProviderGDDH Instance;

        protected Guid id = new Guid("27282cc8-1278-46bf-89c4-89568789E28A");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderGDDH()
        {
            Instance = new PgisProviderGDDH()
            {
                name = "PgisGDDH",
                version = "1.0.0",
                MapKind = "GDDH-new",
                MaxZoom = 18
            };
        }

    }
    /// <summary>
    /// 影像地图
    /// </summary>
    public class PgisProviderTDTYX : PgisProviderNewBase
    {
        public static readonly PgisProviderTDTYX Instance;
        protected Guid id = new Guid("27282558-1278-46bf-89c4-89568789E789");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderTDTYX()
        {
            Instance = new PgisProviderTDTYX()
            {
                name = "PgisTDTYX",
                version = "0.3",
                MapKind = "TDTYX",
                MaxZoom = 20
            };
        }

    }

    /// <summary>
    /// 矢量地图
    /// </summary>
    public class PgisProviderPGISSL : PgisProviderNewBase
    {
        public static readonly PgisProviderPGISSL Instance;
        protected Guid id = new Guid("27282558-1234-46bf-89c4-89568789E789");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderPGISSL()
        {
            Instance = new PgisProviderPGISSL()
            {
                name = "PgisPGISSL",
                version = "1.0.0",
                MapKind = "PGISSL",
                //RefererUrl = "10.119.255.58"
                RefererUrl = "127.0.0.1"
            };
        }

    }

    /// <summary>
    /// 水系
    /// </summary>
    public class PgisProviderTDTSX : PgisProviderNewBase
    {
        public static readonly PgisProviderTDTSX Instance;
        protected Guid id = new Guid("27282558-1235-46bf-8994-89568789E789");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderTDTSX()
        {
            Instance = new PgisProviderTDTSX()
            {
                name = "PgisTDTS",
                version = "0.3",
                MapKind = "TDTSX"
            };
        }

    }

    /// <summary>
    /// 地貌
    /// </summary>
    public class PgisProviderTDTDM : PgisProviderNewBase
    {
        public static readonly PgisProviderTDTDM Instance;
        protected Guid id = new Guid("27282558-2235-87bf-89c4-89568789E789");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderTDTDM()
        {
            Instance = new PgisProviderTDTDM()
            {
                name = "PgisTDTDM",
                version = "0.3",
                MapKind = "TDTDM"
            };
        }

    }

    /// <summary>
    /// 交通
    /// </summary>
    public class PgisProviderTDTJT : PgisProviderNewBase
    {
        public static readonly PgisProviderTDTJT Instance;
        protected Guid id = new Guid("27282558-2235-46bf-12c4-89568789E789");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderTDTJT()
        {
            Instance = new PgisProviderTDTJT()
            {
                name = "PgisTDTJT",
                version = "0.3",
                MapKind = "TDTJT"
            };
        }

    }
    /// <summary>
    /// 天地图矢影
    /// </summary>
    public class PgisProviderTDTSY : PgisProviderNewBase
    {
        public static readonly PgisProviderTDTSY Instance;
        protected Guid id = new Guid("27123558-2235-46bf-89c4-89568789E789");
        public override Guid Id
        {
            get { return id; }
        }
        static PgisProviderTDTSY()
        {
            Instance = new PgisProviderTDTSY()
            {
                name = "PgisTDTSY",
                version = "0.3",
                MapKind = "TDTSY"
            };
        }

    }

}
