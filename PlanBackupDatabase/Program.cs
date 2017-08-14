using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanBackupDatabase
{
    class Program
    {
        static string backupFolder = "F:\\Backup";
        static FTPHelper ftp;
        static void Main(string[] args)
        {
            //ftp = new FTPHelper("ftp://211.149.171.190", "Administrator", "22wu32ap");
            ftp = new FTPHelper("ftp://113.17.184.139/SQL Backup", "caorui", "caorui123", backupFolder);

            Sync();
        }

        private static void Sync()
        {
            Dictionary<string, long> fileNames = ftp.GetFileNames();
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
            Dictionary<string, long> localFiles = new Dictionary<string, long>();
            DirectoryInfo di = new DirectoryInfo(backupFolder);
            foreach (var item in di.GetFiles())
            {
                localFiles.Add(item.Name, item.Length);
            }

            List<string> downloadFiles = fileNames.Where(F => !localFiles.Any(f => f.Key.Contains(F.Key) && f.Value == F.Value)).Select(S => S.Key).ToList();
            downloadFiles.ForEach(F =>
            {
                if (File.Exists(backupFolder + "//" + F))
                    File.Delete(backupFolder + "//" + F);
                ftp.DownloadFile(F);
            });
        }
    }
}
