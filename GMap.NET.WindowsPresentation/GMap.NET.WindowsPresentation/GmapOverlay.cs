
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

    public class GMapOverlay : Canvas, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        int zIndex;

        /// <summary>
        /// the index of Z, render order
        /// </summary>
        public int ZIndex
        {
            get
            {
                return zIndex;
            }
            set
            {
                if (zIndex != value)
                {
                    zIndex = value;
                    OnPropertyChanged("ZIndex");
                }
            }
        }

        /// <summary>
        /// overlay Id
        /// </summary>
        public string Id;

        /// <summary>
        /// list of markers, should be thread safe
        /// </summary>
        public ObservableCollectionThreadSafe<GMapMarker> Markers = new ObservableCollectionThreadSafe<GMapMarker>();


        /// <summary>
        /// 图层图标数改变，图标,添加，总数
        /// </summary>
        public Action<GMapMarker, bool?, int> OnMarkerCountChange;

        /// <summary>
        /// 图标数量改变
        /// </summary>
        /// <param name="gm">图标</param>
        /// <param name="IsAdd">true 添加，false删除,null 变灰</param>
        protected void onMarkerCountChanged(GMapMarker gm, bool? IsAdd)
        {
            if (Control == null) return;
            if (OnMarkerCountChange == null) return;
            Control.Dispatcher.BeginInvoke(new Action(
                          () =>
                          {
                              OnMarkerCountChange(gm, IsAdd, Markers.Count);
                          }
                              ), System.Windows.Threading.DispatcherPriority.SystemIdle);
        }

        /// <summary>
        /// 当前图层显示的是否是缩略图
        /// </summary>
        public bool IsPreView
        {
            get
            {
                return MarkerCanvas.Visibility != System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// 图标坐标改变
        /// </summary>
        /// <param name="marker"></param>
        public virtual void OnMarkerPositionChange(GMapMarker marker)
        {
        }


        #region Create Template

        protected static DataTemplate DataTemplateInstance;
        protected static ItemsPanelTemplate ItemsPanelTemplateInstance;
        protected static Style StyleInstance;

        protected ItemsControl MarkerCanvas = new ItemsControl();


        /// <summary>
        /// 创建图层
        /// </summary>
        /// <param name="itemControl">Canvas图层</param>
        /// <param name="source">Canvas绑定数据源</param>
        protected void CreateTemplate(ItemsControl itemControl, IEnumerable source)
        {
            if (DataTemplateInstance == null)
            {
                DataTemplateInstance = new DataTemplate(typeof(GMapMarker));
                {
                    FrameworkElementFactory fef = new FrameworkElementFactory(typeof(ContentPresenter));
                    fef.SetBinding(ContentPresenter.ContentProperty, new Binding("Shape"));
                    DataTemplateInstance.VisualTree = fef;
                }
            }
            itemControl.ItemTemplate = DataTemplateInstance;

            if (ItemsPanelTemplateInstance == null)
            {
                var factoryPanel = new FrameworkElementFactory(typeof(Canvas));
                {
                    factoryPanel.SetValue(Canvas.IsItemsHostProperty, true);

                    factoryPanel.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                    ItemsPanelTemplateInstance = new ItemsPanelTemplate();
                    {
                        ItemsPanelTemplateInstance.VisualTree = factoryPanel;
                    }
                }
            }
            itemControl.ItemsPanel = ItemsPanelTemplateInstance;

            if (StyleInstance == null)
            {
                StyleInstance = new Style();
                {
                    StyleInstance.Setters.Add(new Setter(Canvas.LeftProperty, new Binding("LocalPositionX")));
                    StyleInstance.Setters.Add(new Setter(Canvas.TopProperty, new Binding("LocalPositionY")));
                    StyleInstance.Setters.Add(new Setter(Canvas.ZIndexProperty, new Binding("ZIndex")));
                }
            }
            itemControl.ItemContainerStyle = StyleInstance;

            if (itemControl.ItemsSource == null)
            {
                itemControl.ItemsSource = source;
            }
        }

        /// <summary>
        /// 初始化图层
        /// </summary>
        protected virtual void init()
        {
            CreateTemplate(MarkerCanvas, Markers);
            this.Children.Add(MarkerCanvas);
        }

        #endregion


        GMapControl control;
        public GMapControl Control
        {
            get
            {
                return control;
            }
            internal set
            {
                control = value;
            }
        }

        public GMapOverlay()
        {
            init();
            CreateEvents();
        }

        public GMapOverlay(string id)
        {
            Id = id;
            init();
            CreateEvents();
        }

        public void Clear()
        {
            Markers.Clear();
        }
        /// <summary>
        /// marker添加删除事件绑定
        /// </summary>
        protected virtual void CreateEvents()
        {
            Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
        }

        /// <summary>
        /// marker添加删除事件绑定
        /// </summary>
        protected virtual void ClearEvents()
        {
            Markers.CollectionChanged -= new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
        }
        public virtual void UpdateBounds()
        { }

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
                    }
                }
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

                            if (obj is IShapable)
                            {
                                (obj as IShapable).RegenerateShape(Control);
                            }
                        }
                    }
                }
            }


        }
    }
}
