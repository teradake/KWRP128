using KWRP.Avalonia.Backend.Model.SpatialDataXmlFormat;
using System;
using System.Linq;

namespace KWRP.Avalonia.Backend.Models.AutomatedConstructionAreaItems
{
    internal static class SpatialDataExtensions
    {
        //public static CompactionAreaItems ToCompactionAreaItems(this CompactionAreaItems compactionAreaItems)
        //{
        //    return compactionAreaItems;
        //}

        //public static CompactionAreaItems ToCompactionAreaItems(this AutomatedConstructionAreaItems acai)
        //{
        //    if (acai.VRData.WorkArea.Shell.Points.Count == 0)
        //    {
        //        throw new Exception("転圧領域データが含まれていません。処理を中止します");
        //    }


        //    try
        //    {
        //        var result = new CompactionAreaItems()
        //        {
        //            IsRollerHeadingRight = acai.VRData.IsRollerHeadingRight,
        //            LaneProgressDirection = acai.VRData.LaneProgressDirection,
        //        };

        //        result.WorkArea.Shell.Points
        //            = acai.VRData.WorkArea.Shell.Points
        //                .Select(p => new Point3D { X = p.X, Y = p.Y, Z = p.Z })
        //                .ToList();

        //        result.WorkArea.Holes
        //            = acai.VRData.WorkArea.Holes
        //                .Select(hole =>
        //                {
        //                    var points = hole
        //                        .Points
        //                        .Select(p => new Point3D { X = p.X, Y = p.Y, Z = p.Z })
        //                        .ToList();
        //                    return new Polyline3D { Points = points };
        //                })
        //                .ToList();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"{typeof(AutomatedConstructionAreaItems)}の変換に失敗しました", ex);
        //    }
        //}
    }
}
