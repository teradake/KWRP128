using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using System.Globalization;
using System.Text;
using Trdk.Geometry;

namespace KWRP.Infra.CSV
{
    public class PolygonCsvParser : ISerializer<Polygon?>
    {
        const int C_MAX_REPORT_LINE_LENGTH = 30;
        private readonly ILogService? _logService;
        private readonly INotificationService? _notificationService;

        public PolygonCsvParser(ILogService logService, INotificationService notificationService)
        {
            _logService = logService;
            _notificationService = notificationService;
        }


        public Polygon? Load(string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<Polygon?> LoadAsync(string filePath)
        {
            try
            {
                var points = new List<Vec2>();
                var passedLineNumbers = new List<int>();

                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    int lineNumber = 0;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;

                        // 各行をカンマで分割
                        string[] values = line.Split(',');

                        // 座標xとyをパースする
                        if (double.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
                            double.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                        {
                            // MyPointオブジェクトを作成し、リストに追加
                            points.Add(new Vec2(x, y));
                        }
                        else if (lineNumber > 1)
                        {
                            passedLineNumbers.Add(lineNumber);
                        }
                    }
                }

                if (passedLineNumbers.Count > 0)
                {
                    _logService?.LogWarn($"ファイル「{filePath}」読み込み時に、以下の行の読込みができませんでした。フォーマットが正しいか確認してください\n" +
                        $"{string.Join(", ", passedLineNumbers.Take(C_MAX_REPORT_LINE_LENGTH).Select(s => $"row {s}"))}" +
                        (passedLineNumbers.Count > C_MAX_REPORT_LINE_LENGTH ? $"..." : ""));
                    _notificationService?.Notify(KWRPNotification.Create(
                        "CSVファイルの一部の行が読込めませんでした。ログを確認してください。", Avalonia.Backend.Enums.NotifyMessageType.Warn, 10));
                }

                return Polygon.AsCounterClockwise(points.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("CSVのパースに失敗しました\n", ex);
            }
        }


        public Task SaveAsync(Polygon? item, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
