
namespace GMap.NET
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;
    using GMap.NET.Internals;
    using System;
    using GMap.NET.MapProviders;
    using System.Threading;
    using GMap.NET.WindowsForms;
    using GMap.NET.WindowsForms.Markers;
    using System.Drawing;

    /// <summary>
    /// form helping to prefetch tiles on local db
    /// </summary>
    public partial class TilePrefetcher : Form
    {

        #region 地图坐标纠偏
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
        }
        #endregion

        BackgroundWorker worker = new BackgroundWorker();
        List<GPoint> list;
        int zoom;
        GMapProvider provider;
        int sleep;
        int all;
        public bool ShowCompleteMessage = false;
        RectLatLng area;
        List<PointLatLng> area1;
        GMap.NET.GSize maxOfTiles;
        public GMapOverlay Overlay;
        int retry;
        public bool Shuffle = true;

        public TilePrefetcher()
        {
            InitializeComponent();

            GMaps.Instance.OnTileCacheComplete += new TileCacheComplete(OnTileCacheComplete);
            GMaps.Instance.OnTileCacheStart += new TileCacheStart(OnTileCacheStart);
            GMaps.Instance.OnTileCacheProgress += new TileCacheProgress(OnTileCacheProgress);

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        readonly AutoResetEvent done = new AutoResetEvent(true);

        void OnTileCacheComplete()
        {
            if (!IsDisposed)
            {
                try
                {
                    done.Set();

                    MethodInvoker m = delegate
                    {
                        label2.Text = "all tiles saved";
                    };
                    Invoke(m);
                }
                catch { }
            }
        }

        void OnTileCacheStart()
        {
            if (!IsDisposed)
            {
                try
                {
                    done.Reset();

                    MethodInvoker m = delegate
                    {
                        label2.Text = "saving tiles...";
                    };
                    Invoke(m);
                }
                catch { }
            }
        }

        void OnTileCacheProgress(int left)
        {
            if (!IsDisposed)
            {
                try
                {
                    MethodInvoker m = delegate
                    {
                        label2.Text = left + " tile to save...";
                    };
                    BeginInvoke(m);
                }
                catch { }
            }
        }

        public void Start(RectLatLng area, int zoom, GMapProvider provider, int sleep, int retry)
        {
            if (!worker.IsBusy)
            {
                this.label1.Text = "...";
                this.progressBarDownload.Value = 0;

                this.area = area;
                this.zoom = zoom;
                this.provider = provider;
                this.sleep = sleep;
                this.retry = retry;

                GMaps.Instance.UseMemoryCache = false;
                GMaps.Instance.CacheOnIdleRead = false;
                GMaps.Instance.BoostCacheEngine = true;

                if (Overlay != null)
                {
                    Overlay.Markers.Clear();
                }

                worker.RunWorkerAsync();

                this.ShowDialog();
            }
        }

        public void Start1(List<PointLatLng> area, int zoom, GMapProvider provider, int sleep, int retry)
        {
            if (!worker.IsBusy)
            {
                this.label1.Text = "...";
                this.progressBarDownload.Value = 0;

                this.area1 = area;
                this.zoom = zoom;
                this.provider = provider;
                this.sleep = sleep;
                this.retry = retry;

                GMaps.Instance.UseMemoryCache = false;
                GMaps.Instance.CacheOnIdleRead = false;
                GMaps.Instance.BoostCacheEngine = true;

                if (Overlay != null)
                {
                    Overlay.Markers.Clear();
                }

                worker.RunWorkerAsync();

                this.ShowDialog();
            }
        }

        public void Stop()
        {
            GMaps.Instance.OnTileCacheComplete -= new TileCacheComplete(OnTileCacheComplete);
            GMaps.Instance.OnTileCacheStart -= new TileCacheStart(OnTileCacheStart);
            GMaps.Instance.OnTileCacheProgress -= new TileCacheProgress(OnTileCacheProgress);

            done.Set();

            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }

            GMaps.Instance.CancelTileCaching();

            done.Close();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ShowCompleteMessage)
            {
                if (!e.Cancelled)
                {
                    MessageBox.Show(this, "Prefetch Complete! => " + ((int)e.Result).ToString() + " of " + all);
                }
                else
                {
                    MessageBox.Show(this, "Prefetch Canceled! => " + ((int)e.Result).ToString() + " of " + all);
                }
            }

            list.Clear();

            GMaps.Instance.UseMemoryCache = true;
            GMaps.Instance.CacheOnIdleRead = true;
            GMaps.Instance.BoostCacheEngine = false;

            worker.Dispose();

            this.Close();
        }

        bool CacheTiles(int zoom, GPoint p)
        {
            foreach (var pr in provider.Overlays)
            {
                Exception ex;
                PureImage img;

                // tile number inversion(BottomLeft -> TopLeft)
                if (pr.InvertedAxisY)
                {
                    img = GMaps.Instance.GetImageFrom(pr, new GPoint(p.X, maxOfTiles.Height - p.Y), zoom, out ex);
                }
                else // ok
                {
                    img = GMaps.Instance.GetImageFrom(pr, p, zoom, out ex);
                }

                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public readonly Queue<GPoint> CachedTiles = new Queue<GPoint>();

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (list != null)
            {
                list.Clear();
                list = null;
            }
            if (!area.IsEmpty)
            {
                list = provider.Projection.GetAreaTileList(area, zoom, 0);
            }
            else
            {
                list = provider.Projection.GetAreaTileList1(area1, zoom, 0);
            }
            maxOfTiles = provider.Projection.GetTileMatrixMaxXY(zoom);
            all = list.Count;

            int countOk = 0;
            int retryCount = 0;

            if (Shuffle)
            {
                Stuff.Shuffle<GPoint>(list);
            }

            lock (this)
            {
                CachedTiles.Clear();
            }

            if (all < MyProcessThreadCount)//小数据量不用开线程
            {
                for (int i = 0; i < all; i++)
                {
                    if (worker.CancellationPending)
                        break;

                    GPoint p = list[i];
                    {
                        if (CacheTiles(zoom, p))
                        {
                            if (Overlay != null)
                            {
                                lock (this)
                                {
                                    CachedTiles.Enqueue(p);
                                }
                            }
                            countOk++;
                            retryCount = 0;
                        }
                        else
                        {
                            if (++retryCount <= retry) // retry only one
                            {
                                i--;
                                System.Threading.Thread.Sleep(1111);
                                continue;
                            }
                            else
                            {
                                retryCount = 0;
                            }
                        }
                    }
                    worker.ReportProgress((int)((i + 1) * 100 / all), i + 1);

                    if (sleep > 0)
                    {
                        System.Threading.Thread.Sleep(sleep);
                    }
                }
            }
            else
            {
                int threadCount = all / MyProcessThreadCount;
                threadCount = threadCount > MyProcessThreadCount ? MyProcessThreadCount : threadCount;
                int step = all / threadCount;
                int last = all % threadCount;
                MyThreadData.ProcessCount = 0;
                for (int i = 0; i < threadCount; i++)
                {
                    ParameterizedThreadStart ParStart = new ParameterizedThreadStart(ThreadMethod);
                    int start =i*step;
                    int end = (i+1)*step;
                    if (i == threadCount - 1)
                        end += last;
                    MyThreadData data = new MyThreadData(start,end);
                    Thread myThread = new Thread(ParStart);
                    myThread.Start(data);
                }
                while (true)
                {
                    if (MyThreadData.ThreadDataExist > 0)
                    {
                        System.Threading.Thread.Sleep(100);
                        int percent = (int)((MyThreadData.ProcessCount) * 100 / all);

                        worker.ReportProgress(percent > 101 ? 100 : percent, MyThreadData.ProcessCount > all ? all : MyThreadData.ProcessCount);
                    }
                    else
                        break;
                }
            }
            e.Result = countOk;

            if (!IsDisposed)
            {
                done.WaitOne();
            }
        }
        private const int MyProcessThreadCount = 100;
        class MyThreadData
        {
            public int start = 0;
            public int end = 0;
            private static int _ProcessCount = 0;
            private static object ProcessCountLocker = new object();
            public static int ProcessCount
            {
                get { return _ProcessCount; }
                set
                {
                    lock (ProcessCountLocker)
                    {
                        _ProcessCount = value;
                    }
                }
            }
            private static object mylocker = new object();
            public MyThreadData(int st = 0, int en =0)
            {
                start = st;
                end = en;
                ThreadDataExist++;
            }
            private static int _ThreadDataExist = 0;
            public static int ThreadDataExist
            {
                get { return _ThreadDataExist; }
                set
                {
                    lock (mylocker)
                    {
                        _ThreadDataExist = value;
                    }
                }
            }
        }
        public void ThreadMethod(object data)
        {
            int start = ((MyThreadData)data).start;
            int end = ((MyThreadData)data).end;

            int retryCount = 0;
            int countOk = 0;
            for (int i = start; i < end; i++)
            {
                if (worker.CancellationPending)
                    break;

                GPoint p = list[i];
                {
                    if (CacheTiles(zoom, p))
                    {
                        if (Overlay != null)
                        {
                            lock (this)
                            {
                                CachedTiles.Enqueue(p);
                            }
                        }
                        countOk++;
                        retryCount = 0;
                    }
                    else
                    {
                        if (++retryCount <= retry) // retry only one
                        {
                            i--;
                            System.Threading.Thread.Sleep(1111);
                            continue;
                        }
                        else
                        {
                            retryCount = 0;
                        }
                    }
                }
                MyThreadData.ProcessCount++;

                if (sleep > 0)
                {
                    System.Threading.Thread.Sleep(sleep);
                }
            }
            MyThreadData.ThreadDataExist--;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.label1.Text = "Fetching tile at zoom (" + zoom + "): " + ((int)e.UserState).ToString() + " of " + all + ", complete: " + e.ProgressPercentage.ToString() + "%";
            this.progressBarDownload.Value = e.ProgressPercentage;

            if (Overlay != null)
            {
                GPoint? l = null;

                lock (this)
                {
                    if (CachedTiles.Count > 0)
                    {
                        l = CachedTiles.Dequeue();
                    }
                }

                if (l.HasValue)
                {
                    var px = Overlay.Control.MapProvider.Projection.FromTileXYToPixel(l.Value);
                    var p = Overlay.Control.MapProvider.Projection.FromPixelToLatLng(px, zoom);

                    var r1 = Overlay.Control.MapProvider.Projection.GetGroundResolution(zoom, p.Lat);
                    var r2 = Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, p.Lat);
                    var sizeDiff = r2 / r1;

                    GMapMarkerTile m = new GMapMarkerTile(p, (int)(Overlay.Control.MapProvider.Projection.TileSize.Width / sizeDiff));
                    Overlay.Markers.Add(m);
                }
            }
        }

        private void Prefetch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void Prefetch_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Stop();
        }
    }

    class GMapMarkerTile : GMapMarker
    {
        static Brush Fill = new SolidBrush(Color.FromArgb(155, Color.Blue));

        public GMapMarkerTile(PointLatLng p, int size)
            : base(p)
        {
            Size = new System.Drawing.Size(size, size);
        }

        public override void OnRender(Graphics g)
        {
            g.FillRectangle(Fill, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
        }
    }
}
