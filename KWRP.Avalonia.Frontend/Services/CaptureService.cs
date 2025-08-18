using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services
{
    public class CaptureService
    {
        private readonly IPathService _pathService;
        private readonly ILogService _logService;
        private readonly INotificationService _notificationService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly WorkAreaStore _workAreaStore;
        private readonly ParameterStore _parameterStore;
        private readonly MachineStore _machineStore;
        private readonly ActivityStore _activityStore;

        public CaptureService(
            ILogService logService,
            INotificationService notificationService,
            IPathService pathService,
            CanvasItemStore canvasItemStore,
            WorkAreaStore workAreaStore,
            ParameterStore parameterStore,
            ActivityStore activityStore,
            MachineStore machineStore)
        {
            _logService = logService;
            _notificationService = notificationService;
            _pathService = pathService;
            _canvasItemStore = canvasItemStore;
            _workAreaStore = workAreaStore;
            _parameterStore = parameterStore;
            _activityStore = activityStore;
            _machineStore = machineStore;
        }


        private RenderTargetBitmap CaptureCanvas(Layoutable canvas)
        {
            var size = new Size(canvas.Bounds.Width, canvas.Bounds.Height);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            int coeff = 2;
            var renderBitmap = new RenderTargetBitmap(
                new PixelSize(coeff * (int)size.Width, coeff * (int)size.Height),
                new Vector(coeff * 96.0d, coeff * 96.0d));
            //var renderBitmap = new RenderTargetBitmap(
            //    new PixelSize(3 * (int)size.Width, 3 * (int)size.Height),
            //    new Vector(300.0d, 300.0d));
            renderBitmap.Render(canvas);

            return renderBitmap;
        }

        public async Task CaptureToFileAsync(Layoutable? canvas)
        {
            try
            {
                if (canvas == null)
                {
                    return;
                }

                var pngPath = await _pathService.GetSaveFilePathAsync(Backend.Enums.FileType.PNG);
                if (string.IsNullOrEmpty(pngPath))
                {
                    _logService.LogInfo("キャプチャを中止しました");
                    return;
                }

                var renderBitmap = CaptureCanvas(canvas);
                await using var os = new FileStream(pngPath, FileMode.Create);
                renderBitmap.Save(os);

                _notificationService.Notify(KWRPNotification.Create("キャプチャを保存しました", Backend.Enums.NotifyMessageType.Info, 6)
                    .WithCommand("フォルダを開く", () =>
                    {
                        if (!string.IsNullOrWhiteSpace(pngPath))
                        {
                            try
                            {
                                _pathService.SelectFileInExplorer(pngPath);
                            }
                            catch (Exception ex)
                            {
                                _logService.LogError("フォルダのオープンに失敗しました", ex);
                                _notificationService.Notify(
                                    KWRPNotification.Create("フォルダを開けませんでした", NotifyMessageType.Warn, 5));
                            }
                        }
                    }));
            }
            catch (Exception ex)
            {
                _logService.LogError("キャプチャに失敗しました", ex);
                System.Diagnostics.Debug.WriteLine("-------------------------------------");
                System.Diagnostics.Debug.WriteLine(ex);
                throw;
            }
        }

        public async Task CaptureToClipboardAsync(Layoutable? canvas)
        {
            // https://github.com/AvaloniaUI/Avalonia/issues/3588
            try
            {
                if (canvas == null)
                {
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var bitmap = CaptureCanvas(canvas);
                    await ClipboardAvalonia.SetImageAsync(bitmap);

                    _notificationService.Notify(KWRPNotification.Create("キャプチャをクリップボードにコピーしました", Backend.Enums.NotifyMessageType.Info, 3));
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("クリップボードへのキャプチャに失敗しました", ex);
                System.Diagnostics.Debug.WriteLine("-------------------------------------");
                System.Diagnostics.Debug.WriteLine(ex);
                throw;
            }
        }
    
        public async Task SavePlanViewAsync(string path)
        {
            if (!_pathService.TryValidatePath(path, out string? reason))
            {
                throw new IOException(reason);
            }

            var canvas = new Canvas { Background = Brushes.Transparent, };
            var box = Trdk.Geometry.BoundingBox.Identity;

            var mat = Matrix.Identity;
            {
                double ratio = 20;
                var cent = _canvasItemStore.AreaBoundingBox.Value?.Center ?? new Trdk.Geometry.Vec2(0, 0);  // 原点付近にもってくる（これいらんかも）
                var d = _parameterStore.ProgresssDirectionRadian.Value; // 作業進捗方向が右側になるよう回転
                var affine = Matrix.CreateRotation(-d)
                    * Matrix.CreateScale(ratio, -ratio)
                    * Matrix.CreateTranslation(-cent.X, -cent.Y);

                mat *= affine;
            }
            var transform = new MatrixTransform(mat);

            // compactionArea
            foreach (var area in _canvasItemStore.CompactionAreas.Select(p => new CompactionAreaViewModel(p, isHole:false)))
            {
                var points = area.Points.Select(p => mat.Transform(p));

                var tempBox = new Trdk.Geometry.BoundingBox {
                    Xmin = points.Select(p => p.X).Min(),
                    Xmax = points.Select(p => p.X).Max(),
                    Ymin = points.Select(p => p.Y).Min(),
                    Ymax = points.Select(p => p.Y).Max(),
                };
                box = box.Update(tempBox);

                var polygon = new Polygon
                {
                    Points = area.Points,
                    Fill = area.Fill,
                    Stroke = area.Stroke,
                    StrokeThickness = 0.5,
                    RenderTransform = transform,
                    RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative),
                };

                canvas.Children.Add(polygon);
            }

            // hole
            foreach (var area in _canvasItemStore.Holes.Select(p => new CompactionAreaViewModel(p, isHole: true)))
            {
                var points = area.Points.Select(p => mat.Transform(p));

                var tempBox = new Trdk.Geometry.BoundingBox
                {
                    Xmin = points.Select(p => p.X).Min(),
                    Xmax = points.Select(p => p.X).Max(),
                    Ymin = points.Select(p => p.Y).Min(),
                    Ymax = points.Select(p => p.Y).Max(),
                };
                box = box.Update(tempBox);

                var polygon = new Polygon
                {
                    Points = area.Points,
                    Fill = new SolidColorBrush { Color = Colors.White, Opacity = 0.5, },
                    Stroke = area.Stroke,
                    StrokeThickness = 0.4,
                    RenderTransform = transform,
                    RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative),
                };

                canvas.Children.Add(polygon);
            }

            // work areas
            foreach (var area in _workAreaStore.WorkAreas.Select(p => p.Value))
            {
                var points = area.Points.Select(p => mat.Transform(p));

                var tempBox = new Trdk.Geometry.BoundingBox
                {
                    Xmin = points.Select(p => p.X).Min(),
                    Xmax = points.Select(p => p.X).Max(),
                    Ymin = points.Select(p => p.Y).Min(),
                    Ymax = points.Select(p => p.Y).Max(),
                };
                box = box.Update(tempBox);

                canvas.Children.Add(new Polygon
                {
                    Points = area.Points,
                    Fill = area.Fill,
                    Stroke = area.Stroke,
                    StrokeThickness = 0.2,
                    RenderTransform = transform,
                    RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative),
                });
            }

            // goal areas
            foreach (var area in _workAreaStore.WorkAreas.Select(p => p.Value).Select(p => p.GoalArea))
            {
                var points = area.Points.Select(p => mat.Transform(p));

                var tempBox = new Trdk.Geometry.BoundingBox
                {
                    Xmin = points.Select(p => p.X).Min(),
                    Xmax = points.Select(p => p.X).Max(),
                    Ymin = points.Select(p => p.Y).Min(),
                    Ymax = points.Select(p => p.Y).Max(),
                };
                box = box.Update(tempBox);

                canvas.Children.Add(new Polygon
                {
                    Points = area.Points,
                    Fill = area.Fill,
                    Stroke = area.Stroke,
                    StrokeThickness = 0.2,
                    RenderTransform = transform,
                    RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative),
                });
            }

            // Information Box
            {
                var bx = _canvasItemStore.CompactionAreas.Select(p => p.AaBB.Xmax).Max();
                var by = _canvasItemStore.CompactionAreas.Select(p => p.AaBB.Ymax).Max();
                var loc = mat.Transform(new Point(bx, by));

                var laneArrangementParametersText = new TextBlock
                {
                    FontSize = 45,
                    ClipToBounds = false,
                    Padding = new Thickness(5),
                    Text =
                        $"区割りパラメータ\n" +
                        $"　鉄輪幅 : {_parameterStore.LaneWidth:F2}m\n" +
                        $"　前方オフセット : {_parameterStore.FrontOffset:F2}m\n" +
                        $"　後方オフセット : {_parameterStore.RearOffset:F2}m\n" +
                        $"　側方オフセット① : {_parameterStore.SidePrevOffset:F2}m\n" +
                        $"　側方オフセット② : {_parameterStore.SideNextOffset:F2}m\n" +
                        $"　レーンラップ幅 : {_parameterStore.LapWidth:F2}m\n" +
                        $"　作業可能長さ : {_parameterStore.LaneChangeLength:F2}m\n" +
                        $"　作業進捗方向 : {_parameterStore.ProgresssDirectionRadian.Value * Math.PI / 180.0:F2} deg.\n" +
                        $"\n" +
                        $"　最小レーン数 : {_parameterStore.PairCountMin}\n" +
                        $"　最大レーン数 : {_parameterStore.PairCountMax}",
                };
                laneArrangementParametersText.Measure(new Size(double.MaxValue, double.MaxValue));

                var grid = new Grid
                {
                    Margin = new Thickness(20),
                    Background = new SolidColorBrush(Color.FromRgb(57, 197, 187)),
                    RenderTransform = new TranslateTransform { X = box.Xmax, Y = box.Ymin, },
                    ClipToBounds = false,
                    Height = laneArrangementParametersText.DesiredSize.Height,
                    Width = laneArrangementParametersText.DesiredSize.Width,
                };
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.Children.Add(laneArrangementParametersText);
                Grid.SetRow(laneArrangementParametersText, 0);
                canvas.Children.Add(grid);

                var tempBox = new Trdk.Geometry.BoundingBox
                {
                    Xmin = box.Xmax,
                    Xmax = box.Xmax + laneArrangementParametersText.DesiredSize.Width * 1.01,
                    Ymin = box.Ymax - laneArrangementParametersText.DesiredSize.Height * 1.01,
                    Ymax = box.Ymax,
                };

                box = box.Update(tempBox);
            }

            // Arrows
            foreach (var workareas in _workAreaStore.WorkAreas.Select(p => p.Value))
            {
                var line = new Line
                {
                    Stroke = Brushes.Black,
                    EndPoint = workareas.Arrow.EndPoint,
                    StartPoint = workareas.Arrow.StartPoint,
                    StrokeThickness = 0.5,
                    RenderTransform = transform,
                    RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative),
                };
                var head = new Polygon
                {
                    Fill = Brushes.Black,
                    Points = workareas.Arrow.ArrowHeadPoints,
                    RenderTransform = transform,
                    RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative),
                };
                canvas.Children.Add(line);
                canvas.Children.Add(head);
            }

            // info card (vmがうまいこと動かなかったので陽にグリッド作って描画)
            {
                foreach (var act in _activityStore.ActivityCards)
                {
                    var vm = new InfoCardViewModel(act);
                    var loc = new Point(vm.OriginX, vm.OriginY);
                    loc = mat.Transform(loc);

                    var areaId = new TextBlock
                    {
                        Text = vm.AreaID,
                        FontSize = 30 * vm.Scale / 0.12,
                        ClipToBounds = false,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    var AreaLength = new TextBlock
                    {
                        Text = vm.AreaLength,
                        FontSize = 30 * vm.Scale / 0.12,
                        Margin = new Thickness(0,-5),
                        ClipToBounds = false,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    var areaWidth = new TextBlock
                    {
                        Text = vm.AreaWidth,
                        FontSize = 30 * vm.Scale / 0.12,
                        ClipToBounds = false,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    var laneCount = new TextBlock
                    {
                        Text = $"{act.LaneCount}レーン",
                        FontSize = 30 * vm.Scale / 0.12,
                        ClipToBounds = false,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };

                    var grid = new Grid
                    {
                        Margin = new Thickness(1, 5),
                        Background = new SolidColorBrush(Color.FromArgb(159,245,116,238)),
                        RenderTransform = new TranslateTransform { X = loc.X, Y = loc.Y, },
                        ClipToBounds = false,
                    };

                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    grid.Children.Add(areaId);
                    grid.Children.Add(AreaLength);
                    grid.Children.Add(areaWidth);
                    grid.Children.Add(laneCount);
                    Grid.SetRow(areaId, 0);
                    Grid.SetRow(AreaLength, 1);
                    Grid.SetRow(areaWidth, 2);
                    Grid.SetRow(laneCount, 3);
                    
                    canvas.Children.Add(grid);
                }
            }

            

            // 出力
            {
                box = box.ExpandByRatio(1.05);

                var size = new Size(box.Width, box.Height);
                canvas.Measure(size);
                canvas.Arrange(new Rect(-box.Xmin, -box.Ymin, box.Width, box.Height));

                int coeff = 2;
                var renderBitmap = new RenderTargetBitmap(
                    new PixelSize(coeff * (int)size.Width, coeff * (int)size.Height),
                    new Vector(coeff * 96.0d, coeff * 96.0d));
                renderBitmap.Render(canvas);

                try
                {
                    await using var fs = new FileStream(path, FileMode.Create);
                    renderBitmap.Save(fs);
                    _logService.LogInfo($"計画図を{path}に保存しました。");
                }
                catch (Exception ex)
                {
                    _logService.LogError("計画図の保存に失敗しました", ex);
                    throw;
                }
            }
        }
    }
}
