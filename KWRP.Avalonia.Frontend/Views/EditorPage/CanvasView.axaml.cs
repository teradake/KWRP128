using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Frontend.Services.Extensions;
using KWRP.Avalonia.Frontend.ViewModels.EditorPage;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Views.EditorPage;

public partial class CanvasView : UserControl
{
    CanvasViewModel? VM => _vm ??= (DataContext as CanvasViewModel);
    CanvasViewModel? _vm;

    public CanvasView()
    {
        InitializeComponent();

        SetMousePosition();
        SetMouseWheel();
        SetPochiPochi();
        SetActualSize();
        SetButtonCommand();

        _ = SetMouseTranslationAsync();
        _ = SetDirectionSelectionModeAsync();
        _ = SetDragRectAsync();
    }

    private void SetButtonCommand()
    {
        Observable.FromEventHandler<RoutedEventArgs>(
            h => canvasAdjustButton.Click += h,
            h => canvasAdjustButton.Click -= h)
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .Subscribe(param => VM?.AdjustAffine());

        Observable.FromEventHandler<RoutedEventArgs>(
            h => scaleUpButton.Click += h,
            h => scaleUpButton.Click -= h)
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Subscribe(param => VM?.Scale(KWRPConstants.C_SCALE_UP));

        Observable.FromEventHandler<RoutedEventArgs>(
            h => scaleDownButton.Click += h,
            h => scaleDownButton.Click -= h)
            .ThrottleFirst(TimeSpan.FromMilliseconds(100))
            .Subscribe(param => VM?.Scale(KWRPConstants.C_SCALE_DOWN));
    }

    void SetActualSize()
    {
        var onSizeChanged = Observable.FromEventHandler<SizeChangedEventArgs>(
            h => this.SizeChanged += h,
            h => this.SizeChanged -= h);

        onSizeChanged
            .Debounce(TimeSpan.FromMilliseconds(500))
            .Subscribe(param =>
            {
                var sz = param.e.NewSize;
                VM?.SetSize((int)sz.Height, (int)sz.Width);
            });
    }

    private async Task SetDragRectAsync(CancellationToken cts = default)
    {
        var mouseDown = Observable.FromEventHandler<PointerPressedEventArgs>(
            h => subGrid.PointerPressed += h,
            h => subGrid.PointerPressed -= h,
            cts);

        var mouseUp = Observable.FromEventHandler<PointerReleasedEventArgs>(
            h => subGrid.PointerReleased += h,
            h => subGrid.PointerReleased -= h,
            cts);

        var mouseMove = Observable.FromEventHandler<PointerEventArgs>(
            h => subGrid.PointerMoved += h,
            h => subGrid.PointerMoved -= h,
            cts);

        bool isDragging = false;

        mouseDown
            .Where(param => param.e.GetCurrentPoint(mainCanvas).Properties.IsRightButtonPressed)
            .Select(param => param.e.GetPosition(mainCanvas))
            .Subscribe(p =>
            {
                isDragging = true;
                VM?.SetDragRectAt(p.X, p.Y);
            });

        mouseUp
            .Where(_ => isDragging)
            .Select(param => param.e.GetPosition(mainCanvas))
            .Subscribe(p =>
            {
                isDragging = false;
                VM?.ReleaseDragRectAt(p.X, p.Y);
            });

        var drag = mouseDown
            .Where(param => param.e.GetCurrentPoint(mainCanvas).Properties.IsRightButtonPressed)
            .SelectMany(_ => mouseMove)
            .TakeUntil(mouseUp)
            .ThrottleFirst(TimeSpan.FromMilliseconds(10))
            .Select(param => param.e.GetPosition(mainCanvas));

        while (!cts.IsCancellationRequested)
        {
            await drag
                .ForEachAsync(p =>
                {
                    VM?.UpdateDragRect(p.X, p.Y);
                });
        }
    }

    private async Task SetDirectionSelectionModeAsync(CancellationToken cts = default)
    {
        var mouseDown = Observable.FromEventHandler<PointerPressedEventArgs>(
            h => directionSelectionCanvas.PointerPressed += h,
            h => directionSelectionCanvas.PointerPressed -= h,
            cts);

        var mouseUp = Observable.FromEventHandler<PointerReleasedEventArgs>(
            h => directionSelectionCanvas.PointerReleased += h,
            h => directionSelectionCanvas.PointerReleased -= h,
            cts);

        var mouseMove = Observable.FromEventHandler<PointerEventArgs>(
            h => directionSelectionCanvas.PointerMoved += h,
            h => directionSelectionCanvas.PointerMoved -= h,
            cts);

        bool isDragging = false;

        mouseDown
            .Where(param => param.e.GetCurrentPoint(directionSelectionCanvas).Properties.IsLeftButtonPressed)
            .Select(param => param.e.GetPosition(directionSelectionCanvas))
            .Subscribe(p =>
            {
                isDragging = true;
                VM?.SetDirectionAt(p.X, p.Y);
            });

        mouseUp
            .Where(_ => isDragging)
            .Select(param => param.e.GetPosition(directionSelectionCanvas))
            .SubscribeAwait(async (p, ct) =>
            {
                isDragging = false;
                await VM?.ReleaseDirectionAt(p.X, p.Y);
            });


        var drag = mouseDown
            .Where(param => param.e.GetCurrentPoint(directionSelectionCanvas).Properties.IsLeftButtonPressed)
            .SelectMany(_ => mouseMove)
            .TakeUntil(mouseUp)
            .ThrottleFirst(TimeSpan.FromMilliseconds(10))
            .Select(param => param.e.GetPosition(directionSelectionCanvas));

        while (!cts.IsCancellationRequested)
        {
            await drag
                .ForEachAsync(p =>
                {
                    VM?.UpdateDirectionArrow(p.X, p.Y);
                });
        }
    }


    private async Task SetMouseTranslationAsync(CancellationToken cts = default)
    {
        var mouseDown = Observable.FromEventHandler<PointerPressedEventArgs>(
            h => subGrid.PointerPressed += h,
            h => subGrid.PointerPressed -= h,
            cts);

        var mouseUp = Observable.FromEventHandler<PointerReleasedEventArgs>(
            h => subGrid.PointerReleased += h,
            h => subGrid.PointerReleased -= h,
            cts);

        var mouseMove = Observable.FromEventHandler<PointerEventArgs>(
            h => subGrid.PointerMoved += h,
            h => subGrid.PointerMoved -= h,
            cts);

        var mouseLeave = Observable.FromEventHandler<PointerEventArgs>(
            h => subGrid.PointerExited += h,
            h => subGrid.PointerExited -= h,
            cts);

        var drag = mouseDown
            .Where(param =>
            {
                var point = param.e.GetCurrentPoint(param.sender as Control);
                return point.Properties.IsLeftButtonPressed;
            })
            .SelectMany(_ => mouseMove)
            .TakeUntil(mouseUp)
            //.TakeUntil(mouseLeave)
            .ThrottleFirst(TimeSpan.FromMilliseconds(15))
            .Select(e => e.e.GetPosition(subGrid));

        while (!cts.IsCancellationRequested)
        {
            await drag
                .Zip(drag.Skip(1), (x, y) => new { Prev = x, Next = y })
                .ForEachAsync(e =>
                {
                    var dx = e.Next.X - e.Prev.X;
                    var dy = e.Next.Y - e.Prev.Y;

                    VM?.PointerTranslate(dx, dy);
                });
        }
    }

    private void SetMouseWheel()
    {
        var mouseWheel = Observable.FromEventHandler<PointerWheelEventArgs>(
            h => subGrid.PointerWheelChanged += h,
            h => subGrid.PointerWheelChanged -= h);

        mouseWheel.Subscribe(param =>
        {
            var pos = param.e.GetPosition(mainCanvas);
            var scale = param.e.Delta.Y > 0 ? KWRPConstants.C_SCALE_UP : KWRPConstants.C_SCALE_DOWN;
            VM?.PointerScaleAt(scale, pos.X, pos.Y);
        });
    }

    private void SetMousePosition()
    {
        var mouseMove = Observable.FromEventHandler<PointerEventArgs>(
            h => subGrid.PointerMoved += h,
            h => subGrid.PointerMoved -= h);

        mouseMove.Subscribe(param =>
        {
            var pos = param.e.GetPosition(mainCanvas);
            mousePosition.Text = $"x: {pos.X:f2}, y: {pos.Y:f2}";
        });
    }

    public void SetPochiPochi()
    {
        var mouseDown = Observable.FromEventHandler<PointerPressedEventArgs>(
            h => subGrid.PointerPressed += h,
            h => subGrid.PointerPressed -= h);

        var mouseUp = Observable.FromEventHandler<PointerReleasedEventArgs>(
            h => subGrid.PointerReleased += h,
            h => subGrid.PointerReleased -= h);

        var mouseLeave = Observable.FromEventHandler<PointerEventArgs>(
            h => subGrid.PointerExited += h,
            h => subGrid.PointerExited -= h);

        var clearRequested = Observable.FromEventHandler<RoutedEventArgs>(
            h => rulerClearButton.Click += h,
            h => rulerClearButton.Click -= h);

        long threshold = 1500000;

        clearRequested
            .Subscribe(_ =>
            {
                System.Diagnostics.Debug.WriteLine("clear");
                VM?.ClearPochi();
            });

        mouseDown
            .Where(_ => rulerToggleButton.IsChecked ??= false)
            .Timestamp()
            .SelectMany(downValue =>
                mouseUp
                    .Take(1)
                    .TakeUntil(mouseLeave)
                    .Timestamp()
                    .Select(upValue => new
                    {
                        DownPos = downValue.Value.e.GetPosition(mainCanvas),
                        DownDuration = upValue.Timestamp - downValue.Timestamp,
                    })
            )
            .Where(val => val.DownDuration < threshold)
            .Select(e => e.DownPos)
            .Subscribe(pos =>
            {
                System.Diagnostics.Debug.WriteLine(pos);
                VM?.AddPochi(pos.X, pos.Y);
            });

    }
}