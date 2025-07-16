using KWRP.Avalonia.Backend.Model.Shapes;
using System.Reflection.Metadata.Ecma335;
using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Services
{
    public interface ILaneArrangementService
    {
        void AllocateDirections(params double[] rotationDirectionsRadian);
        
        /// <summary>
        /// レーン割計算を実行。作業進捗方向を返す
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Task<double> ArrangeLanesAsync(PolygonWithHoles target, bool onlyCalculateDirection=false); 
    }
}
