namespace GMap.NET.WindowsPresentation
{
    using GMap.NET.Internals;
    using GMap.NET.ObjectModel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Smart 图层，用于大量图标静态图标添加，只加载当前区域内图标
    /// </summary>
    public class GmapGpsOverlay : GMapOverlay
    {

        /// <summary>
        /// 缩略图类
        /// </summary>
        public class RenderCanvas : Canvas
        {
            /// <summary>
            /// 可视区域内Marker
            /// </summary>
            public List<GMapMarker> ShowMarkers = new List<GMapMarker>();
            public List<GMapMarker> ShowMarkersExpand = new List<GMapMarker>();
            public GmapGpsOverlay Owner;
            public RenderCanvas(List<GMapMarker> ShowM, List<GMapMarker> ShowExpand, GmapGpsOverlay O)
            {
                ShowMarkers = ShowM;
                ShowMarkersExpand = ShowExpand;
                EmptyTileBorders.Freeze();
                Owner = O;
            }
            /// <summary>
            /// pen for empty tile borders
            /// </summary>
            public Pen EmptyTileBorders = new Pen(Brushes.Green, 5.0);


            protected override void OnRender(DrawingContext dc)
            {
                TimeSpan tick = new TimeSpan();
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                tick = sw.Elapsed;
                try
                {
                    foreach (var item in ShowMarkers)
                    {
                        if (item.imgSource != null && item.Visilility == System.Windows.Visibility.Visible)
                            if (ShowMarkers.Count < 10000)
                            {
                                if (Owner.ShowMarkerReal == null)
                                    dc.DrawImage(item.imgSource, new Rect(item.LocalPositionX - item.Offset.X - item.imgSource.Width / 4,
                                        item.LocalPositionY - item.Offset.Y - item.imgSource.Height / 2, item.imgSource.Width / 2, item.imgSource.Height / 2));
                                else
                                    dc.DrawImage(item.imgSource, new Rect(item.LocalPositionX - item.Offset.X - item.imgSource.Width / 2,
                                        item.LocalPositionY - item.Offset.Y - item.imgSource.Height, item.imgSource.Width, item.imgSource.Height));
                            }
                            else
                                dc.DrawLine(EmptyTileBorders, new Point(item.LocalPositionX, item.LocalPositionY), new Point(item.LocalPositionX, item.LocalPositionY + 5));

                    }
                }
                catch { }
                tick = sw.Elapsed - tick;
                System.Diagnostics.Trace.WriteLine("render" + ":" + tick.Seconds.ToString() + "秒" + tick.TotalMilliseconds.ToString());
            }
        }


        public GmapGpsOverlay(string id)
        {
            Id = id;
            MainThreadID = Thread.CurrentThread.ManagedThreadId;
            ThreadUpdate = new Thread(ThreadUpDateBound);
            ThreadUpdate.IsBackground = true;
            ThreadUpdate.Start();
        }
        /// <summary>
        /// 缩略图
        /// </summary>
        RenderCanvas PreViewCanvas;

        ItemsControl AreaCanvas = new ItemsControl();

        /// <summary>
        /// 内部线程，统计区域数量，检查生命周期
        /// </summary>
        Thread mThread;
        Thread markerInitThread;
        Thread UpdateThead;
        int MainThreadID = -1;
        static UserControl[] RealUI = new UserControl[200];



        /// <summary>
        /// 初始化图层
        /// </summary>
        protected override void init()
        {
            CreateTemplate(MarkerCanvas, null);
            //CreateTemplate(AreaCanvas, Areas);

            PreViewCanvas = new RenderCanvas(ShowList, ShowListExpand, this);
            // this.Children.Add(AreaCanvas);
            this.Children.Add(MarkerCanvas);
            this.Children.Add(PreViewCanvas);
            PreViewCanvas.Visibility = System.Windows.Visibility.Collapsed;
            //mThread = new Thread(checkLife);
            //mThread.IsBackground = true;
            //mThread.Start();

        }
        /// <summary>
        /// 更新图层
        /// </summary>
        public override void UpdateBounds()
        {
            if (markerInitThread != null && markerInitThread.IsAlive)
                markerInitThread.Abort();
            if (MarkerCanvas.Visibility == System.Windows.Visibility.Visible)
            {
                markerInitThread = new Thread(ThreadInitMarker);
                markerInitThread.Name = "markerInitThread";
                markerInitThread.IsBackground = true;
                markerInitThread.Start();
            }
            if (Thread.CurrentThread.ManagedThreadId == MainThreadID)
            {
                PreViewCanvas.InvalidateVisual();
            }
            else if (Control != null)
            {
                Control.Dispatcher.Invoke(new Action(() =>
                {
                    PreViewCanvas.InvalidateVisual();
                }));
            }
        }
        /// <summary>
        /// 生命周期检测
        /// </summary>
        void checkLife()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (Control != null)
                    Control.Dispatcher.Invoke(new Action(
                                    () =>
                                    {
                                        AllList = Markers.ToList();
                                    }

                                    ), System.Windows.Threading.DispatcherPriority.SystemIdle);
                foreach (var item in AllList)
                {
                    if (item.Life == -1)
                    {
                        Control.Dispatcher.Invoke(new Action(
                            () =>
                            {
                                Markers.Remove(item);
                            }
                                ), System.Windows.Threading.DispatcherPriority.SystemIdle);
                    }
                    else if (item.LifeChanging)
                    {
                        Control.Dispatcher.Invoke(new Action(
                           () =>
                           {
                               item.ToGray(item.Life == 0);
                               onMarkerCountChanged(item, null);
                           }
                               ), System.Windows.Threading.DispatcherPriority.SystemIdle);
                    }

                }
            }
        }

        #region 只添加可视区域图标

        RectLatLng ViewArea;

        /// <summary>
        /// 可视区域内Marker
        /// </summary>
        ObservableCollectionThreadSafe<GMapMarker> ShowMarkers = new ObservableCollectionThreadSafe<GMapMarker>();

        List<GMapMarker> ShowList = new List<GMapMarker>();
        List<GMapMarker> ShowListExpand = new List<GMapMarker>();
        List<GMapMarker> AllList = new List<GMapMarker>();

        /// <summary>
        /// 初始化图标的图形
        /// </summary>
        void ThreadInitMarker()
        {
            if (Control == null) return;
            int step = 10;
            List<GMapMarker> iniList = new List<GMapMarker>();
            foreach (var item in ShowList)
            {
                if (item.Shape == null)
                    iniList.Add(item);
            }
            for (int i = 0; i <= iniList.Count / step; i++)
                Control.Dispatcher.Invoke(new Action(
                                 () =>
                                 {
                                     for (int j = 0; (j < step) && (j + i * step < iniList.Count); j++)
                                     {
                                         if (iniList[j + i * step].Shape == null)
                                         {
                                             iniList[j + i * step].InitUI();
                                         }
                                     }
                                 }
                                     ), System.Windows.Threading.DispatcherPriority.SystemIdle);
        }

        Thread ThreadUpdate;
        void ThreadUpDateBound()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (Control == null) continue;
                if (Control.IsDragging) continue;

                Control.Dispatcher.Invoke(new Action(() =>
                {
                    ShowList.Clear();
                    ShowListExpand.Clear();
                    AllList = Markers.ToList();
                }));


                RectLatLng ViewAreaExpand = new RectLatLng(Control.ViewArea.Top + Control.ViewArea.HeightLat,
                    Control.ViewArea.Left - Control.ViewArea.WidthLng, Control.ViewArea.WidthLng * 3, Control.ViewArea.HeightLat * 3);

                foreach (var item in AllList)
                {
                    if (ViewAreaExpand.Contains(item.Position.Lat, item.Position.Lng))
                    {
                        if (Control.ViewArea.Contains(item.Position.Lat, item.Position.Lng))
                            ShowList.Add(item);
                        ShowListExpand.Add(item);
                    }
                }
                Control.Dispatcher.Invoke(new Action(() =>
                {
                    if (Control.Zoom < MarkerShowRealZoomSize)
                    {
                        MarkerCanvas.Visibility = System.Windows.Visibility.Collapsed;
                        PreViewCanvas.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        MarkerCanvas.Visibility = System.Windows.Visibility.Visible;
                        PreViewCanvas.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }));
                UpdateBounds();
            }
        }


        #endregion
        #region 自定义事件

        int MarkerShowZoomSize = 15;
        int MarkerShowRealZoomSize = 18;

        public bool? ShowMarkerReal
        {
            get
            {

                if (Control == null) return null;
                if (Control.Zoom < MarkerShowZoomSize) return null;
                if (Control.Zoom >= MarkerShowRealZoomSize) return true;
                return false;
            }
        }
        public Action<GMapMarker, System.Windows.Input.MouseEventArgs> OnMarkerClick;
        public Action<GMapMarker> OnMarkerEnter;
        public Action<GMapMarker> OnMarkerLeave;
        void _OnMarkerMouseEnter(GMapMarker mk)
        {
            if (OnMarkerEnter != null && ShowMarkerReal != null)
                OnMarkerEnter(mk);
        }
        void _OnMarkerMouseLeave(GMapMarker mk)
        {
            if (OnMarkerLeave != null)
                OnMarkerLeave(mk);
        }
        void _OnMarkerMouseClick(GMapMarker mk, System.Windows.Input.MouseEventArgs e)
        {
            if (OnMarkerClick != null && ShowMarkerReal != null)
                OnMarkerClick(mk, e);
        }

        GMapMarker currentEnterMarker = null;
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            GMapMarker mk = null;
            try
            {
                foreach (var item in ShowList)
                {
                    if (Math.Abs(p.X - item.LocalPositionX + item.Offset.X / 2 - 25) < 16 && Math.Abs(p.Y - item.LocalPositionY + item.Offset.Y / 2 - 17) < 30)
                    {
                        mk = item;
                        break;
                    }
                }
                if (currentEnterMarker != null)
                {
                    if (currentEnterMarker != mk)
                    {
                        _OnMarkerMouseLeave(currentEnterMarker);
                        currentEnterMarker = null;
                    }
                }
                if (mk != null)
                {
                    _OnMarkerMouseEnter(mk);
                }
            }
            catch { }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (currentEnterMarker != null)
            {
                _OnMarkerMouseLeave(currentEnterMarker);
                currentEnterMarker = null;
            }
        }
        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            try
            {
                foreach (var item in ShowList)
                {
                    if (Math.Abs(p.X - item.LocalPositionX + item.Offset.X / 2 - 25) < 16 && Math.Abs(p.Y - item.LocalPositionY + item.Offset.Y / 2 - 17) < 30)
                    {
                        _OnMarkerMouseClick(item, e);
                        break;
                    }
                }
            }
            catch { }
        }
        #endregion
        /// <summary>
        /// marker添加删除事件绑定
        /// </summary>
        protected override void CreateEvents()
        {
            Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
        }

        /// <summary>
        /// marker添加删除事件绑定
        /// </summary>
        protected override void ClearEvents()
        {
            Markers.CollectionChanged -= new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
        }

        void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GMapMarker obj in e.OldItems)
                {
                    if (obj != null)
                    {
                        obj.Overlay = null;
                        onMarkerCountChanged(obj, false);
                        MarkerCanvas.Items.Remove(obj);
                    }
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                MarkerCanvas.Items.Clear();
            }
            if (e.NewItems != null)
            {
                foreach (GMapMarker obj in e.NewItems)
                {
                    if (obj != null)
                    {
                        obj.Overlay = this;
                        onMarkerCountChanged(obj, true);
                        OnMarkerPositionChange(obj);
                        if (obj != null)
                        {
                            obj.ForceUpdateLocalPosition(Control);
                        }
                        MarkerCanvas.Items.Add(obj);
                    }
                }
            }


        }


        //void Areas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        foreach (GMapPolygon obj in e.NewItems)
        //        {
        //            if (obj != null)
        //            {
        //                obj.Overlay = this;

        //                if (obj != null)
        //                {
        //                    obj.ForceUpdateLocalPosition(Control);

        //                    if (obj is IShapable)
        //                    {
        //                        (obj as IShapable).RegenerateShape(Control);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
