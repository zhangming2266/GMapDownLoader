
namespace GMap.NET.WindowsForms
{
    using System.Drawing;
    using System.IO;
    using System.Drawing.Imaging;
    using System;
    using System.Diagnostics;
    using GMap.NET.Internals;
    using GMap.NET.MapProviders;

    /// <summary>
    /// image abstraction
    /// </summary>
    public class GMapImage : PureImage
    {
        public static bool IsGray = false;//黑白地图样式
        public static bool IsFanXuan = false;//反选地图样式
        public static bool IsNight = false;//夜间地图样式
        System.Drawing.Image _Img;
        System.Drawing.Image _ImgGray = null;
        System.Drawing.Image _ImgFanXuan = null;

        public System.Drawing.Image Img
        {
            get
            {
                try
                {
                    if (IsNight)
                    {
                        if (_ImgFanXuan == null)
                        {
                            Bitmap b = new Bitmap(_Img);
                            int width = b.Width;
                            int heght = b.Height;
                            for (int i = 0; i < width; i++)
                            {
                                for (int j = 0; j < heght; j++)
                                {
                                    Color color = b.GetPixel(i, j);
                                    int blue =((int)(color.R/5));
                                    b.SetPixel(i, j, Color.FromArgb( blue, color.G, color.B));
                                }
                            }
                            _ImgFanXuan = (Image)b;
                        }
                        return _ImgFanXuan;
                    }

                    if (IsFanXuan)
                    {
                        if (_ImgFanXuan == null)
                        {
                            Bitmap b = new Bitmap(_Img);
                            int width = b.Width;
                            int heght = b.Height;
                            for (int i = 0; i < width; i++)
                            {
                                for (int j = 0; j < heght; j++)
                                {
                                    Color color = b.GetPixel(i, j);
                                    b.SetPixel(i, j, Color.FromArgb(Math.Abs(255 - color.R), 255 - color.G, 255 - color.B));
                                }
                            }
                            _ImgFanXuan = (Image)b;
                        }
                        return _ImgFanXuan;
                    }

                    if (IsGray)
                    {
                        if (_ImgGray == null)
                        {
                            Bitmap b = new Bitmap(_Img);
                            int width = b.Width;
                            int heght = b.Height;
                            for (int i = 0; i < width; i++)
                            {
                                for (int j = 0; j < heght; j++)
                                {
                                    Color color = b.GetPixel(i, j);
                                    int value = (color.R + color.G + color.B) / 3;
                                    b.SetPixel(i, j, Color.FromArgb(value, value, value));
                                }
                            }
                            _ImgGray = (Image)b;
                        }
                        return _ImgGray;
                    }
                    return _Img;
                }
                catch
                {
                    return _Img;
                }
            }
            set
            {
                _Img = value;
            }
        }



        public override void Dispose()
        {
            if (Img != null)
            {
                Img.Dispose();
                Img = null;
            }

            if (Data != null)
            {
                Data.Dispose();
                Data = null;
            }
        }
    }

    /// <summary>
    /// image abstraction proxy
    /// </summary>
    public class GMapImageProxy : PureImageProxy
    {
        GMapImageProxy()
        {

        }

        public static void Enable()
        {
            GMapProvider.TileImageProxy = Instance;
        }

        public static readonly GMapImageProxy Instance = new GMapImageProxy();

#if !PocketPC
        internal ColorMatrix ColorMatrix;
#endif

        static readonly bool Win7OrLater = Stuff.IsRunningOnWin7orLater();

        public override PureImage FromStream(Stream stream)
        {
            GMapImage ret = null;
            try
            {
#if !PocketPC
                Image m = Image.FromStream(stream, true, Win7OrLater ? false : true);
#else
            Image m = new Bitmap(stream);
#endif
                if (m != null)
                {
                    ret = new GMapImage();
#if !PocketPC
                    ret.Img = ColorMatrix != null ? ApplyColorMatrix(m, ColorMatrix) : m;
#else
               ret.Img = m;
#endif
                }

            }
            catch (Exception ex)
            {
                ret = null;
                Debug.WriteLine("FromStream: " + ex.ToString());
            }

            return ret;
        }

        public override bool Save(Stream stream, GMap.NET.PureImage image)
        {
            GMapImage ret = image as GMapImage;
            bool ok = true;

            if (ret.Img != null)
            {
                // try png
                try
                {
                    ret.Img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch
                {
                    // try jpeg
                    try
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        ret.Img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        ok = false;
                    }
                }
            }
            else
            {
                ok = false;
            }

            return ok;
        }

#if !PocketPC
        Bitmap ApplyColorMatrix(Image original, ColorMatrix matrix)
        {
            // create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            using (original) // destroy original
            {
                // get a graphics object from the new image
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    // set the color matrix attribute
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(matrix);
                        g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                    }
                }
            }

            return newBitmap;
        }
#endif
    }
}
