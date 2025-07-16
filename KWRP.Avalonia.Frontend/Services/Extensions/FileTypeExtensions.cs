using Avalonia.Platform.Storage;
using KWRP.Avalonia.Backend.Enums;

namespace KWRP.Avalonia.Frontend.Services.Extensions
{
    internal static class FileTypeExtensions
    {
        public static FilePickerFileType CreateFilePickerType(this FileType fileType)
        {
            return fileType switch
            {
                FileType.XML => new FilePickerFileType("XML FILE") { Patterns = ["*.xml"] },
                FileType.JSON => new FilePickerFileType("JSON FILE") { Patterns = ["*.json"] },
                FileType.CSV => new FilePickerFileType("CSV FILE") { Patterns = ["*.csv"] },
                FileType.PNG => new FilePickerFileType("PNG FILE") { Patterns = ["*.png"] },
                FileType.DXF => new FilePickerFileType("DXF FILE") { Patterns = ["*.dxf"] },
                _ => throw new System.InvalidOperationException(),
            };
        }
    }
}
