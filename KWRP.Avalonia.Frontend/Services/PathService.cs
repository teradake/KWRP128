using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Services.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services
{
    /// <summary>
    /// パスの取得や妥当性チェックのためのクラス。妥当性はWindows依存なところがある。
    /// </summary>
    public class PathService : IPathService
    {
        private readonly Window _target;

        public PathService(Window target)
        {
            _target = target;
        }

        string ToPath(Uri uri) => uri.LocalPath + Uri.UnescapeDataString(uri.Fragment);

        public async Task<string?> GetOpenFilePathAsync(
            FileType fileType, 
            string title)
        {
            var filePickerType = fileType.CreateFilePickerType();
            var option = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = [filePickerType, ],
            };

            var files = await _target.StorageProvider.OpenFilePickerAsync(option);
            if (files == null || files.Count == 0)
            {
                return null;
            }

            return ToPath(files[0].Path);
        }

        public async Task<string?> GetSaveFilePathAsync(
            FileType fileType,
            string title)
        {
            var filePickerType = fileType.CreateFilePickerType();
            var option = new FilePickerSaveOptions
            {
                Title = title,
                FileTypeChoices = [filePickerType,],
                ShowOverwritePrompt = true,
            };

            var file = await _target.StorageProvider.SaveFilePickerAsync(option);
            if (file == null)
            {
                return null;
            }

            return ToPath(file.Path);
        }

        public async Task<string?> GetSaveFolderPathAsync(string title)
        {
            var option = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            };

            var folders = await _target.StorageProvider.OpenFolderPickerAsync(option);
            if (folders == null || folders.Count == 0)
            {
                return null;
            }

            return ToPath(folders[0].Path);
        }

        public bool IsPathValid(string path) => TryValidatePath(path, out _);

        

        public bool TryValidatePath(string path, out string? reason)
        {
            reason = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                reason = "パスが空です。";
                return false;
            }

            try
            {
                if (Directory.Exists(path))
                {
                    // ディレクトリは存在OK
                    return true;
                }

                if (File.Exists(path))
                {
                    // ファイルが存在する場合はロックされているか確認
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        // 読み取りOK → 使用されていない
                        return true;
                    }
                }
                else
                {
                    // 存在しないパス（保存時）→親ディレクトリの存在チェック
                    var dir = Path.GetDirectoryName(path);
                    if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                    {
                        reason = "保存先のディレクトリが存在しません。";
                        return false;
                    }
                    return true;
                }
            }
            catch (IOException)
            {
                reason = "ファイルが他のプロセスで使用されています。";
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                reason = "ファイルまたはディレクトリへのアクセス権がありません。";
                return false;
            }
            catch (Exception ex)
            {
                reason = $"不明なエラー: {ex.Message}";
                return false;
            }
        }

        public void SelectFileInExplorer(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || (!File.Exists(path) && !Directory.Exists(path)))
            {
                throw new FileNotFoundException("指定されたファイルが見つかりません", path);
            }
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: explorer.exe /select
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{path}\"",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                // フォールバック：フォルダを開く
                OpenFolder(Path.GetDirectoryName(path));
            }
        }

        /// <summary>
        /// 指定されたフォルダを開く
        /// </summary>
        /// <param name="folderPath">開くフォルダのパス</param>
        void OpenFolder(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("指定されたフォルダが見つかりません");
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("フォルダを開くことができませんでした", ex);
            }
        }
    }
}
