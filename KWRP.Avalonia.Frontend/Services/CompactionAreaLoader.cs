using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Infra.CSV;
using KWRP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trdk.Geometry;
using KWRP.Avalonia.Backend.Model.SpatialDataXmlFormat;
using System.Linq;
using KWRP.Avalonia.Backend.Models.AutomatedConstructionAreaItems;
using Trdk.Geometry.NTS;
using System.IO;


namespace KWRP.Avalonia.Frontend.Services
{

    /// <summary>
    /// ファイルから転圧領域に関する情報を取得し、メモリに保存するためのサービス。
    /// 情報はStoreに格納する
    /// </summary>
    public class CompactionAreaLoader : IDataLoader
    {
        private readonly IPathService _pathService;
        private readonly ILogService _logService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ParameterStore _parameterStore;
        private readonly PolygonCsvParser _polygonCsvParser;
        private readonly ICanvasService _canvasService;
        private readonly ApplicationStore _applicationStore;

        private readonly Dictionary<FileType, Func<string, Task>> _strategies;


        public CompactionAreaLoader(
            IPathService pathService,
            ILogService logService,
            CanvasItemStore itemStore,
            ParameterStore parameterStore,
            PolygonCsvParser polygonCsvParser,
            ICanvasService canvasService,
            ApplicationStore applicationStore)
        {
            _pathService = pathService;
            _logService = logService;
            _canvasItemStore = itemStore;
            _parameterStore = parameterStore;
            _polygonCsvParser = polygonCsvParser;
            _canvasService = canvasService;
            _applicationStore = applicationStore;

            _strategies = new Dictionary<FileType, Func<string, Task>>()
            {
                { FileType.XML, LoadXmlDataAsync },
                { FileType.CSV, LoadCsvDataAsync },
            };

            _logService.LogDebug("init");
        }


        public async Task ExecuteLoadFromDialogAsync(FileType fileType)
        {
            if (!_strategies.ContainsKey(fileType))
            {
                throw new ArgumentException($"ファイルはサポートされていません。有効な拡張子はxml, csvです");
            }

            var filePath = await _pathService.GetOpenFilePathAsync(
                fileType: fileType,
                title: $"ファイルを選択してください");

            if (string.IsNullOrEmpty(filePath))
            {
                _logService.LogDebug("ファイル選択を中止しました");
                return;
            }

            await LoadFileAsync(filePath, fileType);

            _applicationStore.SpatialDataPath.Value = filePath;
        }

        public async Task ExecuteLoadFromFileAsync(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            var fileType = ext switch
            {
                ".xml" => FileType.XML,
                ".csv" => FileType.CSV,
                _ => throw new Exception($"ファイルはサポートされていません。有効な拡張子はxml, csvです"),
            };

            await LoadFileAsync(filePath, fileType);

            _applicationStore.SpatialDataPath.Value = filePath;
        }

        async Task LoadFileAsync(string filePath, FileType fileType)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"指定されたファイルが存在しません: {filePath}");
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                { }

                _logService.LogDebug($"領域ファイル「{filePath}」を選択しました");
                await _strategies[fileType](filePath);
                _logService.LogInfo($"領域ファイル「{filePath}」を読込みが完了しました");
            }
            catch (IOException ex)
            {
                throw new Exception($"ファイルが開かれています\n{ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"ファイル読み込みに失敗しました\n{ex.Message}", ex);
            }
        }

     

        async Task LoadXmlDataAsync(string path)
        {
            try
            {
                var serializer = SerializerFactory.Create<CompactionAreaItems>(FileType.XML);
                var item = await serializer.LoadAsync(path) ?? throw new NullReferenceException();

                var shell = Polygon.AsCounterClockwise(item
                    .WorkArea
                    .Shell
                    .Points
                    .Select(p => new Vec2(p.X, p.Y))
                    .ToArray());
                var holes = item.WorkArea.Holes.Select(hole =>
                {
                    return Polygon.AsClockwise(hole
                        .Points
                        .Select(p => new Vec2(p.X, p.Y))
                        .ToArray());
                }).ToList();
                var direction = item.LaneProgressDirection;

                RegisterData(shell, holes, direction, false);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + $"{typeof(CompactionAreaItems)}のロードに失敗しました。フォーマットが正しくない可能性があります");
            }

            try
            {
                var serializer = SerializerFactory.Create<AutomatedConstructionAreaItems>(FileType.XML);
                var item = await serializer.LoadAsync(path) ?? throw new NullReferenceException();

                var shell = Polygon.AsCounterClockwise(item
                    .VRData
                    .WorkArea
                    .Shell
                    .Points
                    .Select(p => new Vec2(p.X, p.Y))
                    .ToArray());
                var holes = item.VRData.WorkArea.Holes.Select(hole =>
                {
                    return Polygon.AsClockwise(hole
                        .Points
                        .Select(p => new Vec2(p.X, p.Y))
                        .ToArray());
                }).ToList();
                var direction = item.VRData.LaneProgressDirection;

                RegisterData(shell, holes, direction, false);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + $"{typeof(AutomatedConstructionAreaItems)}のロードに失敗しました");
            }

            System.Diagnostics.Debug.WriteLine($"領域データのロードに失敗しました。フォーマットが正しくない可能性があります。");
            throw new ArgumentException($"領域データのロードに失敗しました。フォーマットが正しくない可能性があります。");
        }

        async Task LoadCsvDataAsync(string path)
        {
            try
            {
                var shell = await _polygonCsvParser.LoadAsync(path);
                var holes = new List<Polygon>();
                double direction = 0.0;
                RegisterData(shell!, holes, direction, false);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"領域データのロードに失敗しました。フォーマットが正しくない可能性があります。");
                throw new ArgumentException(ex.Message + $"領域データ{path}のロードに失敗しました。フォーマットが正しくない可能性があります。");
            }
        }

        void RegisterData(Polygon shell, IList<Polygon> holes, double direction, bool headingToRight)
        {
            try
            {
                _canvasItemStore.CompactionAreas.Clear();
                _canvasItemStore.Holes.AddRange(holes);
                _canvasItemStore.CompactionAreas.Add(shell);

                var targets = shell.MakeHole(holes).Select(poly => poly.ToPolygonWithHoles()).ToArray();
                foreach (var target in targets)
                {
                    var offsetPolygons = target.Offset(-_parameterStore.PerimeterAllowance);
                    foreach (var offsetPoly in offsetPolygons)
                    {
                        _canvasItemStore.OffsetCompactionAreas.Add(offsetPoly.Shell);
                        _canvasItemStore.OffsetCompactionAreas.AddRange(offsetPoly.Holes);
                    }
                    _canvasItemStore.TargetPolygons.AddRange(offsetPolygons);
                }

                _parameterStore.ProgresssDirectionRadian.Value = direction;
                _parameterStore.RollerHeadType = headingToRight
                    ? RollerHeadingType.ToRight
                    : RollerHeadingType.ToLeft;
                _canvasService.AdjustAffine();

                var area = _canvasItemStore.TargetPolygons.Sum(p => p.Area);
                var pnts = _canvasItemStore.TargetPolygons.Sum(p => p.N);
                _logService.LogInfo($"area: {area}, holeCount: {pnts}, aabb: {shell.AaBB}");
            }
            catch (Exception ex)
            {
                throw new Exception($"領域データの登録時に例外が発生しました: {ex.Message}", ex);
            }
        }


    }
}
