using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Services
{
    public interface ICanvasService
    {
        void AdjustAffine();
        void Translate(double offsetX, double offsetY);
        void ScaleAt(double scale, double centerX, double centerY);
        void ResetAffine();
        void SetWindowSize(int height, int width);
        void SetCanvasSize(int height, int width);
        void Rotate(double angleDegree);
    }
}
