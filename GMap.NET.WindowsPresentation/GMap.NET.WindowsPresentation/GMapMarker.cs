
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using GMap.NET;
    using System.Windows.Media;
    using System.Diagnostics;
    using System.Windows.Shapes;
    using System;
    using System.Windows.Media.Imaging;
    using System.IO;

    /// <summary>
    /// GMap.NET marker
    /// </summary>
    public class GMapMarker : MoveObj, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, name);
            }
        }

        UIElement shape;
        static readonly PropertyChangedEventArgs Shape_PropertyChangedEventArgs = new PropertyChangedEventArgs("Shape");

        public string Name = "";

        /// <summary>
        /// marker visual
        /// </summary>
        public UIElement Shape
        {
            get
            {
                return shape;
            }
            set
            {
                if (shape != value)
                {
                    shape = value;
                    if (shape != null)
                    {
                        shape.MouseEnter += shape_MouseEnter;
                        shape.MouseLeave += shape_MouseLeave;
                    }
                    OnPropertyChanged(Shape_PropertyChangedEventArgs);

                    UpdateLocalPosition();
                }
            }
        }

        void shape_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ZIndex -= 10000;
        }

        void shape_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ZIndex += 10000;
        }

        public GMapOverlay Overlay;

        private PointLatLng position;

        /// <summary>
        /// coordinate of marker
        /// </summary>
        public PointLatLng Position
        {
            get
            {
                return position;
            }
            set
            {
                updateLife();
                if (Overlay != null)
                    Overlay.OnMarkerPositionChange(this);
                if (position != value)
                {
                    position = value;
                    UpdateLocalPosition();
                }
            }
        }

        GMapControl map;

        /// <summary>
        /// the map of this marker
        /// </summary>
        public GMapControl Map
        {
            get
            {
                if (Shape != null && map == null)
                {
                    DependencyObject visual = Shape;
                    while (visual != null && !(visual is GMapControl))
                    {
                        visual = VisualTreeHelper.GetParent(visual);
                    }

                    map = visual as GMapControl;
                }

                return map;
            }
            internal set
            {
                map = value;
            }
        }

        /// <summary>
        /// custom object
        /// </summary>
        public object Tag;

        System.Windows.Point offset;
        /// <summary>
        /// offset of marker
        /// </summary>
        public System.Windows.Point Offset
        {
            get
            {
                return offset;
            }
            set
            {
                if (offset != value)
                {
                    offset = value;
                    UpdateLocalPosition();
                }
            }
        }

        int localPositionX;
        static readonly PropertyChangedEventArgs LocalPositionX_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionX");

        /// <summary>
        /// local X position of marker
        /// </summary>
        public int LocalPositionX
        {
            get
            {
                return localPositionX;
            }
            internal set
            {
                if (localPositionX != value)
                {
                    localPositionX = value;
                    OnPropertyChanged(LocalPositionX_PropertyChangedEventArgs);
                }
            }
        }

        int localPositionY;
        static readonly PropertyChangedEventArgs LocalPositionY_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionY");

        /// <summary>
        /// local Y position of marker
        /// </summary>
        public int LocalPositionY
        {
            get
            {
                return localPositionY;
            }
            internal set
            {
                if (localPositionY != value)
                {
                    localPositionY = value;
                    OnPropertyChanged(LocalPositionY_PropertyChangedEventArgs);
                }
            }
        }

        int zIndex;
        static readonly PropertyChangedEventArgs ZIndex_PropertyChangedEventArgs = new PropertyChangedEventArgs("ZIndex");

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
                    OnPropertyChanged(ZIndex_PropertyChangedEventArgs);
                }
            }
        }
        public GMapMarker(PointLatLng pos, string name = "")
        {
            Name = name;
            Position = pos;
        }

        internal GMapMarker()
        {
        }
        static SolidColorBrush sss = new SolidColorBrush(Color.FromRgb(41, 124, 165));
        /// <summary>
        /// InitUI
        /// </summary>
        public virtual void  InitUI()
        {
            Rectangle rect = new Rectangle();
            rect.Width = 20;
            rect.Height = 20;
            rect.Fill = sss;

            Shape = rect;
        }

        public BitmapImage imgSource { get; set; }

        /// <summary>
        /// calls Dispose on shape if it implements IDisposable, sets shape to null and clears route
        /// </summary>
        public virtual void Clear()
        {
            var s = (Shape as IDisposable);
            if (s != null)
            {
                s.Dispose();
                s = null;
            }
            Shape = null;
        }
        Visibility _Visibility = Visibility.Visible;
        /// <summary>
        /// 显示隐藏
        /// </summary>
        public Visibility Visilility
        {
            get
            {
                return _Visibility;
            }
            set
            {
                if (_Visibility != value)
                {
                    _Visibility = value;
                    if (Shape != null)
                    {
                        Shape.Visibility = value;
                    }
                }
            }
        }
        /// <summary>
        /// 是否具有生命周期
        /// </summary>
        public bool HasLife = false;
        private DateTime createTime;

        /// <summary>
        /// 生命状态改变
        /// </summary>
        public bool LifeChanging
        {
            get { return _lifeChanging; }
        }
        bool _lifeChanging = false;
        int _prevLife = 1;
        /// <summary>
        /// 生存期，-1表示删除，0表示已经超时无效，1表示正常
        /// </summary>
        public int Life
        {
            get
            {
                if (!HasLife) return 1;
                if (createTime == null)
                    return -1;
                TimeSpan overdue = new TimeSpan(0, AvaliableTick / 60, AvaliableTick % 60);
                TimeSpan remove = new TimeSpan(0, AvaliableTick / 60, 30 + AvaliableTick % 60);
                TimeSpan cur = DateTime.Now - createTime;
                int result;
                if (cur < overdue)
                    result = 1;
                else if (cur < remove)
                    result = 0;
                else
                    result = -1;
                if (!_lifeChanging)
                    _lifeChanging = _prevLife == result ? false : (result != _prevLife);
                _prevLife = result;
                return result;
            }
        }

        /// <summary>
        /// 生命周期 单位：秒 需要设置HasLife = true生效
        /// </summary>
        public int AvaliableTick = 6;

        /// <summary>
        /// 激活
        /// </summary>
        private void updateLife()
        {
            createTime = DateTime.Now;
        }
        /// <summary>
        /// 超时变灰
        /// </summary>
        public virtual void ToGray(bool isGray)
        {
            if (!_lifeChanging) return;
            _lifeChanging = false;
            if (Shape == null) return;
            if (isGray)
                Shape.Opacity = 0.5;
            else
                Shape.Opacity = 1;
        }

        BitmapImage createBitmapImage(RenderTargetBitmap renderTargetBitmap)
        {
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
        public BitmapSource ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            //Transform transform = obj.LayoutTransform;
            //obj.LayoutTransform = null;

            //// fix margin offset as well
            //Thickness margin = obj.Margin;
            //obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(obj.Width, obj.Height);

            // force control to Update

            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);
            // return values as they were before
            //obj.LayoutTransform = transform;
            //obj.Margin = margin;
            //   bmp.Freeze();
            return createBitmapImage(bmp);
        }

        /// <summary>
        /// updates marker position, internal access usualy
        /// </summary>
        void UpdateLocalPosition()
        {
            if (Map != null)
            {
                GPoint p = Map.FromLatLngToLocal(Position);
                p.Offset(-(long)Map.MapTranslateTransform.X, -(long)Map.MapTranslateTransform.Y);
                LocalPositionX = (int)(p.X + (long)(Offset.X));
                LocalPositionY = (int)(p.Y + (long)(Offset.Y));
            }
        }

        /// <summary>
        /// forces to update local marker  position
        /// dot not call it if you don't really need to ;}
        /// </summary>
        /// <param name="m"></param>
        internal void ForceUpdateLocalPosition(GMapControl m)
        {
            if (m != null)
            {
                map = m;
            }
            UpdateLocalPosition();
        }
    }
}