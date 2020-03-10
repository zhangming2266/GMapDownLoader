
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;

    public class GMapPolygon : GMapMarker, IShapable
    {
        public readonly List<PointLatLng> Points = new List<PointLatLng>();

        /// <summary>
        /// 包含的Marker，用于统计数量
        /// </summary>
        public readonly List<GMapMarker> ChildrenMarker = new List<GMapMarker>();
        public Canvas CenterToolTip = new Canvas();
        private Path mPath = null;

        #region 绘制参数
        //
        // 摘要:
        //     获取或设置指定 System.Windows.Shapes.Shape 轮廓绘制方式的 System.Windows.Media.Brush。
        //
        // 返回结果:
        //     一个 System.Windows.Media.Brush，指定 System.Windows.Shapes.Shape 轮廓的绘制方式。默认值为
        //     null。
        public Brush Stroke = Brushes.MidnightBlue;
        /// <summary>
        /// 绘制填充颜色
        /// </summary>
        public Brush Fill = Brushes.AliceBlue;
        //
        // 摘要:
        //     获取或设置 System.Windows.Shapes.Shape 轮廓的宽度。
        //
        // 返回结果:
        //     System.Windows.Shapes.Shape 轮廓的宽度。
        public double StrokeThickness = 5;

        /// <summary>
        /// 线段顶点连接样式
        /// </summary>
        public PenLineJoin StrokeLineJoin = PenLineJoin.Round;
        /// <summary>
        /// 线段头样式
        /// </summary>
        public PenLineCap StrokeStartLineCap = PenLineCap.Triangle;
        /// <summary>
        /// 线段末端样式
        /// </summary>
        public PenLineCap StrokeEndLineCap = PenLineCap.Square;

        /// <summary>
        /// 虚线样式
        /// </summary>
        public DoubleCollection StrokeDashArray = null;

        /// <summary>
        /// 透明度
        /// </summary>
        public double Opacity = 0.2;
        /// <summary>
        /// 是否响应鼠标事件
        /// </summary>
        public bool IsHitTestVisible = true;

        #endregion
        Canvas canvas = new Canvas();
        public GMapPolygon(IEnumerable<PointLatLng> points, string name)
        {
            Name = name;
            Points.AddRange(points);
            RenderToolTip();
            Shape = canvas;
            RegenerateShape(null);
        }

        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        public virtual void RenderToolTip()
        {
            Border b = new Border();
            b.Background = Brushes.AliceBlue;
            b.CornerRadius = new CornerRadius(4);
            b.BorderThickness = new Thickness(0);
            Label l = new Label();
            l.Content = ChildrenMarker.Count.ToString();
            b.Child = l;
            CenterToolTip.Children.Clear();
            CenterToolTip.Children.Add(b);
        }
        /// <summary>
        /// regenerates shape of polygon
        /// </summary>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                this.Map = map;

                if (Points.Count > 1)
                {
                    Position = Points[0];

                    var localPath = new List<System.Windows.Point>(Points.Count);
                    var offset = Map.FromLatLngToLocal(Points[0]);
                    double maxX = -10000;
                    double maxY = -10000;
                    double minX = 10000;
                    double minY = 10000;
                    foreach (var i in Points)
                    {
                        var p = Map.FromLatLngToLocal(i);
                        double x = p.X - offset.X;
                        double y = p.Y - offset.Y;
                        if (minX > x)
                            minX = x;
                        if (minY > y)
                            minY = y;
                        if (maxX < x)
                            maxX = x;
                        if (maxY < y)
                            maxY = y;
                        localPath.Add(new System.Windows.Point(x, y));
                    }

                    var shape = CreatePolygonPath(localPath, false);
                    if (mPath == null)
                    {
                        mPath = shape;
                        canvas.Children.Add(mPath);
                        if (CenterToolTip != null)
                            canvas.Children.Add(CenterToolTip);
                    }
                    else
                        mPath.Data = shape.Data;
                    if (CenterToolTip != null)
                    {
                       
                            if (maxX - minX > 140)
                                CenterToolTip.Visibility = Visibility.Visible;
                            else
                                CenterToolTip.Visibility = Visibility.Collapsed;

                        Canvas.SetLeft(CenterToolTip, (maxX + minX) / 2 - ((Canvas)CenterToolTip).ActualWidth / 2);
                        Canvas.SetTop(CenterToolTip, (maxY + minY) / 2 - ((Canvas)CenterToolTip).ActualHeight / 2);
                    }
                }
                else
                {
                    this.Shape = null;
                }
            }
        }

        /// <summary>
        /// creates path from list of points, for performance set addBlurEffect to false
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public virtual Path CreatePolygonPath(List<Point> localPath, bool addBlurEffect)
        {
            // Create a StreamGeometry to use to specify myPath.
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(localPath[0], true, true);

                // Draw a line to the next specified point.
                ctx.PolyLineTo(localPath, true, true);
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            geometry.Freeze();

            // Create a path to draw a geometry with.
            Path myPath = new Path();
            {
                // Specify the shape of the Path using the StreamGeometry.
                myPath.Data = geometry;

                if (addBlurEffect)
                {
                    BlurEffect ef = new BlurEffect();
                    {
                        ef.KernelType = KernelType.Gaussian;
                        ef.Radius = 3.0;
                        ef.RenderingBias = RenderingBias.Performance;
                    }

                    myPath.Effect = ef;
                }

                myPath.Stroke = Stroke;
                myPath.StrokeThickness = StrokeThickness;
                myPath.StrokeLineJoin = StrokeLineJoin;
                myPath.StrokeStartLineCap = StrokeStartLineCap;
                myPath.StrokeEndLineCap = StrokeEndLineCap;
                myPath.StrokeDashArray = StrokeDashArray;
                myPath.Fill = Fill;

                myPath.Opacity = Opacity;
                myPath.IsHitTestVisible = IsHitTestVisible;
                myPath.MouseEnter += myPath_MouseEnter;
                myPath.MouseLeave += myPath_MouseLeave;

            }
            return myPath;
        }

        void myPath_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Path p = sender as Path;
            p.Opacity = 0.2;
        }

        void myPath_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Path p = sender as Path;
            p.Opacity = 0.4;
        }

        /// <summary>
        /// checks if point is inside the polygon,
        /// info.: http://greatmaps.codeplex.com/discussions/279437#post700449
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsInside(PointLatLng p)
        {
            int count = Points.Count;

            if (count < 3)
            {
                return false;
            }

            bool result = false;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                var p1 = Points[i];
                var p2 = Points[j];

                if (p1.Lat < p.Lat && p2.Lat >= p.Lat || p2.Lat < p.Lat && p1.Lat >= p.Lat)
                {
                    if (p1.Lng + (p.Lat - p1.Lat) / (p2.Lat - p1.Lat) * (p2.Lng - p1.Lng) < p.Lng)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

    }
}
