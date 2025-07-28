using System;
using System.IO;
using System.Linq;
using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Services
{
    public class FileSystemCleaner : ICleaner
    {
        private readonly IFileDeletePolicy _fileDeletePolicy;
        private readonly IFolderDeletePolicy _folderDeletePolicy;

        public event EventHandler<Exception> TryDeleteFileException;
        public event EventHandler<Exception> TryDeleteFolderException;

        public FileSystemCleaner(IFileDeletePolicy fileDeletePolicy, IFolderDeletePolicy folderDeletePolicy)
        {
            _fileDeletePolicy = fileDeletePolicy;
            _folderDeletePolicy = folderDeletePolicy;
        }

        public void Cleanup(string rootDirectoryPath)
        {
            if (rootDirectoryPath == null || !Directory.Exists(rootDirectoryPath)) return;

            IOrderedEnumerable<string> dirs = Directory.GetDirectories(rootDirectoryPath, "*", SearchOption.AllDirectories).OrderByDescending(dir => dir.Length);

            foreach (var dir in dirs)
            {
                var files = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly);

                // 오래된 파일 삭제
                foreach (var file in files)
                {
                    if (_fileDeletePolicy.ShouldDeleteFile(file))
                    {
                        TryDeleteFile(file);
                    }
                }

                // 폴더가 오래되었고, 비어있다면 삭제
                if (_folderDeletePolicy.ShouldDeleteFolder(dir))
                {
                    TryDeleteFolder(dir);
                }
            }
        }

        private void TryDeleteFile(string file)
        {
            try { File.Delete(file); }
            catch (Exception ex) { TryDeleteFileException?.Invoke(this, ex); /* 경합/권한 문제 → 무시 */ }
        }
        private void TryDeleteFolder(string dir)
        {
            try { Directory.Delete(dir, false); }
            catch(Exception ex) { TryDeleteFolderException?.Invoke(this, ex); /* 경합/권한 문제 → 무시 */ }
        }
    }
}
