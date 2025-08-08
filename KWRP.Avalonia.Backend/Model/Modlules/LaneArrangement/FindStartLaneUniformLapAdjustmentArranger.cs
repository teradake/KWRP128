using KWRP.Avalonia.Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Modules.LaneArrangement
{
    /// <summary>
    /// 進捗方向は右であると仮定して、レーンをいい感じに配置する
    /// 生成されるレーンはorthogonal（短辺がX軸平行、長辺がY軸に平行）
    /// </summary>
    public class FindStartLaneUniformLapAdjustmentArranger
    {
        record CrossPoint(Vec2 point, int segId);

        enum ZoneType { Left, Right, Vertical }
        record NGZone(double ymin, double ymax, ZoneType type);


        private readonly ILogService? _logService;
        private readonly List<BoundingBox> _lanes = [];
        private readonly List<(double XL, double XR)> _xranges = [];

        public IEnumerable<(double XL, double XR)> XRanges => _xranges;
        public IEnumerable<BoundingBox> Lanes => _lanes;
        public PolygonWithHoles TargetPolygon { get; }


        public FindStartLaneUniformLapAdjustmentArranger(PolygonWithHoles targetPolygon, ILogService? logService = null)
        {
            TargetPolygon = targetPolygon;
            _logService = logService;
        }

        public void CreateInscribeLanes(
            double laneWidth, 
            double lapWidth, 
            double laneMinLength, 
            double leftOffset,
            double rightOffset,
            bool adjustLapWidth = true)
        {
            // validate
            if (laneWidth <= 0)
            {
                throw new ArgumentException($"鉄倫幅({nameof(laneWidth)})は0より大きい値としてください");
            }
            if (lapWidth <= 0)
            {
                throw new ArgumentException($"レーンラップ幅({nameof(lapWidth)})は0より大きい値としてください");
            }
            if (Utils.IsPositive(2 * lapWidth - laneWidth))
            {
                throw new ArgumentException($"レーンラップ幅({nameof(lapWidth)})は 2 x 鉄輪幅より短い値としてください");
            }

            
            // 最小レーン長が配置できる最左座標を事前に計算する
            var startX = TargetPolygon.FindStartingPosition(laneMinLength);
            var endX = TargetPolygon.AaBB.Xmax - rightOffset;

            // レーン割スタート位置調整
            {
                startX = Math.Clamp(startX, TargetPolygon.AaBB.Xmin, endX);
                startX += leftOffset;
                startX += 0.01;     // 数値誤差対策。レーン割がいい感じになる
            }
            double len = endX - startX;

            /// ラップ調整値を計算 
            /// ① LaneWidth * M - LapWidth * (M - 1) <= len の式から
            /// lenから0m以上はみ出すのに必要なレーン本数Mが求まるので ②M := (len - LapWidth) / (Lana - Lap)
            /// Mを①に代入することで調整すべき値 diffが計算できる
            if (adjustLapWidth)
            {
                int m = (int)Math.Ceiling((len - lapWidth) / (laneWidth - lapWidth));
                var tempLen = m * laneWidth - (m - 1) * lapWidth;
                var diff = tempLen - len + 0.01;
                if (diff > 0 && m > 1 && diff / (m - 1) < lapWidth * 0.2)
                {
                    _logService?.LogDebug($"ラップ調整: {lapWidth} -> {lapWidth + diff / (m - 1)}");
                    lapWidth += diff / (m - 1);
                }
            }


            // レーン生成位置X座標を前計算（高速化が必要になったらこれ利用して平面走査。とりあえず愚直に）
            var laneSidePosition = new List<(double xL, double xR)>();
            for (double xL = 0; ; xL += laneWidth - lapWidth)
            {
                double xR = xL + laneWidth;
                if (Utils.IsPositive(xR - len))
                {
                    System.Diagnostics.Debug.WriteLine(xR - len);
                    break;
                }
                laneSidePosition.Add((startX + xL, startX + xR));
            }

            // レーン生成
            _lanes.Clear();
            foreach (var (xL, xR) in  laneSidePosition) 
            {
                try
                {
                    var ngZones = new List<NGZone>();

                    foreach (var poly in TargetPolygon.Holes.Append(TargetPolygon.Shell))
                    {
                        var segments = poly.GetSegments().ToArray();

                        // 直線x = xlとpolygonの交点を取得
                        var crossPointsL = segments
                            .Select((seg, index) => new { intersect = seg.GetVerticalLineIntersection(xL), segId = index })
                            .Where(item => item.intersect != null)
                            .Select(item => new CrossPoint(item.intersect!, item.segId));

                        var chunkL = new List<CrossPoint>();
                        foreach (var cp in crossPointsL)
                        {
                            if (chunkL.Count > 0 && chunkL.Last().point.Equals(cp.point))
                            {
                                if (segments[chunkL.Last().segId].ToRight ^ segments[cp.segId].ToRight)
                                {
                                    chunkL.RemoveAt(chunkL.Count - 1);
                                }
                                continue;
                            }
                            chunkL.Add(cp);
                        }

                        if (chunkL.Count % 2 != 0)
                        {
                            _logService?.LogWarn($"x=xL(={xL:f3})で交点が奇数になっています。レーン割がうまくいかない可能性があります");
                            throw new Exception($"x=xR(={xL:f3})で交点が奇数になっています");
                        }

                        // 直線x = xrとpolygonの交点を取得
                        var crossPointsR = segments
                            .Select((seg, index) => new { intersect = seg.GetVerticalLineIntersection(xR), segId = index })
                            .Where(item => item.intersect != null)
                            .Select(item => new CrossPoint(item.intersect!, item.segId));

                        var chunkR = new List<CrossPoint>();
                        foreach (var cp in crossPointsR)
                        {
                            if (chunkR.Count > 0 && chunkR.Last().point.Equals(cp.point))
                            {
                                if (segments[chunkR.Last().segId].ToRight ^ segments[cp.segId].ToRight)
                                {
                                    chunkR.RemoveAt(chunkR.Count - 1);
                                }
                                continue;
                            }
                            chunkR.Add(cp);
                        }

                        if (chunkR.Count % 2 != 0)
                        {
                            _logService?.LogWarn($"x=xR(={xR:f3})で交点が奇数になっています。レーン割がうまくいかない可能性があります");
                            throw new Exception($"x=xR(={xR:f3})で交点が奇数になっています");
                        }

                        var crossPoints = crossPointsL
                            .Concat(crossPointsR)
                            .ToArray();

                        Array.Sort(crossPoints, (lhs, rhs) =>
                        {
                            if (lhs.segId == rhs.segId)
                            {
                                return segments[lhs.segId].ToRight
                                    ? lhs.point.X.CompareTo(rhs.point.X)
                                    : rhs.point.X.CompareTo(lhs.point.X);
                            }
                            return lhs.segId.CompareTo(rhs.segId);
                        });
                        
                        var starter = poly.IsClockwise
                            ? crossPointsR.MinBy(cp => cp.point.Y)
                            : crossPointsR.MaxBy(cp => cp.point.Y);
                        var startId = Array.IndexOf(crossPoints, starter);
                        if (startId == -1)
                        {
                            starter = poly.IsClockwise
                                ? crossPointsL.MaxBy(cp => cp.point.Y)
                                : crossPointsL.MinBy(cp => cp.point.Y);
                            startId = Array.IndexOf(crossPoints, starter);
                        }

                        for (int i = 0; i < crossPoints.Length; i += 2)
                        {
                            int p = (i + startId) % crossPoints.Length;
                            int q = (i + 1 + startId) % crossPoints.Length;
                            var ymin = Math.Min(crossPoints[p].point.Y, crossPoints[q].point.Y);
                            var ymax = Math.Max(crossPoints[p].point.Y, crossPoints[q].point.Y);
                            
                            int s = crossPoints[p].segId;
                            int t = crossPoints[q].segId;
                            while ((t - s) % segments.Length != 0)
                            {
                                s = (s + 1) % segments.Length;
                                ymin = Math.Min(ymin, segments[s].Src.Y);
                                ymax = Math.Max(ymax, segments[s].Src.Y);
                            }

                            ngZones.Add(new NGZone(ymin, ymax,
                                Utils.IsZero(crossPoints[p].point.X - crossPoints[q].point.X)
                                    ? ZoneType.Vertical
                                    : (crossPoints[p].point.X < crossPoints[q].point.X ? ZoneType.Right : ZoneType.Left)));
                        }
                    }

                    ngZones.Sort((lhs, rhs) =>
                    {
                        if (lhs.ymin.CompareTo(rhs.ymin) != 0)
                        {
                            return lhs.ymin.CompareTo(rhs.ymin);
                        }
                        return lhs.ymax.CompareTo(rhs.ymax);
                    });

                    bool laneStartFixed = false;
                    var laneRange = new List<(double ymin, double ymax)>();
                    double lower = -KWRPConstants.C_INF;
                    foreach (var zone in ngZones)
                    {
                        if (zone.type == ZoneType.Right)
                        {
                            laneStartFixed = true;
                            lower = Math.Max(lower, zone.ymax);
                        }
                        else if (zone.type == ZoneType.Left)
                        {
                            if (laneStartFixed && zone.ymin > lower)
                            {
                                laneRange.Add((lower, zone.ymin));
                            }
                            laneStartFixed = false;
                        }
                        else if (zone.type == ZoneType.Vertical)
                        {
                            if (!laneStartFixed) continue;
                            if (zone.ymin > lower)
                            {
                                laneRange.Add((lower, zone.ymin));
                            }
                            lower = Math.Max(lower, zone.ymax);
                        }
                    }

                    if (laneRange.Count == 0)
                    {
                        _logService?.LogDebug("no lane created");
                        continue;
                    }

                    // レーンの生成
                    bool created = false;
                    foreach (var range in laneRange)
                    {
                        _lanes.Add(new BoundingBox
                        {
                            Xmin = xL,
                            Xmax = xR,
                            Ymin = range.ymin,
                            Ymax = range.ymax,
                        });
                        created = true;
                    }

                    if (created)
                    {
                        _xranges.Add((xL, xR));
                    }
                    else
                    {
                        _logService?.LogDebug("no lane created");
                    }

                }
                catch (Exception e)
                {
                    throw new Exception($"区間[{xL}, {xR}]でレーン生成時にエラーが発生しました: {e.Message}", e);
                }
            }
        }
    }
    
}
