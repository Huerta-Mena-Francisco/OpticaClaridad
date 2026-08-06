// Models/BackupModel.cs
using System;

namespace OpticaClaridad.Models
{
    public class BackupModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreationDate { get; set; }
        public long FileSize { get; set; }
        public string DatabaseName { get; set; }
        public string DisplaySize => FormatFileSize(FileSize);

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    // Models/BackupModel.cs - MODIFICAR ESTA PARTE
    public class BackupSettings
    {
        // Cambiar la ruta por defecto a C:\Backups
        public string BackupPath { get; set; } = @"C:\Backups";
        public int RetentionDays { get; set; } = 30;
        public bool CompressBackups { get; set; } = true;
        public bool DailyAutoBackup { get; set; } = true;
        public TimeSpan BackupTime { get; set; } = new TimeSpan(2, 0, 0); // 2:00 AM

        // Nuevo: Agregar permisos de SQL Server
        public bool GrantSqlPermissions { get; set; } = true;
    }
}