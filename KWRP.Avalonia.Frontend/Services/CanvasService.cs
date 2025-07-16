using Avalonia;
using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using System;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services
{
    public class CanvasService : ICanvasService
    {
        private readonly CanvasStateStore _canvasStateStore;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ILogService _logService;
        
        public CanvasService(
            CanvasStateStore canvasStateStore,
            ILogService logService,
            CanvasItemStore canvasItemStore)
        {
            _canvasStateStore = canvasStateStore;
            _logService = logService;
            _canvasItemStore = canvasItemStore;

            _logService.LogDebug("init");
        }

        public void AdjustAffine()
        {
            if (_canvasItemStore.AreaBoundingBox.Value == null)
            {
                _logService.LogDebug("BoundingBoxがnull");
                ResetAffine();
                return;
            }

            var windowHeight = _canvasStateStore.WindowHeight;
            var windowWidth = _canvasStateStore.WindowWidth;

            var currentBox = _canvasItemStore.AreaBoundingBox.Value;
            var currentRot = _canvasStateStore.Rot.CurrentValue;
            var targetBoundingBox = currentBox
                .ToPolygon()
                .Translate(-currentBox.Center.X, -currentBox.Center.Y)
                .Rotate(-currentRot.RadianValue)
                .Translate(currentBox.Center.X, currentBox.Center.Y)
                .AaBB;

            if (targetBoundingBox.Height < KWRPConstants.C_EPS
                || targetBoundingBox.Width < KWRPConstants.C_EPS
                || windowHeight * windowWidth < KWRPConstants.C_EPS)
            {
                _logService.LogDebug("BoundingBoxがinvalid");
                ResetAffine();
                return;
            }

            var ratio = Math.Min(windowWidth / targetBoundingBox.Width, windowHeight / targetBoundingBox.Height);

            _logService.LogDebug($"ratio: {ratio}, trans: {(-targetBoundingBox.Xmin, -targetBoundingBox.Ymin)}");
            ResetAffine();
            ScaleAt(ratio, 0, 0);
            Translate(-targetBoundingBox.Xmin * ratio, -targetBoundingBox.Ymin * ratio);
            Rotate(currentRot.DegreeValue);
        }

        public void Translate(double offsetX, double offsetY) => _canvasStateStore.Affine.Value *= Matrix.CreateTranslation(offsetX, offsetY);

        public void ScaleAt(double scale, double centerX, double centerY) 
        {
            var mat = new Matrix(scale, 0, 0, scale, centerX - (scale * centerX), centerY - (scale * centerY));
            _canvasStateStore.Affine.Value = mat * _canvasStateStore.Affine.Value;
        }

        public void Rotate(double angleDegree)
        {
            Vec2 center = _canvasItemStore.AreaBoundingBox.Value?.Center ?? new Vec2(0, 0);
            RotateAt(angleDegree, center.X, center.Y);
        }

        void RotateAt(double angleDegree, double centerX, double centerY)
        {
            var mat = Matrix.CreateTranslation(-centerX, -centerY)
                * Matrix.CreateRotation(angleDegree * Math.PI / 180.0)
                * Matrix.CreateTranslation(centerX, centerY);

            _canvasStateStore.Affine.Value = mat * _canvasStateStore.Affine.Value;
        }

        public void ResetAffine() => _canvasStateStore.Affine.Value = Matrix.Identity;
        

        public void SetWindowSize(int height, int width)
        {
            _canvasStateStore.WindowHeight = height;
            _canvasStateStore.WindowWidth = width;
        }

        public void SetCanvasSize(int height, int width)
        {
            _canvasStateStore.CanvasHeight.Value = height;
            _canvasStateStore.CanvasWidth.Value = width;
        }
    }
}
