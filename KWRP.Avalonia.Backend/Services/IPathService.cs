using KWRP.Avalonia.Backend.Enums;

namespace KWRP.Avalonia.Backend.Services
{
    public interface IPathService
    {
        Task<string?> GetOpenFilePathAsync(FileType fileType, string title = "ファイルを選択してください");
        Task<string?> GetSaveFilePathAsync(FileType fileType, string title = "保存するファイル名を指定してください");
        Task<string?> GetSaveFolderPathAsync(string title = "フォルダを選択してください");

        bool IsPathValid(string path);
        bool TryValidatePath(string path, out string? reason);

        void SelectFileInExplorer(string path);
    }
}
