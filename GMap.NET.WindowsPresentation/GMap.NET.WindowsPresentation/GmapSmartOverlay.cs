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
    public class GmapSmartOverlay : GMapOverlay
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
            GmapSmartOverlay Owner;
            public RenderCanvas(List<GMapMarker> ShowM, GmapSmartOverlay O)
            {
                Owner = O;
                ShowMarkers = ShowM;
                EmptyTileBorders.Freeze();
            }
            /// <summary>
            /// pen for empty tile borders
            /// </summary>
            public Pen EmptyTileBorders = new Pen(Brushes.LightBlue, 5.0);


            protected override void OnRender(DrawingContext dc)
            {
                if (Owner.ShowMarkerReal == false)
                    foreach (var item in ShowMarkers)
                    {
                        dc.DrawLine(EmptyTileBorders, new Point(item.LocalPositionX, item.LocalPositionY), new Point(item.LocalPositionX, item.LocalPositionY + 5));
                    }
                else if (Owner.ShowMarkerReal == true)
                    foreach (var item in ShowMarkers)
                    {
                        if (item.imgSource != null)
                            dc.DrawImage(item.imgSource, new Rect(item.LocalPositionX - item.Offset.X - item.imgSource.Width / 2, item.LocalPositionY - item.Offset.Y - item.imgSource.Height / 2, item.imgSource.Width, item.imgSource.Height));
                    }

            }
        }

        public int MarkerShowZoomSize = 16;

        public int MarkerShowRealZoomSize = 18;

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

        public GmapSmartOverlay(string id)
        {
            Id = id;

        }
        void _OnMarkerMouseEnter(GMapMarker mk)
        {
            if (OnMarkerEnter != null && ShowMarkerReal == true)
                OnMarkerEnter(mk);
        }

        void _OnMarkerMouseLeave(GMapMarker mk)
        {
            if (OnMarkerLeave != null)
                OnMarkerLeave(mk);
        }
        void _OnMarkerMouseClick(GMapMarker mk)
        {
            if (OnMarkerClick != null && ShowMarkerReal == true)
                OnMarkerClick(mk);
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
                    if (Math.Abs(p.X - item.LocalPositionX) < 35 && Math.Abs(p.Y - item.LocalPositionY) < 35)
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
                    if (Math.Abs(p.X - item.LocalPositionX) < 30 && Math.Abs(p.Y - item.LocalPositionY) < 30)
                    {
                        _OnMarkerMouseClick(item);
                        break;
                    }
                }
            }
            catch { }
        }

        ItemsControl AreaCanvas = new ItemsControl();
        /// <summary>
        /// 缩略图
        /// </summary>
        RenderCanvas PreViewCanvas;

        /// <summary>
        /// 初始化图层
        /// </summary>
        protected override void init()
        {

            PreViewCanvas = new RenderCanvas(ShowList, this);

            this.Children.Add(PreViewCanvas);
        }

        public Action<GMapMarker> OnMarkerClick;
        public Action<GMapMarker> OnMarkerEnter;
        public Action<GMapMarker> OnMarkerLeave;



        /// <summary>
        /// 更新图层
        /// </summary>
        public override void UpdateBounds()
        {
            GetShowList();
            PreViewCanvas.InvalidateVisual();
        }

        double prevFoceUpdateMapZoom = -1;

        /// <summary>
        /// 智能刷新图层中元素图标坐标
        /// </summary>
        public void FoceUpdateChildren()
        {
            if (Control == null) return;
            if (prevFoceUpdateMapZoom == Control.Zoom) return;
            if (Visibility == Visibility.Collapsed) return;
            if (ShowMarkerReal == null) return;
            prevFoceUpdateMapZoom = Control.Zoom;
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //TimeSpan tick = new TimeSpan();
            //sw.Start();
            //tick = sw.Elapsed;
            long xx = -(long)Control.MapTranslateTransform.X;
            long yy = -(long)Control.MapTranslateTransform.Y;
            GPoint p = new GPoint(50, 50);
            try
            {
                var temp = Markers.ToList();
                
                foreach (var item in temp)
                {
                    if (item == null) continue;
                    p = Control.FromLatLngToLocal(item.Position);
                    p.X = p.X + xx;
                    p.Y = p.Y + yy;

                    item.LocalPositionX = (int)(p.X + (long)(item.Offset.X));
                    item.LocalPositionY = (int)(p.Y + (long)(item.Offset.Y));
                }
            }
            catch
            {

            }
        }

        #region 只添加可视区域图标

        List<GMapMarker> ShowList = new List<GMapMarker>();
        List<GMapMarker> AllList = new List<GMapMarker>();
        /// <summary>
        /// 获取区域内图标
        /// </summary>
        void GetShowList()
        {
            ShowList.Clear();
            if (Control == null || Visibility != Visibility.Visible)
                return;

            try
            {
                if (Control.Zoom < MarkerShowZoomSize)
                {
                    PreViewCanvas.Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (Control.Zoom < MarkerShowRealZoomSize)
                {
                    PreViewCanvas.Visibility = System.Windows.Visibility.Visible;
                }
                if (ShowMarkerReal == null)
                    return;

                lock (Markers)
                    AllList = Markers.ToList();

                RectLatLng ViewAreaExpand = new RectLatLng(Control.ViewArea.Top + Control.ViewArea.HeightLat/2,
                    Control.ViewArea.Left - Control.ViewArea.WidthLng/2, Control.ViewArea.WidthLng * 2, Control.ViewArea.HeightLat * 2);

                foreach (var item in AllList)
                {
                    if (item != null)
                    {
                        if (ViewAreaExpand.Contains(item.Position.Lat, item.Position.Lng))
                        {
                            ShowList.Add(item);
                        }
                    }
                }

            }
            catch
            {
            }
        }
        #endregion

        /// <summary>
        /// marker添加删除事件绑定
        /// </summary
        protected override void CreateEvents()
        {
            Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
        }

        /// <summary>
        /// marker添加删除事件绑定
        /// </summary
        protected override void ClearEvents()
        {
            Markers.CollectionChanged -= new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
        }

        void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems != null)
            {
                foreach (GMapMarker obj in e.NewItems)
                {
                    if (obj != null)
                    {
                        obj.Overlay = this;
                    }
                }
            }


        }
    }
}
