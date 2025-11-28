using KWRP.Backend.Services;
using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Modlules.LaneIntegration
{
    public class NaiveDfsLaneIntegrator
    {
        public static IEnumerable<Polygon[]> Integrate(
            IEnumerable<Polygon> orthogonalLanes, 
            int minPairCount, 
            int maxPairCount,
            ILanguageService? _languageService=null)
        {
            // 隣接リストを作る
            var lanes = orthogonalLanes
                .OrderBy(lane => lane.AaBB.Xmin)
                .ToArray();
            int N = lanes.Length;

            if (N > KWRPConstants.C_LANE_COUNT_LIMIT)
            {
                throw new ArgumentException(
                    String.Format(
                        _languageService?.GetString("Domain.Back.TooManyLanesAbort") 
                        ?? $"レーンの数が多すぎるので処理を中止します。{0} > {1}", 
                        N, KWRPConstants.C_LANE_COUNT_LIMIT));
            }

            var adjacentyList = Enumerable.Range(0, N).Select(_ => new List<int>()).ToArray();
            var degreeIn = new int[N];
            var degreeOut = new int[N];
            for (int i = 0; i < N; ++i)
            {
                for (int j = i + 1; j < N; ++j)
                {
                    if (lanes[i].AaBB.IsIntersect(lanes[j].AaBB))
                    {
                        adjacentyList[i].Add(j);
                        degreeOut[i]++;
                        degreeIn[j]++;
                    }
                }
                // 面積の大きいレーンを優先的にペアにしたいので
                adjacentyList[i].Sort((a, b) => lanes[a].Area.CompareTo(lanes[b].Area));
            }

            // 深さ優先探索(とりあえず非再帰)でペアリング用のグループを作る
            var visited = new bool[N];
            var stack = new Stack<int>();
            var starters = Enumerable
                .Range(0, N)
                .Where(i => degreeIn[i] == 0)
                .OrderBy(i => lanes[i].Area);
            foreach (var start in starters)
            {
                stack.Push(~start);
                stack.Push(start);
            }

            var temporalLaneIds = new List<int>();
            var LaneIdGroups = new List<int[]>();
            while (stack.Count > 0)
            {
                int now = stack.Pop();
                if (now >= 0)
                {
                    temporalLaneIds.Add(now);
                    foreach (var next in adjacentyList[now])
                    {
                        if (visited[next]) 
                            continue;
                        stack.Push(~next);
                        stack.Push(next);
                    }
                }
                else
                {
                    if (temporalLaneIds.Count == 0) 
                        continue;
                    LaneIdGroups.Add(temporalLaneIds.ToArray());
                    foreach (var id in temporalLaneIds)
                    {
                        visited[id] = true;
                    }
                    temporalLaneIds.Clear();
                }
            }

            // 各グループについて上限下限に従ってペアリングを行う
            foreach (var group in LaneIdGroups)
            {
                int len = group.Length;
                if (len < minPairCount)
                {
                    // レーンが少ないのでスキップ
                    continue;
                }

                var (q, r) = Math.DivRem(len, maxPairCount);
                if (r == 0 || r >= minPairCount)
                {   // そのままペアできる
                    for (int i = 0; i < len; i += maxPairCount)
                    {
                        var chunk = Enumerable.Range(i, Math.Min(maxPairCount, len - i))
                            .Select(id => lanes[group[id]])
                            .ToArray();
                        yield return chunk;
                    }
                }
                else
                {   // 調整する
                    var chunkCounts = new int[q + 1];
                    for (int i = 0; i < q; ++i)
                    {
                        chunkCounts[i] = maxPairCount;
                    }
                    chunkCounts[q] = r;

                    for (int i = q - 1; i >= 0; --i)
                    {
                        int need = minPairCount - chunkCounts[q];
                        int available = chunkCounts[i] - minPairCount;
                        if (need == 0 || available == 0) break;
                        if (available > 0)
                        {
                            chunkCounts[q] += Math.Min(available, need);
                            chunkCounts[i] -= Math.Min(available, need);
                        }
                    }

                    int now = 0;
                    foreach (var count in chunkCounts)
                    {
                        if (count < minPairCount) continue;
                        var chunk = Enumerable.Range(now, count)
                            .Select(id => lanes[group[id]])
                            .ToArray();
                        yield return chunk;
                        now += count;
                    }
                }
            }
        }

    }
}
