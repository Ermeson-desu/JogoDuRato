using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GameDuMouse.GameMain.Services
{
    public sealed class EditorService : IDisposable
    {
        private readonly string importedDir;
        private readonly ConcurrentQueue<string> pickedPaths = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> importedPaths = new ConcurrentQueue<string>();
        private int dialogOpen;
        private bool disposed;

        public EditorService()
        {
            importedDir = Path.GetFullPath(Path.Combine("Content", "Imported"));
            EnsureImportedDirectory();
        }

        public bool IsDialogOpen => Volatile.Read(ref dialogOpen) == 1;

        public void BeginPickImage()
        {
            if (disposed)
                return;

            if (Interlocked.Exchange(ref dialogOpen, 1) == 1)
                return;

            string psScript = "Add-Type -AssemblyName System.Windows.Forms; " +
                              "$ofd = New-Object System.Windows.Forms.OpenFileDialog; " +
                              "$ofd.Filter = 'Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg'; " +
                              "if($ofd.ShowDialog() -eq 'OK'){ Write-Output $ofd.FileName }";

            try
            {
                var psi = new ProcessStartInfo("powershell", "-NoProfile -Command " + psScript)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
                proc.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        pickedPaths.Enqueue(e.Data.Trim());
                };
                proc.Exited += (s, e) =>
                {
                    Interlocked.Exchange(ref dialogOpen, 0);
                    proc.Dispose();
                };

                proc.Start();
                proc.BeginOutputReadLine();
            }
            catch
            {
                Interlocked.Exchange(ref dialogOpen, 0);
            }
        }

        public bool TryConsumePickedImage(out string path)
        {
            return pickedPaths.TryDequeue(out path);
        }

        public void BeginImportImage(string sourcePath)
        {
            if (disposed || string.IsNullOrWhiteSpace(sourcePath))
                return;

            Task.Run(() =>
            {
                var imported = ImportImageInternal(sourcePath);
                if (!string.IsNullOrWhiteSpace(imported))
                    importedPaths.Enqueue(imported);
            });
        }

        public bool TryConsumeImportedImage(out string path)
        {
            return importedPaths.TryDequeue(out path);
        }

        public void BeginDeleteImportedIfUnused(string imagePath, List<string> remainingPaths)
        {
            if (disposed || string.IsNullOrWhiteSpace(imagePath))
                return;

            var snapshot = new List<string>();
            if (remainingPaths != null)
            {
                for (int i = 0; i < remainingPaths.Count; i++)
                {
                    var item = remainingPaths[i];
                    if (!string.IsNullOrWhiteSpace(item))
                        snapshot.Add(item);
                }
            }

            Task.Run(() => DeleteImportedIfUnusedInternal(imagePath, snapshot));
        }

        private string ImportImageInternal(string sourcePath)
        {
            try
            {
                string fullSource = Path.GetFullPath(sourcePath);
                if (!File.Exists(fullSource))
                    return null;

                EnsureImportedDirectory();

                if (IsUnderDirectory(fullSource, importedDir))
                    return fullSource;

                string fileName = Path.GetFileName(fullSource);
                if (string.IsNullOrWhiteSpace(fileName))
                    return null;

                string destPath = Path.Combine(importedDir, fileName);
                destPath = EnsureUniquePath(destPath);
                File.Copy(fullSource, destPath, false);
                return destPath;
            }
            catch
            {
                return null;
            }
        }

        private void DeleteImportedIfUnusedInternal(string imagePath, List<string> remainingPaths)
        {
            try
            {
                string fullPath = Path.GetFullPath(imagePath);
                if (!IsUnderDirectory(fullPath, importedDir))
                    return;

                if (remainingPaths != null)
                {
                    for (int i = 0; i < remainingPaths.Count; i++)
                    {
                        string other = remainingPaths[i];
                        if (string.IsNullOrWhiteSpace(other))
                            continue;

                        if (string.Equals(Path.GetFullPath(other), fullPath, StringComparison.OrdinalIgnoreCase))
                            return;
                    }
                }

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch
            {
                // ignore delete failures
            }
        }

        private void EnsureImportedDirectory()
        {
            if (!Directory.Exists(importedDir))
                Directory.CreateDirectory(importedDir);
        }

        private static bool IsUnderDirectory(string path, string directory)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(directory))
                return false;

            string normalizedDir = directory;
            if (!normalizedDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                normalizedDir += Path.DirectorySeparatorChar;

            return path.StartsWith(normalizedDir, StringComparison.OrdinalIgnoreCase);
        }

        private static string EnsureUniquePath(string path)
        {
            if (!File.Exists(path))
                return path;

            string dir = Path.GetDirectoryName(path) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            for (int i = 1; i < 1000; i++)
            {
                string candidate = Path.Combine(dir, name + "_" + i + ext);
                if (!File.Exists(candidate))
                    return candidate;
            }

            return Path.Combine(dir, name + "_" + Guid.NewGuid().ToString("N") + ext);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            while (pickedPaths.TryDequeue(out _)) { }
            while (importedPaths.TryDequeue(out _)) { }
        }
    }
}
