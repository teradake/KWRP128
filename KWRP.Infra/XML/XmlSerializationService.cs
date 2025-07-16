using KWRP.Avalonia.Backend.Services;
using System.Xml;
using System.Xml.Serialization;

namespace KWRP.Infra.XML
{
    public class XmlSerializerService<T> : ISerializer<T>
    {
        public async Task SaveAsync(T item, string filePath)
        {
            try
            {
                await using var stream = File.Create(filePath);
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, item);
            }
            catch (Exception ex)
            {
                throw new IOException($"XMLファイルの保存に失敗しました: {filePath}", ex);
            }
        }

        public async Task<T?> LoadAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("XMLファイルが見つかりません", filePath);

                await using var stream = File.OpenRead(filePath);
                var serializer = new XmlSerializer(typeof(T));
                return (T?)serializer.Deserialize(stream);
            }
            catch (InvalidOperationException ex)
            {
                throw new FormatException($"XMLファイルの読み込みに失敗しました（形式エラー）: {filePath}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"XMLファイルの読み込みに失敗しました: {filePath}", ex);
            }
        }

        public T? Load(string filePath)
        {
            try
            {
                using var reader = XmlReader.Create(filePath);
                var serializer = new XmlSerializer(typeof(T));
                if (serializer.Deserialize(reader) is T item)
                {
                    return item;
                }
                throw new Exception();
            }
            catch (InvalidOperationException ex)
            {
                throw new FormatException($"XMLファイルの読み込みに失敗しました（形式エラー）: {filePath}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"XMLファイルの読み込みに失敗しました: {filePath}", ex);
            }
        }
    }

}
