using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Modules.LaneArrangement
{
    /// <summary>
    /// 進捗方向は右であると仮定して、レーンをいい感じに配置する
    /// 生成されるレーンはorthogonal（短辺がX軸平行、長辺がY軸に平行）
    /// </summary>
    public class NaiveLaneArranger
    {
        private readonly List<BoundingBox> _lanes = [];
        private readonly List<(double XL, double XR)> _xranges = [];

        public IEnumerable<(double XL, double XR)> XRanges => _xranges;
        public IEnumerable<BoundingBox> Lanes => _lanes;
        public PolygonWithHoles TargetPolygon { get; }


        public NaiveLaneArranger(PolygonWithHoles targetPolygon)
        {
            TargetPolygon = targetPolygon;
        }

        public void CreateInscribeLanes(double laneWidth, double lapWidth)
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
            if (2 * lapWidth > laneWidth)
            {
                throw new ArgumentException($"レーンラップ幅({nameof(lapWidth)})の値が鉄倫幅({nameof(laneWidth)})に対して大きいため、レーン割を中止します");
            }

            _lanes.Clear();

            // めちゃくちゃナイーブな実装
            for (double xL = TargetPolygon.AaBB.Xmin + 0.01; xL + laneWidth <= TargetPolygon.AaBB.Xmax; xL = xL + laneWidth - lapWidth)
            {
                try
                {
                    var xR = xL + laneWidth;

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
                        throw new Exception($"区間({xL:f3}, {xR:f3})で交点が奇数になっています");
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
