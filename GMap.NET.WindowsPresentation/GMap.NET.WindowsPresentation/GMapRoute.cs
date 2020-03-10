
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;

    public interface IShapable
    {
        void RegenerateShape(GMapControl map);
    }

    public class GMapRoute : GMapMarker, IShapable
    {
        public readonly List<PointLatLng> Points = new List<PointLatLng>();
        #region 绘制参数
        //
        // 摘要:
        //     获取或设置指定 System.Windows.Shapes.Shape 轮廓绘制方式的 System.Windows.Media.Brush。
        //
        // 返回结果:
        //     一个 System.Windows.Media.Brush，指定 System.Windows.Shapes.Shape 轮廓的绘制方式。默认值为
        //     null。
        public Brush Stroke = Brushes.Navy;
        //
        // 摘要:
        //     获取或设置 System.Windows.Shapes.Shape 轮廓的宽度。
        //
        // 返回结果:
        //     System.Windows.Shapes.Shape 轮廓的宽度。
        public double StrokeThickness = 3;

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
        /// 透明度
        /// </summary>
        public double Opacity = 0.6;
        /// <summary>
        /// 是否响应鼠标事件
        /// </summary>
        public bool IsHitTestVisible = false;
        
        #endregion

        public GMapRoute(IEnumerable<PointLatLng> points, string name = "")
        {
            Name = name;
            Points.AddRange(points);
            RegenerateShape(null);
        }

        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        /// <summary>
        /// regenerates shape of route
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
                    foreach (var i in Points)
                    {
                        var p = Map.FromLatLngToLocal(i);
                        localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
                    }

                    var shape = CreateRoutePath(localPath,false);

                    if (this.Shape is Path)
                    {
                        (this.Shape as Path).Data = shape.Data;
                    }
                    else
                    {
                        this.Shape = shape;
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
        public virtual Path CreateRoutePath(List<Point> localPath, bool addBlurEffect = true)
        {
            // Create a StreamGeometry to use to specify myPath.
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(localPath[0], false, false);

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

                myPath.Opacity = Opacity;
                myPath.IsHitTestVisible = IsHitTestVisible;
            }
            return myPath;
        }
    }
}
