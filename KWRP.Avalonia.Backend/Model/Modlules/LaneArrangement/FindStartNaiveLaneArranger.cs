using KWRP.Avalonia.Backend.Services;
using KWRP.Backend.Services;
using System.ComponentModel.DataAnnotations;
using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Modules.LaneArrangement
{
    /// <summary>
    /// 進捗方向は右であると仮定して、レーンをいい感じに配置する
    /// 生成されるレーンはorthogonal（短辺がX軸平行、長辺がY軸に平行）
    /// </summary>
    public class FindStartNaiveLaneArranger
    {
        private readonly ILogService? _logService;
        private readonly ILanguageService? _languageService;
        private readonly List<BoundingBox> _lanes = [];
        private readonly List<(double XL, double XR)> _xranges = [];

        public IEnumerable<(double XL, double XR)> XRanges => _xranges;
        public IEnumerable<BoundingBox> Lanes => _lanes;
        public PolygonWithHoles TargetPolygon { get; }


        public FindStartNaiveLaneArranger(PolygonWithHoles targetPolygon, ILogService? logService = null, ILanguageService? languageService = null)
        {
            TargetPolygon = targetPolygon;
            _logService = logService;
            _languageService = languageService;
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
                throw new ArgumentException(_languageService?.GetString("Domain.Back.WheelWidthGreaterThanZero") ?? "鉄輪幅は0より大きい値としてください");
            }
            if (lapWidth <= 0)
            {
                throw new ArgumentException(_languageService?.GetString("Domain.Back.LaneLapWidthGreaterThanZero") ?? "レーンラップ幅は0より大きい値としてください");
            }
            if (Utils.IsPositive(2 * lapWidth - laneWidth))
            {
                throw new ArgumentException(_languageService?.GetString("Domain.Back.LaneLapWidthShorterThanDoubleWheelWidth") ?? "レーンラップ幅は鉄輪幅×2より短い値としてください");
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
                if (diff > 0 && m > 1 && diff / (m - 1) < lapWidth * 0.25)
                {
                    _logService?.LogDebug(
                        String.Format(
                            _languageService?.GetString("Domain.Back.LapAdjustment")
                            ?? $"ラップ調整: {0} -> {1}",
                            lapWidth, lapWidth + diff / (m - 1)));
                    lapWidth += diff / (m - 1);
                }
            }


            // レーン生成位置X座標を前計算
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
                    var yL = new List<double>();
                    var yR = new List<double>();
                    foreach (var seg in TargetPolygon.GetSegments())
                    {
                        var intersectionL = seg.GetVerticalLineIntersection(xL);
                        if (intersectionL != null)
                        {
                            yL.Add(intersectionL.Y);
                        }

                        var intersectionR = seg.GetVerticalLineIntersection(xR);
                        if (intersectionR != null)
                        {
                            yR.Add(intersectionR.Y);
                        }
                    }

                    if (yL.Count % 2 != 0 || yR.Count % 2 != 0)
                    {
                        throw new Exception(
                            String.Format(
                                _languageService?.GetString("Domain.Back.OddIntersectionInSection")
                                ?? $"区間({0:f3}, {1:f3})で交点が奇数になっています",
                                xL, xR));
                    }

                    var stackL = new Stack<(double a, double b)>();
                    var stackR = new Stack<(double a, double b)>();
                    yL.Sort();
                    yR.Sort();
                    for (int i = 0; i < yL.Count; i += 2) stackL.Push((yL[i], yL[i + 1]));
                    for (int i = 0; i < yR.Count; i += 2) stackR.Push((yR[i], yR[i + 1]));

                    bool created = false;
                    while (stackL.Count > 0 && stackR.Count > 0)
                    {
                        var ymin = Math.Max(stackL.Peek().a, stackR.Peek().a);
                        var ymax = Math.Min(stackR.Peek().b, stackL.Peek().b);
                        if (Utils.IsPositive(ymax - ymin))
                        {
                            _lanes.Add(new BoundingBox
                            {
                                Xmin = xL,
                                Xmax = xR,
                                Ymin = ymin,
                                Ymax = ymax,
                            });
                            created = true;
                        }
                        if (stackL.Peek().a > stackR.Peek().a)
                        {
                            stackL.Pop();
                        }
                        else
                        {
                            stackR.Pop();
                        }
                    }

                    if (created)
                    {
                        _xranges.Add((xL, xR));
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

    }
    
}
