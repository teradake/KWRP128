using KWRP.Avalonia.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace KWRP.Infra.JSON
{
    public class JsonSerializerService<T> : ISerializer<T>
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // 日本語・記号もそのままエンコード
            ReadCommentHandling = JsonCommentHandling.Skip,        // コメント付きJSONもOK
            WriteIndented = true                                    // 整形出力
        };

        public async Task SaveAsync(T item, string filePath)
        {
            try
            {
                using var stream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(stream, item, _options);
            }
            catch (Exception ex)
            {
                throw new IOException($"jsonファイルの保存に失敗しました: {filePath}", ex);
            }
        }

        public async Task<T?> LoadAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("jsonファイルが見つかりません", filePath);
                }
                using var stream = File.OpenRead(filePath);
                return await JsonSerializer.DeserializeAsync<T>(stream, _options);
            }
            catch (JsonException ex)
            {
                throw new FormatException($"jsonファイルの読み込みに失敗しました（形式エラー）: {filePath}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"jsonファイルの読み込みに失敗しました: {filePath}", ex);
            }
        }

        public T? Load(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("jsonファイルが見つかりません", filePath);
                }
                string rawJson = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(rawJson);
            }
            catch (JsonException ex)
            {
                throw new FormatException($"jsonファイルの読み込みに失敗しました（形式エラー）: {filePath}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"jsonファイルの読み込みに失敗しました: {filePath}", ex);
            }
        }
    }
}
