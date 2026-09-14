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
using KWRP.Backend.Services;
using KWRP.Backend.Enums;


namespace KWRP.Avalonia.Frontend.Services
{

    /// <summary>
    /// ファイルから転圧領域に関する情報を取得し、メモリに保存するためのサービス。
    /// 情報はStoreに格納する
    /// </summary>
    public class CompactionAreaWithLengthUnitLoader : IDataLoader
    {
        private readonly IPathService _pathService;
        private readonly ILogService _logService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ParameterStore _parameterStore;
        private readonly PolygonCsvParser _polygonCsvParser;
        private readonly ICanvasService _canvasService;
        private readonly ApplicationStore _applicationStore;
        private readonly ILanguageService _languageService;

        private readonly Dictionary<FileType, Func<string, LengthUnitType, Task>> _strategies;


        public CompactionAreaWithLengthUnitLoader(
            IPathService pathService,
            ILogService logService,
            CanvasItemStore itemStore,
            ParameterStore parameterStore,
            PolygonCsvParser polygonCsvParser,
            ICanvasService canvasService,
            ApplicationStore applicationStore,
            ILanguageService languageService)
        {
            _pathService = pathService;
            _logService = logService;
            _canvasItemStore = itemStore;
            _parameterStore = parameterStore;
            _polygonCsvParser = polygonCsvParser;
            _canvasService = canvasService;
            _applicationStore = applicationStore;

            _strategies = new Dictionary<FileType, Func<string, LengthUnitType, Task>>()
            {
                { FileType.XML, LoadXmlDataAsync },
                { FileType.CSV, LoadCsvDataAsync },
            };

            _logService.LogDebug("init");
            _languageService = languageService;
        }

        public async Task ExecuteLoadFromDialogAsync(FileType fileType) => await ExecuteLoadFromDialogAsync(fileType, LengthUnitType.Meter);
        public async Task ExecuteLoadFromDialogAsync(FileType fileType, LengthUnitType lengthUnitType)
        {
            if (!_strategies.ContainsKey(fileType))
            {
                throw new ArgumentException(_languageService.GetString("Domain.Front.FileNotSupported"));
            }

            var filePath = await _pathService.GetOpenFilePathAsync(
                fileType: fileType,
                title: _languageService.GetString("Domain.Back.SelectFile"));

            if (string.IsNullOrEmpty(filePath))
            {
                _logService.LogDebug(_languageService.GetString("Domain.Front.FileSelectionCancelled"));
                return;
            }

            await LoadFileAsync(filePath, fileType, lengthUnitType);

            _applicationStore.SpatialDataPath.Value = filePath;
        }
        public async Task ExecuteLoadFromFileAsync(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            var fileType = ext switch
            {
                ".xml" => FileType.XML,
                ".csv" => FileType.CSV,
                _ => throw new ArgumentException(_languageService.GetString("Domain.Front.FileNotSupported")),
            };

            await LoadFileAsync(filePath, fileType, LengthUnitType.Meter);

            _applicationStore.SpatialDataPath.Value = filePath;
        }

        async Task LoadFileAsync(string filePath, FileType fileType, LengthUnitType lengthUnitType)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(String.Format(_languageService.GetString("Domain.Front.FileDoesNotExist"), filePath));
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                { }

                _logService.LogDebug(String.Format(_languageService.GetString("Domain.Front.SelectedAreaFile"), filePath));
                await _strategies[fileType](filePath, lengthUnitType);
                _logService.LogDebug(String.Format(_languageService.GetString("Domain.Front.AreaFileLoadComplete"), filePath));
            }
            catch (IOException ex)
            {
                throw new Exception(String.Format(_languageService.GetString("Domain.Front.FileOpenError"), ex.Message, ex));
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(_languageService.GetString("Domain.Front.FileReadFailed"), ex.Message, ex));
            }
        }

     

        async Task LoadXmlDataAsync(string path, LengthUnitType lengthUnitType)
        {
            try
            {
                var serializer = SerializerFactory.Create<CompactionAreaItems>(FileType.XML);
                var item = await serializer.LoadAsync(path) ?? throw new NullReferenceException();
                var divisor = lengthUnitType == LengthUnitType.Meter ? 1.0 : 1000.0;

                var shell = Polygon.AsCounterClockwise(item
                    .WorkArea
                    .Shell
                    .Points
                    .Select(p => new Vec2(p.X / divisor, p.Y / divisor))
                    .ToArray());
                var holes = item.WorkArea.Holes.Select(hole =>
                {
                    return Polygon.AsClockwise(hole
                        .Points
                        .Select(p => new Vec2(p.X / divisor, p.Y / divisor))
                        .ToArray());
                }).ToList();
                var direction = item.LaneProgressDirection;
                var headingType = item.IsRollerHeadingRight ? RollerHeadingType.Right : RollerHeadingType.Left;

                RegisterData(shell, holes, direction, lengthUnitType, headingType, null);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message +
                    String.Format(_languageService.GetString("Domain.Front.CompactionAreaItemsLoadFailed"), typeof(CompactionAreaItems)));
            }

            try
            {
                var serializer = SerializerFactory.Create<AutomatedConstructionAreaItems>(FileType.XML);
                var item = await serializer.LoadAsync(path) ?? throw new NullReferenceException();
                var divisor = lengthUnitType == LengthUnitType.Meter ? 1.0 : 1000.0;

                var shell = Polygon.AsCounterClockwise(item
                    .VRData
                    .WorkArea
                    .Shell
                    .Points
                    .Select(p => new Vec2(p.X / divisor, p.Y / divisor))
                    .ToArray());
                var holes = item.VRData.WorkArea.Holes.Select(hole =>
                {
                    return Polygon.AsClockwise(hole
                        .Points
                        .Select(p => new Vec2(p.X / divisor, p.Y / divisor))
                        .ToArray());
                }).ToList();
                var direction = item.VRData.LaneProgressDirection;

                var headingType = item.VRData.IsRollerHeadingRight ? RollerHeadingType.Right : RollerHeadingType.Left;
                if (item.VRData.HasRollerHeadingPattern)
                {
                    headingType = item.VRData.RollerHeadingPattern.ToRollerHeadingType();
                }

                Vec2? baseline = null;
                if (item.VRData.HasBaseLinePoint)
                {
                    baseline = new Vec2(
                        x: item.VRData.BaseLinePoint!.X,
                        y: item.VRData.BaseLinePoint!.Y);
                }
                
                RegisterData(shell, holes, direction, lengthUnitType, headingType, baseline);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message +
                    String.Format(_languageService.GetString("Domain.Front.CompactionAreaItemsLoadFailed"), typeof(AutomatedConstructionAreaItems)));
            }

            System.Diagnostics.Debug.WriteLine(_languageService.GetString("Domain.Front.AreaDataLoadFailed"));
            throw new ArgumentException(_languageService.GetString("Domain.Front.AreaDataLoadFailed"));
        }

        async Task LoadCsvDataAsync(string path, LengthUnitType lengthUnitType)
        {
            try
            {
                var shell = await _polygonCsvParser.LoadAsync(path);

                if (lengthUnitType == LengthUnitType.Millimeter && shell != null)
                {
                    var pnts = shell.Points.Select(p => new Vec2(p.X / 1000.0, p.Y / 1000.0));
                    shell = Polygon.AsCounterClockwise(pnts.ToArray());
                }

                var holes = new List<Polygon>();
                double direction = 0.0;
                RegisterData(shell!, holes, direction, lengthUnitType, RollerHeadingType.Left, null);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(_languageService.GetString("Domain.Front.AreaDataLoadFailed"));
                throw new ArgumentException(ex.Message +
                    String.Format(_languageService.GetString("Domain.Front.CompactionAreaItemsLoadFailed"), path));
            }
        }

        void RegisterData(
            Polygon shell, 
            IList<Polygon> holes, 
            double direction, 
            LengthUnitType lengthUnitType, 
            RollerHeadingType rollerHeadingType, 
            Vec2? baseline)
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
                _parameterStore.CurrentLengthUnitType.Value = lengthUnitType;
                _parameterStore.RollerHeadType.Value = rollerHeadingType;

                _canvasService.AdjustAffine();

                var area = _canvasItemStore.TargetPolygons.Sum(p => p.Area);
                var pnts = _canvasItemStore.TargetPolygons.Sum(p => p.N);
                _logService.LogInfo($"area: {area}, holeCount: {pnts}, aabb: {shell.AaBB}");
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(_languageService.GetString("Domain.Front.AreaDataRegistrationException"), ex.Message), ex);
            }
        }
    }
}
