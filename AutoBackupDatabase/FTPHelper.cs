using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace AutoBackupDatabase
{
    public class FTPHelper
    {
        FtpWebRequest fwr;
        string ftpUrl;

        public FTPHelper(string url)
        {
            ftpUrl = url;
        }

        public List<string> GetFileNames()
        {
            fwr = FtpWebRequest.Create(ftpUrl) as FtpWebRequest;
            fwr.UseBinary = true;
            fwr.Method = WebRequestMethods.Ftp.ListDirectory;
            fwr.Credentials = new NetworkCredential("Administrator", "22wu32ap");
            WebResponse response = fwr.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string line = reader.ReadLine();

            List<string> list = new List<string>();
            while (!string.IsNullOrEmpty(line))
            {
                list.Add(line);
                line = reader.ReadLine();
            }
            response.Close();
            reader.Close();

            return list;
        }

        public void DownloadFile(string fileName, string localPath)
        {
            try
            {
                fwr = FtpWebRequest.Create(ftpUrl + fileName) as FtpWebRequest;
                fwr.UseBinary = true;
                fwr.Method = WebRequestMethods.Ftp.DownloadFile;
                fwr.Credentials = new NetworkCredential("Administrator", "22wu32ap");
                WebResponse response = fwr.GetResponse();

                Stream stream = response.GetResponseStream();
                int bufferSize = 1;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = stream.Read(buffer, 0, Convert.ToInt32(bufferSize));
                FileStream outputStream = new FileStream(localPath + "\\" + fileName, FileMode.Create);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, bufferSize);
                    readCount = stream.Read(buffer, 0, bufferSize);
                }

                stream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("远程服务器返回错误: (550) 文件不可用"))
                    throw new Exception(ex.Message);
            }
        }
    }
}
