﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Demo.WindowsForms;
using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Net;
using GMap.NET.Internals;
using MetroPro.DebugHelper;
using Demo.WindowsPresentation.Source;

namespace Demo.WindowsPresentation
{
    public partial class MainWindow : Window
    {
        PointLatLng start;
        PointLatLng end;

        // marker
        GMapMarker currentMarker;
        GmapGpsOverlay MarkerOverLayer;
        GMapOverlay TrackingLayer;
        // zones list
        List<GMapMarker> Circles = new List<GMapMarker>();


        private List<PointLatLng> cityAreaPoint = new List<PointLatLng>();
        private void DrawCityArea(int i = 0)
        {
            return;
            try
            {
                cityAreaPoint.Clear();
                TrackingLayer.Clear();
                string path = AppDomain.CurrentDomain.BaseDirectory + @"china\";
                DirectoryInfo theFolder = new DirectoryInfo(path);
                DirectoryInfo[] dirInfo = theFolder.GetDirectories();
                foreach (DirectoryInfo NextFolder in dirInfo)
                {
                    if (i == 0)
                    {
                        if (NextFolder.Name == "省")
                        {
                            FileInfo[] fileInfo = NextFolder.GetFiles();
                            foreach (FileInfo NextFile in fileInfo)
                            {
                                if (NextFile.Name.Substring(0, NextFile.Name.IndexOf(".")) == "四川")
                                {
                                    cityAreaPoint.Clear();
                                    string strArea = File.ReadAllText(NextFile.Directory + @"\" + NextFile.Name);
                                    while (strArea.IndexOf(";") > 0)
                                    {
                                        string tempstr = strArea.Substring(0, strArea.IndexOf(";"));
                                        strArea = strArea.Substring(strArea.IndexOf(";") + 1, strArea.Length - strArea.IndexOf(";") - 1);
                                        string tempstrlat = tempstr.Substring(tempstr.IndexOf(",") + 1, tempstr.Length - tempstr.IndexOf(",") - 1);
                                        string tempstrlon = tempstr.Substring(0, tempstr.IndexOf(","));
                                        PointLatLng tempPoint = new PointLatLng(double.Parse(tempstrlat), double.Parse(tempstrlon));
                                        tempPoint.Lat = tempPoint.Lat;
                                        tempPoint.Lng = tempPoint.Lng;
                                        tempPoint = EvilTransform.transform(tempPoint.Lat, tempPoint.Lng);
                                        cityAreaPoint.Add(tempPoint);
                                    }
                                    string tempstrlat1 = strArea.Substring(strArea.IndexOf(",") + 1, strArea.Length - strArea.IndexOf(",") - 1);
                                    string tempstrlon1 = strArea.Substring(0, strArea.IndexOf(","));
                                    PointLatLng tempPoint1 = new PointLatLng(double.Parse(tempstrlat1), double.Parse(tempstrlon1));
                                    tempPoint1.Lat = tempPoint1.Lat;
                                    tempPoint1.Lng = tempPoint1.Lng;
                                    tempPoint1 = EvilTransform.transform(tempPoint1.Lat, tempPoint1.Lng);
                                    cityAreaPoint.Add(tempPoint1);
                                    GMapRoute r = new GMapRoute(cityAreaPoint, "Gps");//将路转换成线   

                                    TrackingLayer.Markers.Add(r);//将道路加入图层
                                }
                            }
                        }
                    }
                    if (NextFolder.Name == "四川")
                    {
                        FileInfo[] fileInfo = NextFolder.GetFiles();
                        foreach (FileInfo NextFile in fileInfo)
                        {
                            cityAreaPoint.Clear();
                            string strArea = File.ReadAllText(NextFile.Directory + @"\" + NextFile.Name);
                            while (strArea.IndexOf(";") > 0)
                            {
                                string tempstr = strArea.Substring(0, strArea.IndexOf(";"));
                                strArea = strArea.Substring(strArea.IndexOf(";") + 1, strArea.Length - strArea.IndexOf(";") - 1);
                                string tempstrlat = tempstr.Substring(tempstr.IndexOf(",") + 1, tempstr.Length - tempstr.IndexOf(",") - 1);
                                string tempstrlon = tempstr.Substring(0, tempstr.IndexOf(","));
                                PointLatLng tempPoint = new PointLatLng(double.Parse(tempstrlat), double.Parse(tempstrlon));
                                tempPoint.Lat = tempPoint.Lat;
                                tempPoint.Lng = tempPoint.Lng;
                                tempPoint = EvilTransform.transform(tempPoint.Lat, tempPoint.Lng);
                                cityAreaPoint.Add(tempPoint);
                            }

                            GMapPolygon r = new GMapPolygon(cityAreaPoint, NextFile.Name);
                            r.StrokeThickness = 1;
                            switch (cityAreaPoint.Count % 4)
                            {
                                case 0:
                                    r.Fill = Brushes.LawnGreen;
                                    break;
                                case 1:
                                    r.Fill = Brushes.LightGoldenrodYellow;
                                    break;
                                case 2:
                                    r.Fill = Brushes.OrangeRed;
                                    break;
                                case 3:
                                    r.Fill = Brushes.Blue;
                                    break;
                                default:
                                    break;
                            }
                            TrackingLayer.Markers.Add(r);
                            // MarkerOverLayer.Areas.Add(r);//将道路加入图层

                        }
                    }
                }
            }
            catch
            {
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.SecondaryCache = ch;

            // set your proxy here if need
            //GMapProvider.IsSocksProxy = true;
            //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
            //GMapProvider.WebProxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");
            // or
            //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
            //

            // set cache mode only if no internet avaible
            //if (!Stuff.PingNetwork("pingtest.com"))
            //{
            //    MainMap.Manager.Mode = AccessMode.CacheOnly;
            //    MessageBox.Show("No internet connection available, going to CacheOnly mode.", "GMap.NET - Demo.WindowsPresentation", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}

            // config map
            MainMap.MapProvider = GMapProviders.PgisGDDH;
            MainMap.Position = new PointLatLng(30.6961334816182, 103.2985095977783);

            //MainMap.ScaleMode = ScaleModes.Dynamic;

            // map events
            MainMap.OnPositionChanged += new PositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);
            MainMap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            MainMap.DragButton = MouseButton.Left;
            MainMap.OpenDragDelay = true;
            MarkerOverLayer = new GmapGpsOverlay("test");
            MarkerOverLayer.OnMarkerCountChange = onMarkerCountChanged;
            MainMap.Overlays.Add(MarkerOverLayer);
            TrackingLayer = new GMapOverlay("TrackingLayer");
            MainMap.Overlays.Add(TrackingLayer);
            List<PointLatLng> lll = new List<PointLatLng>();

            // get map types
            comboBoxMapType.ItemsSource = GMapProviders.List;
            comboBoxMapType.DisplayMemberPath = "Name";
            comboBoxMapType.SelectedItem = MainMap.MapProvider;

            // acccess mode
            comboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
            comboBoxMode.SelectedItem = MainMap.Manager.Mode;

            // get cache modes
            checkBoxCacheRoute.IsChecked = MainMap.Manager.UseRouteCache;
            checkBoxGeoCache.IsChecked = MainMap.Manager.UseGeocoderCache;

            // setup zoom min/max
            sliderZoom.Maximum = MainMap.MaxZoom;
            sliderZoom.Minimum = MainMap.MinZoom;

            // get position
            textBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);

            // get marker state
            checkBoxCurrentMarker.IsChecked = true;

            // can drag map
            checkBoxDragMap.IsChecked = MainMap.CanDragMap;


            //validator.Window = this;

            // set current marker
            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "custom position marker");
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                TrackingLayer.Markers.Add(currentMarker);
                // MainMap.Markers.Add(currentMarker);

            }

            //if(false)
            {
                // add my city location for demo
                GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

                PointLatLng? city = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius", out status);
                if (city != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    GMapMarker it = new GMapMarker(city.Value);
                    {
                        it.ZIndex = 55;
                        it.Shape = new CustomMarkerDemo(this, it, "Welcome to Lithuania! ;}");
                    }
                    MainMap.Markers.Add(it);

                    #region -- add some markers and zone around them --
                    //if(false)
                    {
                        List<PointAndInfo> objects = new List<PointAndInfo>();
                        {
                            string area = "Antakalnis";
                            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        {
                            string area = "Senamiestis";
                            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        {
                            string area = "Pilaite";
                            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        AddDemoZone(8.8, city.Value, objects);
                    }
                    #endregion
                }

                if (MainMap.Markers.Count > 1)
                {
                    //  MainMap.ZoomAndCenterMarkers(null);
                }
                DrawCityArea();
            }
            MainMap.Zoom = 10;
            // perfromance test
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += new EventHandler(timer_Tick);

            //moverPerformance test

            timer1.Interval = TimeSpan.FromMilliseconds(1);
            timer1.Tick += new EventHandler(timer_Tick1);

            // transport demo
            transport.DoWork += new DoWorkEventHandler(transport_DoWork);
            transport.ProgressChanged += new ProgressChangedEventHandler(transport_ProgressChanged);
            transport.WorkerSupportsCancellation = true;
            transport.WorkerReportsProgress = true;
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        #region -- performance test--
        public RenderTargetBitmap ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(obj.Width, obj.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(obj);

            if (bmp.CanFreeze)
            {
                bmp.Freeze();
            }

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;

            return bmp;
        }

        double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        Random r = new Random();

        int tt = 0;
        public System.Windows.Media.Imaging.BitmapImage testImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(
                System.Environment.CurrentDirectory + @"\red-dot.png", UriKind.RelativeOrAbsolute));

        // GLOBALMETHOD.ImageFromUrl(System.Environment.CurrentDirectory + @"\Images\mainMenu\比例尺-白.png");
        Thread myThread;
        void timer_Tick1(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;

            //if (myThread == null || (myThread != null && !myThread.IsAlive))
            //{
            //    myThread = new Thread(() =>
            //    {
            foreach (var item in MarkerOverLayer.Markers)
            {
                var pos = new PointLatLng(NextDouble(r, item.Position.Lat + 0.005, item.Position.Lat - 0.005), NextDouble(r, item.Position.Lng + 0.005, item.Position.Lng - 0.005));
                item.Position = pos;
                var ss = item.Shape as Test;
            //    ss.sp_Tip.Visibility = r.Next(100) % 2 == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //   item.Visilility = r.Next(100) % 2 == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                //item.ToGray(r.Next(100) % 2 == 0);

            }
            //    });
            //    myThread.IsBackground = true;
            //    myThread.Start();
            //}


            DateTime end = DateTime.Now;

            int delta = (int)(end - start).TotalMilliseconds;
            Debug.WriteLine("-------------------: " + delta + "ms");
            if (tt >= 100000)
            {
                timer.Stop();
                tt = 0;
            }
        }
        BitmapImage bbb;
        Image iii;
        void createImage()
        {
            iii = new Image();
            {
                RenderOptions.SetBitmapScalingMode(iii, BitmapScalingMode.LowQuality);
                iii.Stretch = Stretch.None;

                iii.MouseEnter += new System.Windows.Input.MouseEventHandler(image_MouseEnter);
                iii.MouseLeave += new System.Windows.Input.MouseEventHandler(image_MouseLeave);

                iii.Source = new BitmapImage(
                new Uri(
                    Directory.GetCurrentDirectory() + "/red-dot.png",
                    UriKind.RelativeOrAbsolute));
                bbb = new BitmapImage(
                new Uri(
                    Directory.GetCurrentDirectory() + "/red-dot.png",
                    UriKind.RelativeOrAbsolute));
                bbb.Freeze();
            }
        }

        protected void onMarkerCountChanged(GMapMarker gm, bool? IsAdd, int count)
        {

        }
        public partial class ccc : Canvas
        {
            Image img;
            public ccc()
            {
                img = new Image();
                this.Children.Add(img);
            }
            public void set(BitmapImage bb)
            {
                img.Source = bb;
            }
            protected override void OnRender(System.Windows.Media.DrawingContext dc)
            {
                //if (this.DataContext != null)
                //{
                //    var a = this.DataContext as GMapMarker;
                //    dc.DrawImage(a.imgSource, new System.Windows.Rect(0, 0, a.imgSource.Width, a.imgSource.Height));
                //}
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (iii == null) createImage();

           // Thread t = new Thread(() => {
                for (int i = 0; i < 1000; i++)
                {
                    // var pos = new PointLatLng(NextDouble(r, 30, 20), NextDouble(r, 100, 120));
                    var pos = new PointLatLng(NextDouble(r, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(r, MainMap.ViewArea.Right, MainMap.ViewArea.Left));
                    MyMarker m = new MyMarker(pos, (tt++).ToString());
                    {
                        //  var s = new Test((tt).ToString());
                        //  ccc s = new ccc();                    
                        //var image = new Image();
                        //{
                        //    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
                        //    image.Stretch = Stretch.None;
                        //    image.Opacity = s.Opacity;

                        //    image.MouseEnter += new System.Windows.Input.MouseEventHandler(image_MouseEnter);
                        //    image.MouseLeave += new System.Windows.Input.MouseEventHandler(image_MouseLeave);
                        //    image.Source = m.ToImageSource(s);
                        //}

                        //   RenderOptions.SetBitmapScalingMode(s, BitmapScalingMode.LowQuality);
                        //   s.MyBrush = bbb;
                        // s.IsHitTestVisible = false;
                        m.Name = i.ToString();
                        //    m.Shape = s;
                        m.imgSource = bbb;
                        //  s.set(bbb);
                        //   m.Offset = new System.Windows.Point(-s.Width, -s.Height);
                    }
                    //m.InitUI();
                    m.HasLife = false;
                    // lock (MarkerOverLayer.Markers)
                    MarkerOverLayer.Markers.Add(m);


                }
            
           // });
            //t.IsBackground = true;
            //t.Start();

            lb_MarkerCount.Content = tt.ToString();
            if (tt >= 99995)
            {
                timer.Stop();
                tt = 0;
            }

        }

        void image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.RenderTransform = null;
        }

        void image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.RenderTransform = new ScaleTransform(1.2, 1.2, 12.5, 12.5);
        }

        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer1 = new DispatcherTimer();
        #endregion

        #region -- transport demo --
        BackgroundWorker transport = new BackgroundWorker();

        readonly List<VehicleData> trolleybus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> trolleybusMarkers = new Dictionary<int, GMapMarker>();

        readonly List<VehicleData> bus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> busMarkers = new Dictionary<int, GMapMarker>();

        bool firstLoadTrasport = true;

        void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            using (Dispatcher.DisableProcessing())
            {
                lock (trolleybus)
                {
                    foreach (VehicleData d in trolleybus)
                    {
                        GMapMarker marker;

                        if (!trolleybusMarkers.TryGetValue(d.Id, out marker))
                        {
                            marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                            marker.Tag = d.Id;
                            marker.Shape = new CircleVisual(marker, Brushes.Red);

                            trolleybusMarkers[d.Id] = marker;
                            MainMap.Markers.Add(marker);
                        }
                        else
                        {
                            marker.Position = new PointLatLng(d.Lat, d.Lng);
                            var shape = (marker.Shape as CircleVisual);
                            {
                                shape.Text = d.Line;
                                shape.Angle = d.Bearing;
                                shape.Tooltip.SetValues("TrolleyBus", d);

                                if (shape.IsChanged)
                                {
                                    shape.UpdateVisual(false);
                                }
                            }
                        }
                    }
                }

                lock (bus)
                {
                    foreach (VehicleData d in bus)
                    {
                        GMapMarker marker;

                        if (!busMarkers.TryGetValue(d.Id, out marker))
                        {
                            marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                            marker.Tag = d.Id;

                            var v = new CircleVisual(marker, Brushes.Blue);
                            {
                                v.Stroke = new Pen(Brushes.Gray, 2.0);
                            }
                            marker.Shape = v;

                            busMarkers[d.Id] = marker;
                            MainMap.Markers.Add(marker);
                        }
                        else
                        {
                            marker.Position = new PointLatLng(d.Lat, d.Lng);
                            var shape = (marker.Shape as CircleVisual);
                            {
                                shape.Text = d.Line;
                                shape.Angle = d.Bearing;
                                shape.Tooltip.SetValues("Bus", d);

                                if (shape.IsChanged)
                                {
                                    shape.UpdateVisual(false);
                                }
                            }
                        }
                    }
                }

                if (firstLoadTrasport)
                {
                    firstLoadTrasport = false;
                }
            }
        }

        void transport_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!transport.CancellationPending)
            {
                try
                {
                    lock (trolleybus)
                    {
                        Stuff.GetVilniusTransportData(Demo.WindowsForms.TransportType.TrolleyBus, string.Empty, trolleybus);
                    }

                    lock (bus)
                    {
                        Stuff.GetVilniusTransportData(Demo.WindowsForms.TransportType.Bus, string.Empty, bus);
                    }

                    transport.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("transport_DoWork: " + ex.ToString());
                }
                Thread.Sleep(3333);
            }
            trolleybusMarkers.Clear();
            busMarkers.Clear();
        }

        #endregion

        // add objects and zone around them
        void AddDemoZone(double areaRadius, PointLatLng center, List<PointAndInfo> objects)
        {
            var objectsInArea = from p in objects
                                where MainMap.MapProvider.Projection.GetDistance(center, p.Point) <= areaRadius
                                select new
                                {
                                    Obj = p,
                                    Dist = MainMap.MapProvider.Projection.GetDistance(center, p.Point)
                                };
            if (objectsInArea.Any())
            {
                var maxDistObject = (from p in objectsInArea
                                     orderby p.Dist descending
                                     select p).First();

                // add objects to zone
                foreach (var o in objectsInArea)
                {
                    GMapMarker it = new GMapMarker(o.Obj.Point);
                    {
                        it.ZIndex = 55;
                        var s = new CustomMarkerDemo(this, it, o.Obj.Info + ", distance from center: " + o.Dist + "km.");
                        it.Shape = s;
                    }

                    MainMap.Markers.Add(it);
                }

                // add zone circle
                //if(false)
                {
                    GMapMarker it = new GMapMarker(center);
                    it.ZIndex = -1;

                    Circle c = new Circle();
                    c.Center = center;
                    c.Bound = maxDistObject.Obj.Point;
                    c.Tag = it;
                    c.IsHitTestVisible = false;

                    UpdateCircle(c);
                    Circles.Add(it);

                    it.Shape = c;
                    MainMap.Markers.Add(it);
                }
            }
        }

        // calculates circle radius
        void UpdateCircle(Circle c)
        {
            var pxCenter = MainMap.FromLatLngToLocal(c.Center);
            var pxBounds = MainMap.FromLatLngToLocal(c.Bound);

            double a = (double)(pxBounds.X - pxCenter.X);
            double b = (double)(pxBounds.Y - pxCenter.Y);
            var pxCircleRadius = Math.Sqrt(a * a + b * b);

            c.Width = 55 + pxCircleRadius * 2;
            c.Height = 55 + pxCircleRadius * 2;
            (c.Tag as GMapMarker).Offset = new System.Windows.Point(-c.Width / 2, -c.Height / 2);
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            sliderZoom.Minimum = MainMap.MinZoom;
            sliderZoom.Maximum = MainMap.MaxZoom;
        }

        void MainMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(MainMap);
           // currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(MainMap);
                currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        // zoo max & center markers
        private void button13_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ZoomAndCenterMarkers(null);

            /*
            PointAnimation panMap = new PointAnimation();
            panMap.Duration = TimeSpan.FromSeconds(1);
            panMap.From = new Point(MainMap.Position.Lat, MainMap.Position.Lng);
            panMap.To = new Point(0, 0);
            Storyboard.SetTarget(panMap, MainMap);
            Storyboard.SetTargetProperty(panMap, new PropertyPath(GMapControl.MapPointProperty));

            Storyboard panMapStoryBoard = new Storyboard();
            panMapStoryBoard.Children.Add(panMap);
            panMapStoryBoard.Begin(this);
             */
        }

        // tile louading starts
        void MainMap_OnTileLoadStart()
        {
            System.Windows.Forms.MethodInvoker m = delegate()
            {
                progressBar1.Visibility = Visibility.Visible;
            };

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch
            {
            }
        }

        // tile loading stops
        void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            MainMap.ElapsedMilliseconds = ElapsedMilliseconds;

            System.Windows.Forms.MethodInvoker m = delegate()
            {
                progressBar1.Visibility = Visibility.Hidden;
                groupBox3.Header = "loading, last in " + MainMap.ElapsedMilliseconds + "ms";
            };

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch
            {
            }
        }

        // current location changed
        void MainMap_OnCurrentPositionChanged(PointLatLng point)
        {
            mapgroup.Header = "gmap: " + point;
        }

        // reload
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ReloadMap();
        }

        // enable current marker
        private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Add(currentMarker);
            }
        }

        // disable current marker
        private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Remove(currentMarker);
            }
        }

        // enable map dragging
        private void checkBoxDragMap_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.OpenDragDelay = true;
        }

        // disable map dragging
        private void checkBoxDragMap_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.OpenDragDelay = false;
        }

        // goto!
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double lat = double.Parse(textBoxLat.Text, CultureInfo.InvariantCulture);
                double lng = double.Parse(textBoxLng.Text, CultureInfo.InvariantCulture);

                MainMap.Position = new PointLatLng(lat, lng);
            }
            catch (Exception ex)
            {
                MessageBox.Show("incorrect coordinate format: " + ex.Message);
            }
        }

        // goto by geocoder
        private void textBoxGeo_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GeoCoderStatusCode status = MainMap.SetPositionByKeywords(textBoxGeo.Text);
                if (status != GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    currentMarker.Position = MainMap.Position;
                }
            }
        }

        // zoom changed
        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // updates circles on map
            foreach (var c in Circles)
            {
                UpdateCircle(c.Shape as Circle);
            }
        }

        // zoom up
        private void czuZoomUp_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)MainMap.Zoom) + 1;
        }

        // zoom down
        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
        }

        // prefetch
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            RectLatLng area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
                {
                    MessageBoxResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButton.YesNoCancel);

                    if (res == MessageBoxResult.Yes)
                    {
                        TilePrefetcher obj = new TilePrefetcher();
                        obj.Owner = this;
                        obj.ShowCompleteMessage = true;
                        obj.Start(area, i, MainMap.MapProvider, 100);
                    }
                    else if (res == MessageBoxResult.No)
                    {
                        continue;
                    }
                    else if (res == MessageBoxResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // access mode
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)comboBoxMode.SelectedItem;
            MainMap.ReloadMap();
        }

        // clear cache
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    MainMap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                    MessageBox.Show("Done. Cache is clear.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // export
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowExportDialog();
        }

        // import
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowImportDialog();
        }

        // use route cache
        private void checkBoxCacheRoute_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseRouteCache = checkBoxCacheRoute.IsChecked.Value;
        }

        // use geocoding cahce
        private void checkBoxGeoCache_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseGeocoderCache = checkBoxGeoCache.IsChecked.Value;
            MainMap.Manager.UsePlacemarkCache = MainMap.Manager.UseGeocoderCache;
        }

        // save currnt view
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImageSource img = MainMap.ToImageSource();
                PngBitmapEncoder en = new PngBitmapEncoder();
                en.Frames.Add(BitmapFrame.Create(img as BitmapSource));

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "GMap.NET Image"; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "Image (.png)|*.png"; // Filter files by extension
                dlg.AddExtension = true;
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;

                    using (System.IO.Stream st = System.IO.File.OpenWrite(filename))
                    {
                        en.Save(st);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // clear all markers
        private void button10_Click(object sender, RoutedEventArgs e)
        {
            var clear = MainMap.Markers.Where(p => p != null && p != currentMarker);
            if (clear != null)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    MainMap.Markers.Remove(clear.ElementAt(i));
                    i--;
                }
            }

            if (radioButtonPerformance.IsChecked == true)
            {
                tt = 0;
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            }
        }

        // add marker
        private void button8_Click(object sender, RoutedEventArgs e)
        {
            GMapMarker m = new GMapMarker(currentMarker.Position);
            {
                Placemark? p = null;
                if (checkBoxPlace.IsChecked.Value)
                {
                    GeoCoderStatusCode status;
                    var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && plret != null)
                    {
                        p = plret;
                    }
                }

                string ToolTipText;
                if (p != null)
                {
                    ToolTipText = p.Value.Address;
                }
                else
                {
                    ToolTipText = currentMarker.Position.ToString();
                }

                m.Shape = new CustomMarkerDemo(this, m, ToolTipText);
                m.ZIndex = 55;
            }
            MainMap.Markers.Add(m);
        }

        // sets route start
        private void button11_Click(object sender, RoutedEventArgs e)
        {
            start = currentMarker.Position;
        }

        // sets route end
        private void button9_Click(object sender, RoutedEventArgs e)
        {
            end = currentMarker.Position;
        }

        // adds route
        private void button12_Click(object sender, RoutedEventArgs e)
        {
            RoutingProvider rp = MainMap.MapProvider as RoutingProvider;
            if (rp == null)
            {
                rp = GMapProviders.OpenStreetMap; // use OpenStreetMap if provider does not implement routing
            }

            MapRoute route = rp.GetRoute(start, end, false, false, (int)MainMap.Zoom);
            if (route != null)
            {
                GMapMarker m1 = new GMapMarker(start);
                m1.Shape = new CustomMarkerDemo(this, m1, "Start: " + route.Name);

                GMapMarker m2 = new GMapMarker(end);
                m2.Shape = new CustomMarkerDemo(this, m2, "End: " + start.ToString());

                List<PointLatLng> EvPoints = new List<PointLatLng>();
                foreach (var item in route.Points)
                {
                    EvPoints.Add(GMap.NET.Internals.EvilTransform.transform(item.Lat, item.Lng));
                }
                GMapRoute mRoute = new GMapRoute(EvPoints);
                {
                    mRoute.ZIndex = -1;
                }

                MainMap.Markers.Add(m1);
                MainMap.Markers.Add(m2);
                MainMap.Markers.Add(mRoute);

                MainMap.ZoomAndCenterMarkers(null);
            }
        }

        // enables tile grid view
        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = true;
        }

        // disables tile grid view
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = false;
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int offset = 22;

            if (MainMap.IsFocused)
            {
                if (e.Key == Key.Left)
                {
                    MainMap.Offset(-offset, 0);
                }
                else if (e.Key == Key.Right)
                {
                    MainMap.Offset(offset, 0);
                }
                else if (e.Key == Key.Up)
                {
                    MainMap.Offset(0, -offset);
                }
                else if (e.Key == Key.Down)
                {
                    MainMap.Offset(0, offset);
                }
                else if (e.Key == Key.Add)
                {
                    czuZoomUp_Click(null, null);
                }
                else if (e.Key == Key.Subtract)
                {
                    czuZoomDown_Click(null, null);
                }
            }
        }

        // set real time demo
        private void realTimeChanged(object sender, RoutedEventArgs e)
        {
            if (MarkerOverLayer != null)
                MarkerOverLayer.Markers.Clear();

            // start performance test
            if (radioButtonPerformance.IsChecked == true)
            {
                timer.Start();
            }
            else
            {
                // stop performance test
                timer.Stop();
            }

            // start realtime transport tracking demo
            if (radioButtonTransport.IsChecked == true)
            {
                if (!transport.IsBusy)
                {
                    firstLoadTrasport = true;
                    transport.RunWorkerAsync();
                }
            }
            else
            {
                if (transport.IsBusy)
                {
                    transport.CancelAsync();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                MainMap.Bearing--;
            }
            else if (e.Key == Key.Z)
            {
                MainMap.Bearing++;
            }
        }

        private void MovePerformance_Checked(object sender, System.Windows.RoutedEventArgs e)
        {


            if (MovePerformance.IsChecked == true)
            {
                timer1.Start();
            }
            else
            {
                // stop performance test
                timer1.Stop();
            }

        }
    }

    public class MapValidationRule : ValidationRule
    {
        bool UserAcceptedLicenseOnce = false;
        internal MainWindow Window;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is OpenStreetMapProviderBase))
            {
                if (!UserAcceptedLicenseOnce)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt"))
                    {
                        string ctn = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt");
                        int li = ctn.IndexOf("License");
                        string txt = ctn.Substring(li);

                        var d = new Demo.WindowsPresentation.Windows.Message();
                        d.richTextBox1.Text = txt;

                        if (true == d.ShowDialog())
                        {
                            UserAcceptedLicenseOnce = true;
                            if (Window != null)
                            {
                                Window.Title += " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        // user deleted License.txt ;}
                        UserAcceptedLicenseOnce = true;
                    }
                }

                if (!UserAcceptedLicenseOnce)
                {
                    return new ValidationResult(false, "user do not accepted license ;/");
                }
            }

            return new ValidationResult(true, null);
        }
    }
}
