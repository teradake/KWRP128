using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using KWRP.Avalonia.Backend.Services;
using netDxf;
using Trdk.Geometry;

namespace KWRP.Avalonia.NetDxf
{
    internal class AvaloniaDxfRenderer
    {
        public static async Task<RenderTargetBitmap> RenderAsync(DxfDocument doc, DxfConverterOption option, ILogService? logger=null)
        {
            try
            {
                var box = new BoundingBox
                {
                    Xmin = option.Xmin,
                    Xmax = option.Xmin + option.MapWidth,
                    Ymin = option.Ymin,
                    Ymax = option.Ymin + option.MapHeight,
                };

                logger?.LogInfo("dxfファイルからエンティティのロード開始");
                var shapes = await Task.Run(() => DxfLoader.Load(doc, box));
                logger?.LogInfo("dxfファイルからエンティティのロード完了");


                logger?.LogInfo($"描画処理を開始");
                var dxfCanvas = new DxfCanvas
                {
                    Shapes = shapes,
                };
                dxfCanvas.InvalidateVisual();


                logger?.LogInfo($"bitmapの作成開始");
                var size = new Size(box.Width, box.Height);
                dxfCanvas.Measure(size);
                dxfCanvas.Arrange(new Rect(size));

                var coeff = option.PixelsPerMeter;
                var renderBitmap = new RenderTargetBitmap(
                new PixelSize(coeff * (int)size.Width, coeff * (int)size.Height),
                new Vector(coeff * 96.0d, coeff * 96.0d));
                renderBitmap.Render(dxfCanvas);
                
                return renderBitmap;
            }
            catch (Exception e)
            {
                logger?.LogError("bitmapの生成に失敗しました: " + e.Message, e);
                throw;
            }
        }
    }


    internal class DxfLoader
    {
        public static List<DxfShape> Load(string path, BoundingBox? captureRange=null)
        {
            var doc = DxfDocument.Load(path);
            return Load(doc, captureRange);
        }

        public static List<DxfShape> Load(DxfDocument doc, BoundingBox? captureRange = null)
        {
            var shapes = new List<DxfShape>();  
            var entities = doc.Entities;

            var ucs = doc.DrawingVariables.CurrentUCS;
            var ucs_transform = ucs.GetTransformation().Inverse();
            
            var rot = Matrix3.RotationZ(0);
            var translate = new Vector3
            {
                X = -captureRange?.Xmin ?? 0.0,
                Y = -captureRange?.Ymin ?? 0.0,
                Z = 0.0,
            };

              
            foreach (var line in entities.Lines.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                line.TransformBy(ucs_transform, -ucs.Origin);
                line.TransformBy(rot, translate);
                shapes.Add(line.ToShape());
            }

            foreach (var poly in entities.Polylines2D.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                poly.TransformBy(ucs_transform, -ucs.Origin);
                poly.TransformBy(rot, translate);
                shapes.Add(poly.ToShape());
            }

            foreach (var poly in entities.Polylines3D.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                poly.TransformBy(ucs_transform, -ucs.Origin);
                poly.TransformBy(rot, translate);
                shapes.Add(poly.ToShape());
            }

            foreach (var circle in entities.Circles.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                circle.TransformBy(ucs_transform, -ucs.Origin);
                circle.TransformBy(rot, translate);
                shapes.Add(circle.ToShape());
            }

            foreach (var arc in entities.Arcs.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                arc.TransformBy(ucs_transform, -ucs.Origin);
                arc.TransformBy(rot, translate);
                shapes.Add(arc.ToShape());
            }

            foreach (var text in entities.MTexts.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                text.TransformBy(ucs_transform, -ucs.Origin);
                text.TransformBy(rot, translate);
                shapes.Add(text.ToShape());
            }

            foreach (var text in entities.Texts.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                text.TransformBy(ucs_transform, -ucs.Origin);
                text.TransformBy(rot, translate);
                shapes.Add(text.ToShape());
            }

            foreach (var insert in entities.Inserts.Where(e => e.Layer.IsVisible && !e.Layer.IsFrozen))
            {
                insert.TransformBy(ucs_transform, -ucs.Origin);
                insert.TransformBy(rot, translate);
                shapes.AddRange(insert.ToShapes());
            }


            return shapes;
        }
    }
}
