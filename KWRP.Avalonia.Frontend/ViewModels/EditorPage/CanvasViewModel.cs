using KWRP.Avalonia.Backend;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using ObservableCollections;
using R3;
using System;
using System.Linq;
using Trdk.Geometry;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Input;
using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Avalonia.NetDxf;
using Avalonia.Media.Imaging;
using KWRP.Avalonia.Frontend.Services.Dxf;
using System.Threading.Tasks;
using System.IO;
using KWRP.Avalonia.Backend.Enums;

namespace KWRP.Avalonia.Frontend.ViewModels.EditorPage
{
    /// <summary>
    /// このクラスはシングルトンとするので、Disposeする必要なし
    /// </summary>
    public class CanvasViewModel : ViewModelBase
    {
        static int instanceCreatedCount = 0;

        private readonly CanvasStateStore _canvasStateStore;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ILogService _logService;
        private readonly ICanvasService _canvasService;
        private readonly ParameterStore _parameterStore;
        private readonly WorkAreaStore _workAreaStore;
        private readonly IDirectionArrowService _directionArrowService;
        private readonly IDragRectService _dragRectService;
        private readonly CaptureService _captureService;
        private readonly ActivityStore _activityStore;
        private readonly ApplicationStore _applicationStore;
        private readonly DxfStore _dxfStore;
        private readonly DxfConvertService _convertService;


        public CanvasViewModel(
            CanvasItemStore canvasItemStore,
            CanvasStateStore canvasStateStore,
            ILogService logService,
            ICanvasService canvasService,
            ParameterStore parameterStore,
            WorkAreaStore workAreaStore,
            IDirectionArrowService directionArrowService,
            CaptureService captureService,
            IDragRectService dragRectService,
            ActivityStore activityStore,
            ApplicationStore applicationStore,
            DxfStore dxfStore,
            DxfConvertService dxfConvertService)
        {
            _canvasItemStore = canvasItemStore;
            _activityStore = activityStore;
            _canvasStateStore = canvasStateStore;
            _logService = logService;
            _canvasService = canvasService;
            _parameterStore = parameterStore;
            _applicationStore = applicationStore;
            _workAreaStore = workAreaStore;
            _directionArrowService = directionArrowService;
            _captureService = captureService;
            _dragRectService = dragRectService;
            _dxfStore = dxfStore;
            _convertService = dxfConvertService;


            IsParallelMode = _parameterStore
                .IsParpendicularMode
                .Select(b => !b)
                .ToReadOnlyBindableReactiveProperty(!_parameterStore.IsParpendicularMode.Value)
                .AddTo(Disposables);

            IsParpendicularMode = _parameterStore
                .IsParpendicularMode
                .ToReadOnlyBindableReactiveProperty(_parameterStore.IsParpendicularMode.Value)
                .AddTo(Disposables);


            CanvasScaleInv = _canvasStateStore
                .Scale
                .Select(s => s == 0 ? 1.0 : Math.Clamp(1.0 / s, 0.01, 100))
                .ToBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue);

            Thick = CanvasScaleInv
                .Select(s => KWRPConstants.C_LINE_THICKNESS * s)
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue * KWRPConstants.C_LINE_THICKNESS);

            ActivityThick = CanvasScaleInv
                .Select(s => KWRPConstants.C_ACT_STROKE_THICKNESS * s)
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue * KWRPConstants.C_ACT_STROKE_THICKNESS);

            SelectedActivityThick = CanvasScaleInv
                .Select(s => KWRPConstants.C_ACTIVITYGROUP_PERIM_THICKNESS * s)
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue * KWRPConstants.C_ACTIVITYGROUP_PERIM_THICKNESS);

            LaneThick = CanvasScaleInv
                .Select(s => KWRPConstants.C_LANE_PERIM_THICKNESS * s)
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue * KWRPConstants.C_LANE_PERIM_THICKNESS);

            PairedLaneThick = CanvasScaleInv
                .Select(s => KWRPConstants.C_PAIREDLANE_PERIM_THICKNESS * s)
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue * KWRPConstants.C_PAIREDLANE_PERIM_THICKNESS);

            RulerFontSize = CanvasScaleInv
                .Select(s => KWRPConstants.C_POCHIRULER_FONTSIZE * s)
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.Scale.CurrentValue * KWRPConstants.C_POCHIRULER_FONTSIZE);

            DragRect = _canvasItemStore
                .DragRect
                .Select(item => new DragRectViewModel(item))
                .ToReadOnlyBindableReactiveProperty(new DragRectViewModel(_canvasItemStore.DragRect.Value));

            DirectionArrow = _canvasItemStore
                .DirectionArrow
                .ToReadOnlyBindableReactiveProperty(new ArrowViewModel(0, 0, 0, 0, isVisible: false));

            IsDirectionSelectionMode = _canvasStateStore
                .IsDirectionSelectionMode
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.IsDirectionSelectionMode.Value);

            IsOptimizing = _canvasStateStore
                .IsOptimizing
                .ToReadOnlyBindableReactiveProperty(_canvasStateStore.IsOptimizing.Value);

            _canvasItemStore
                .DirectionArrow
                .Where(arrow => !arrow.IsVisible && arrow.Length > KWRPConstants.C_EPS)
                .Subscribe(_ => _canvasStateStore.IsDirectionSelectionMode.Value = false);

            Pochis = _canvasItemStore
                .RulerPoints
                .CreateView(p => new PochiViewModel(p))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            PochiLinks = _canvasItemStore
                .RulerLinks
                .CreateView(s => new PochiLinkViewModel(s.Src, s.Dst))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            CompactionAreas = _canvasItemStore
                .CompactionAreas
                .CreateView(p => new CompactionAreaViewModel(p, false))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            Obstacles = _canvasItemStore
                .Holes
                .CreateView(p => new CompactionAreaViewModel(p, true))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            OffsetCompactionAreas = _canvasItemStore
                .OffsetCompactionAreas
                .CreateView(p => new CompactionAreaViewModel(p, true))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            Lanes = _canvasItemStore
                .Lanes
                .CreateView(lane => new LaneViewModel(lane))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            PairedLanes = _canvasItemStore
                .PairedLanes
                .CreateView(p => new PairedLaneViewModel(p))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            WorkAreas = _workAreaStore
                .WorkAreas
                .CreateView(p => p.Value)
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            DummyActivities = _activityStore
                .ActivityCards
                .CreateView(p => new ActivityViewModel(p))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            SelectedActivityGroup = _activityStore
                .CurrentGroupItems
                .CreateView(p => new ActivityViewModel(p))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            Cards = _activityStore
                .ActivityCards
                .CreateView(p => new InfoCardViewModel(p))
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            SelectedActivity = _activityStore
                .SelectedAcitivty
                .Select(p => p != null ? new ActivityViewModel(p) : null)
                .ToBindableReactiveProperty();

            SelectedActivity
                .Subscribe(p =>
                {
                    System.Diagnostics.Debug.WriteLine(p);
                });

            PlanViewOption = _dxfStore.SelectedOption
                .Select(vm => vm?.ToModel() ?? null)
                .ToBindableReactiveProperty(_dxfStore.SelectedOption.Value?.ToModel() ?? null);

            PlanView = PlanViewOption
                .Select(op =>
                {
                    if (op == null || op.DxfFilePath == null) return null;
                    return _convertService.GetSavePngPath(op);
                })
                .SelectAwait(async (png, ct) =>
                {
                    if (string.IsNullOrEmpty(png) || !File.Exists(png))
                        return null;

                    await using var imageStream = File.OpenRead(png);
                    return await Task.Run(() => Bitmap.DecodeToWidth(imageStream, PlanViewOption.Value!.MapWidth * PlanViewOption.Value!.PixelsPerMeter));
                })
                .ToReadOnlyBindableReactiveProperty();

            IsLaneDesign = _applicationStore
                .EditorMode
                .Select(mode => mode == Backend.Enums.EditorMode.LaneDesign)
                .ToBindableReactiveProperty(_applicationStore.EditorMode.Value == Backend.Enums.EditorMode.LaneDesign);
            IsWorkAreaDesign = _applicationStore
                .EditorMode
                .Select(mode => mode == Backend.Enums.EditorMode.WorkAreaDesign)
                .ToBindableReactiveProperty(_applicationStore.EditorMode.Value == Backend.Enums.EditorMode.WorkAreaDesign);
            IsActivityDesign = _applicationStore
                .EditorMode
                .Select(mode => mode == Backend.Enums.EditorMode.ActivityDesign)
                .ToBindableReactiveProperty(_applicationStore.EditorMode.Value == Backend.Enums.EditorMode.ActivityDesign);

            IsLaneDesign
                .Where(b => b)
                .Subscribe(_ =>
                {
                    LaneVisible.Value = true;
                    PairedLaneVisible.Value = true;
                    WorkAreaVisible.Value = false;
                    GoalAreaVisible.Value = false;
                    ActivityVisible.Value = false;
                });
            IsWorkAreaDesign
                .Where(b => b)
                .Subscribe(_ =>
                {
                    LaneVisible.Value = false;
                    PairedLaneVisible.Value = false;
                    WorkAreaVisible.Value = true;
                    GoalAreaVisible.Value = true;
                    ActivityVisible.Value = false;
                });
            IsActivityDesign
                .Where(b => b)
                .Subscribe(_ =>
                {
                    LaneVisible.Value = false;
                    PairedLaneVisible.Value = false;
                    WorkAreaVisible.Value = true;
                    GoalAreaVisible.Value = true;
                    ActivityVisible.Value = true;
                });


            LanesCount = _canvasItemStore.LanesCount.ToReadOnlyBindableReactiveProperty(_canvasItemStore.LanesCount.CurrentValue);
            AreasArea = _canvasItemStore.AreaOfCompactionArea.ToReadOnlyBindableReactiveProperty(_canvasItemStore.LanesCount.CurrentValue);
            LanesArea = _canvasItemStore.AreaOfLanes.ToReadOnlyBindableReactiveProperty(_canvasItemStore.AreaOfLanes.CurrentValue);
            Coverage = _canvasItemStore.AreaOfLanes
                .CombineLatest(_canvasItemStore.AreaOfCompactionArea, (lane, perim) => perim > 0 ? lane / perim * 100 : -1)
                .ToReadOnlyBindableReactiveProperty(-1);

            CanvasHeight = _canvasStateStore.CanvasHeight.ToReadOnlyBindableReactiveProperty();
            CanvasWidth = _canvasStateStore.CanvasWidth.ToReadOnlyBindableReactiveProperty();
            CanvasRotation = _canvasStateStore.Rot.ThrottleLast(TimeSpan.FromMilliseconds(50)).ToReadOnlyBindableReactiveProperty(_canvasStateStore.Rot.CurrentValue);
            ProgressDir = _parameterStore
                .ProgresssDirectionRadian
                .Select(radian => Direction2.ByRadian(radian))
                .ToReadOnlyBindableReactiveProperty(Direction2.ByRadian(_parameterStore.ProgresssDirectionRadian.Value));
            //RollerHeadDir = _parameterStore
            //    .RollerHeadType
            //    .Select(typ => _parameterStore.ProgresssDirectionRadian.Value + typ.ToAngleRadian())
            //    .Select(radian => Direction2.ByRadian(radian))
            //    .ToReadOnlyBindableReactiveProperty(Direction2.ByRadian(_parameterStore.ProgresssDirectionRadian.Value + _parameterStore.RollerHeadType.Value.ToAngleRadian()));
            RollerHeadDir = _parameterStore
                .RollerHeadType
                .Select(typ => typ.ToAngleRadian())
                .Select(radian => Direction2.ByRadian(radian))
                .ToReadOnlyBindableReactiveProperty(Direction2.ByRadian(_parameterStore.RollerHeadType.Value.ToAngleRadian()));



            Affine = _canvasStateStore.Affine
                .Select(m => new MatrixTransform(m))
                .ToReadOnlyBindableReactiveProperty(new MatrixTransform(_canvasStateStore.Affine.Value));
            Box = _canvasItemStore.AreaBoundingBox
                .Select(BoundingBoxViewModel.Create)
                .ToReadOnlyBindableReactiveProperty(BoundingBoxViewModel.Create(_canvasItemStore.AreaBoundingBox.Value));

            SidePrevOffset = _parameterStore.SidePrevOffset.ToReadOnlyBindableReactiveProperty(_parameterStore.SidePrevOffset.Value);
            SideNextOffset = _parameterStore.SideNextOffset.ToBindableReactiveProperty(_parameterStore.SideNextOffset.Value);

            SideNextOffset.Subscribe(v => _logService.LogDebug(v.ToString())).AddTo(Disposables);

            RemovePochiCommand = _canvasItemStore
                .RulerPoints
                .ObserveCountChanged()
                .Select(cnt => cnt > 0)
                .ToReactiveCommand(_ =>
                {
                    _canvasItemStore.RulerPoints.RemoveAt(_canvasItemStore.RulerPoints.Count - 1);
                    if (_canvasItemStore.RulerLinks.Count > 0)
                        _canvasItemStore.RulerLinks.RemoveAt(_canvasItemStore.RulerLinks.Count - 1);
                });

            CloseDirectionSelectionModeCommand = _canvasStateStore
                .IsDirectionSelectionMode
                .ToReactiveCommand(_ =>
                {
                    _canvasStateStore.IsDirectionSelectionMode.Value = false;
                    _canvasItemStore.DirectionArrow.Value = new ArrowViewModel(0, 0, 0, 0, isVisible: false);
                });

            RotateCanvasCommand = new ReactiveCommand<string>(dir => _canvasService.Rotate(dir == "ccw" ? +10 : -10));
            RotateCanvasEastToRightCommand = new ReactiveCommand(_ => _canvasService.Rotate(-_canvasStateStore.Rot.CurrentValue.DegreeValue));
            RotateCanvasProgressDirectionToRightCommand = new ReactiveCommand(_ => _canvasService.Rotate(-(_canvasStateStore.Rot.CurrentValue.DegreeValue + _parameterStore.ProgresssDirectionRadian.Value * 180.0 / Math.PI)));

            OpenDirectionSelectionModeCommand = IsLaneDesign
                .ToReactiveCommand(_ =>
                {
                    _canvasStateStore.IsDirectionSelectionMode.Value = true;
                });

            CaptureCommand = new ReactiveCommand<Layoutable>(async (canvas, ct) =>
            {
                try
                {
                    _applicationStore.IsBusy.Value = true;
                    await _captureService.CaptureToFileAsync(canvas);
                }
                catch (Exception ex)
                {
                    _logService.LogError(ex.Message, ex);
                }
                finally
                {
                    _applicationStore.IsBusy.Value = false;
                }
            });

            CaptureClipboardCommand = new ReactiveCommand<Layoutable>(async (canvas, ct) =>
            {
                try
                {
                    _applicationStore.IsBusy.Value = true;
                    await _captureService.CaptureToClipboardAsync(canvas);
                }
                catch (Exception ex)
                {
                    _logService.LogError(ex.Message, ex);
                }
                finally
                {
                    _applicationStore.IsBusy.Value = false;
                }
            });

            if (++instanceCreatedCount > 1)
            {
                _logService.LogWarn(
                    "CanvasViewModelが複数回インスタンス化されています。\n" +
                    "メモリリークの可能性があるのでCanvasViewModelはシングルトンとして宣言してください。");
            }
            _logService.LogDebug("init CanvasViewModel");
        }

        public IReadOnlyBindableReactiveProperty<DragRectViewModel> DragRect { get; }
        public INotifyCollectionChangedSynchronizedViewList<PochiViewModel> Pochis { get; }
        public INotifyCollectionChangedSynchronizedViewList<PochiLinkViewModel> PochiLinks { get; }
        public INotifyCollectionChangedSynchronizedViewList<CompactionAreaViewModel> CompactionAreas { get; }
        public INotifyCollectionChangedSynchronizedViewList<CompactionAreaViewModel> Obstacles { get; }
        public INotifyCollectionChangedSynchronizedViewList<CompactionAreaViewModel> OffsetCompactionAreas { get; }
        public INotifyCollectionChangedSynchronizedViewList<LaneViewModel> Lanes { get; }
        public INotifyCollectionChangedSynchronizedViewList<PairedLaneViewModel> PairedLanes { get; }
        public INotifyCollectionChangedSynchronizedViewList<WorkAreaViewModel> WorkAreas { get; }
        public INotifyCollectionChangedSynchronizedViewList<ActivityViewModel> DummyActivities { get; }
        public INotifyCollectionChangedSynchronizedViewList<ActivityViewModel> SelectedActivityGroup { get; }
        public INotifyCollectionChangedSynchronizedViewList<InfoCardViewModel> Cards { get; }
        public BindableReactiveProperty<ActivityViewModel?> SelectedActivity { get; }

        public ReactiveCommand RemovePochiCommand { get; }
        public ReactiveCommand CloseDirectionSelectionModeCommand { get; }
        public ReactiveCommand OpenDirectionSelectionModeCommand { get; }
        public ReactiveCommand<Layoutable> CaptureCommand { get; }
        public ReactiveCommand<string> RotateCanvasCommand { get; }
        public ReactiveCommand RotateCanvasEastToRightCommand { get; }
        public ReactiveCommand RotateCanvasProgressDirectionToRightCommand { get; }
        public ReactiveCommand<Layoutable> CaptureClipboardCommand { get; }


        public IReadOnlyBindableReactiveProperty<double> Thick { get; }
        public IReadOnlyBindableReactiveProperty<double> LaneThick { get; }
        public IReadOnlyBindableReactiveProperty<double> PairedLaneThick { get; }
        public IReadOnlyBindableReactiveProperty<double> ActivityThick { get; }
        public IReadOnlyBindableReactiveProperty<double> SelectedActivityThick { get; }
        public IReadOnlyBindableReactiveProperty<double> RulerFontSize { get; }

        public IReadOnlyBindableReactiveProperty<int> CanvasHeight { get; }
        public IReadOnlyBindableReactiveProperty<int> CanvasWidth { get; }
        public BindableReactiveProperty<double> CanvasScaleInv { get; }
        public IReadOnlyBindableReactiveProperty<MatrixTransform> Affine { get; }
        public IReadOnlyBindableReactiveProperty<BoundingBoxViewModel?> Box { get; }
        public IReadOnlyBindableReactiveProperty<Direction2> CanvasRotation { get; }
        public IReadOnlyBindableReactiveProperty<Direction2> ProgressDir { get; }
        public IReadOnlyBindableReactiveProperty<Direction2> RollerHeadDir { get; }

        public IReadOnlyBindableReactiveProperty<double> SidePrevOffset { get; }
        public BindableReactiveProperty<double> SideNextOffset { get; }


        public BindableReactiveProperty<DxfConverterOption?> PlanViewOption { get; }
        public IReadOnlyBindableReactiveProperty<Bitmap?> PlanView { get; }
        public IReadOnlyBindableReactiveProperty<int> PlanPosX { get; }
        public IReadOnlyBindableReactiveProperty<int> PlanPosY { get; }


        public IReadOnlyBindableReactiveProperty<bool> IsOptimizing { get; }
        public IReadOnlyBindableReactiveProperty<bool> IsDirectionSelectionMode { get; }
        public IReadOnlyBindableReactiveProperty<ArrowViewModel> DirectionArrow { get; }

        public IReadOnlyBindableReactiveProperty<bool> IsParallelMode { get; }
        public IReadOnlyBindableReactiveProperty<bool> IsParpendicularMode { get; }

        public IReadOnlyBindableReactiveProperty<int> LanesCount { get; }    // 生成されたレーン本数
        public IReadOnlyBindableReactiveProperty<double> LanesArea { get; }     // 生成されたレーンの総面積
        public IReadOnlyBindableReactiveProperty<double> AreasArea { get; }     // 転圧領域の総面積
        public IReadOnlyBindableReactiveProperty<double> Coverage { get; }     // 充填率

        public BindableReactiveProperty<bool> IsLaneDesign { get; }
        public BindableReactiveProperty<bool> IsWorkAreaDesign { get; }
        public BindableReactiveProperty<bool> IsActivityDesign { get; }
        public BindableReactiveProperty<bool> PlanViewVisible { get; } = new(true);
        public BindableReactiveProperty<bool> LaneVisible { get; } = new(true);
        public BindableReactiveProperty<bool> PairedLaneVisible { get; } = new(true);
        public BindableReactiveProperty<bool> WorkAreaVisible { get; } = new(true);
        public BindableReactiveProperty<bool> GoalAreaVisible { get; } = new(true);
        public BindableReactiveProperty<bool> ActivityVisible { get; } = new(true);

        
        public void AddPochi(double x, double y)
        {
            var pochi = new Vec2(x, y);
            if (_canvasItemStore.RulerPoints.Count > 0 && _canvasItemStore.RulerPoints.Last().Equals(pochi))
            {
                return;
            }
            _canvasItemStore.RulerPoints.Add(pochi);
        }
        public void ClearPochi() => _canvasItemStore.RulerPoints.Clear();

        public void SetSize(int height, int width) => _canvasService.SetWindowSize(height, width);
        public void PointerTranslate(double dx, double dy) => _canvasService.Translate(dx, dy);
        public void PointerScaleAt(double scale, double x, double y) => _canvasService.ScaleAt(scale, x, y);
        public void Scale(double scale) => PointerScaleAt(scale, _canvasItemStore.AreaBoundingBox.Value?.Center.X ?? 0, _canvasItemStore.AreaBoundingBox.Value?.Center.Y ?? 0);
        public void ResetAffine() => _canvasService.ResetAffine();
        public void AdjustAffine() => _canvasService.AdjustAffine();
        public void RotateAffine(double rotDegree) => _canvasService.Rotate(rotDegree);

        public void StartDirectionSelection() => _canvasStateStore.IsDirectionSelectionMode.Value = true;
        public void SetDirectionAt(double x, double y) => _directionArrowService.SetArrowAt(x, y);
        public async Task ReleaseDirectionAt(double x, double y)
        {
            try
            {
                _canvasStateStore.IsOptimizing.Value = true;
                await _directionArrowService.ReleaseArrowAt(x, y);
            }
            finally
            {
                _canvasStateStore.IsOptimizing.Value = false;
            }
        }
        public void UpdateDirectionArrow(double x, double y) =>_directionArrowService.UpdateArrowAt(x, y);

        public void SetDragRectAt(double x, double y) => _dragRectService.SetDragRectAt(x, y);
        public void ReleaseDragRectAt(double x, double y) => _dragRectService.ReleaseDragRectAt(x, y);
        public void UpdateDragRect(double x, double y) => _dragRectService.UpdateDragRectAt(x, y);

        
    }

}