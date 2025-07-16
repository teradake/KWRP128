using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Services;
using KWRP.Infra.JSON;
using KWRP.Infra.XML;

namespace KWRP.Infra
{
    public static class SerializerFactory
    {
        public static ISerializer<T> Create<T>(FileType fileType)
        {
            return fileType switch
            {
                FileType.XML => new XmlSerializerService<T>(),
                FileType.JSON => new JsonSerializerService<T>(),
                _ => throw new InvalidOperationException($"{fileType}は有効な拡張子ではありません")
            };
        }
    }
}
