using KWRP.Avalonia.Backend.Enums;

namespace KWRP.Avalonia.Backend.Services
{
    public interface IDataLoader
    {
        Task ExecuteLoadFromDialogAsync(FileType fileType);
        Task ExecuteLoadFromFileAsync(string filePath);
    }
}
