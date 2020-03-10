using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Demo.WindowsPresentation.CustomMarkers
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : UserControl
    {
        public Test(string txt)
        {
            InitializeComponent();
            if (imgSource == null)
            {
                imgSource = new BitmapImage(
                   new Uri(
                       Directory.GetCurrentDirectory() + "/red-dot.png",
                       UriKind.RelativeOrAbsolute));
                RenderOptions.SetBitmapScalingMode(imgSource, BitmapScalingMode.LowQuality);
                imgSource.Freeze();
            }
        }
        public static BitmapImage imgSource = null;
        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            dc.DrawImage(imgSource, new System.Windows.Rect(0, 0, imgSource.Width, imgSource.Height));
        }
    }
}
