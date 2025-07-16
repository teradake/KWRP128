using Avalonia;
using KWRP.Avalonia.Backend.Services;
using R3;
using System;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class CanvasStateStore
    {
        private readonly ILogService _logService;

        public CanvasStateStore(
            ILogService logService)
        {
            _logService = logService;

            Scale = Affine
                .ThrottleFirstLast(TimeSpan.FromMilliseconds(50))
                .Select(mat => Math.Sqrt(mat.M11 * mat.M11 + mat.M12 * mat.M12))
                .ToReadOnlyReactiveProperty(1.0);

            Rot = Affine
                .ThrottleFirstLast(TimeSpan.FromMilliseconds(50))
                .Select(mat => Direction2.ByRadian(Math.Atan2(-mat.M21, mat.M11)))
                .ToReadOnlyReactiveProperty(Direction2.ByDegree(0));

            _logService.LogDebug("init");
        }


        public ReactiveProperty<bool> IsDirectionSelectionMode { get; } = new(false);
        public ReactiveProperty<bool> IsOptimizing { get; } = new(false);
        public int WindowHeight { get; set; } = 400;
        public int WindowWidth { get; set; } = 400;
        public ReactiveProperty<int> CanvasHeight { get; } = new(200);
        public ReactiveProperty<int> CanvasWidth { get; } = new(200);

        public ReactiveProperty<Matrix> Affine { get; } = new(Matrix.Identity);
        public ReadOnlyReactiveProperty<Direction2> Rot { get; }
        public ReadOnlyReactiveProperty<double> Scale { get; }

        
    }
}