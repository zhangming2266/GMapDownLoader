

namespace GMap.NET.WindowsPresentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using System.Windows;
    public class MoveObj
    {
        protected PointLatLng MoveToPoint;
        protected PointLatLng PrevMoveToPoint;
        protected Queue<Point> MovePointQueue = new Queue<Point>();
        public virtual void MoveTo(PointLatLng p)
        {

        }
    }
}
