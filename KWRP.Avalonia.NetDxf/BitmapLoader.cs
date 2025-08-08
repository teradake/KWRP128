using Avalonia.Media.Imaging;
using ImageMagick;

namespace KWRP.NetDxf
{
    internal class BitmapLoader
    {
        public static Bitmap? Load(string imageFilePath)
        {
            if (string.IsNullOrEmpty(imageFilePath))
            {
                return null;
            }
            if (!File.Exists(imageFilePath))
            {
                return null;
            }

            var ext = Path.GetExtension(imageFilePath);

            // tiffファイル意外の画像の場合はそのままAvalonia.Media.Imaging.Bitmapを使う
            try
            {
                if (ext != ".tiff" && ext != ".tif")
                {
                    return new Bitmap(imageFilePath);
                }
            }
            catch
            {
                return null;
            }


            // tiffファイルの場合は、ImageMagickを使ってメモリストリーム経由でBitmapを生成する
            try
            {
                // dpiを下げて読み込み
                var settings = new MagickReadSettings
                {
                    Density = new Density(72, 72), // 72 DPI
                };
                using var magickImage = new MagickImage(imageFilePath, settings);

                // 画像のサイズを制限
                uint maxWidth = 1024; // 最大幅
                uint maxHeight = 1024; // 最大高さ
                if (magickImage.Width > maxWidth || magickImage.Height > maxHeight)
                {
                    magickImage.Resize(new MagickGeometry(maxWidth, maxHeight)
                    {
                        IgnoreAspectRatio = false // アスペクト比を維持
                    });
                }

                using var memoryStream = new MemoryStream();
                magickImage.Format = MagickFormat.Png;
                magickImage.Write(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new Bitmap(memoryStream);
            }
            catch
            {
                return null;
            }
        }
    }
}
