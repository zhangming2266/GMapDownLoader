using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Demo.WindowsPresentation.Source
{
    public class MyMarker : GMapMarker
    {
        public MyMarker(PointLatLng pos)
            : base(pos)
        {

        }
        public MyMarker(PointLatLng pos, string name = "")
            : base(pos)
        {
            Name = name;
            Position = pos;
        }
        public override void InitUI()
        {
            UserControl UIShape = new Test("");
            Shape = UIShape;
            Offset = new System.Windows.Point(-20 / 2, -30 / 2);
        }
    }
}
