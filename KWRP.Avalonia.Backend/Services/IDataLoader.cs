using KWRP.Avalonia.Backend.Enums;
using KWRP.Backend.Enums;

namespace KWRP.Avalonia.Backend.Services
{
    public interface IDataLoader
    {
        Task ExecuteLoadFromDialogAsync(FileType fileType);
        Task ExecuteLoadFromDialogAsync(FileType fileType, LengthUnitType lengthUnitType);
        Task ExecuteLoadFromFileAsync(string filePath);
    }
}
