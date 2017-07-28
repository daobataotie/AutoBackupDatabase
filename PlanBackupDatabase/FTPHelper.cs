using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace PlanBackupDatabase
{
    internal class FTPHelper
    {
        string userName;
        string password;
        FtpWebRequest fwr;
        string ftpUrl;

        public FTPHelper(string url, string userName, string password)
        {
            this.userName = userName;
            this.password = password;
            ftpUrl = url;
        }

        public Dictionary<string, long> GetFileNames()
        {
            fwr = FtpWebRequest.Create(ftpUrl) as FtpWebRequest;
            fwr.UseBinary = true;
            fwr.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            fwr.Credentials = new NetworkCredential(userName, password);
            WebResponse response = fwr.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string line = reader.ReadLine();

            List<string> tempList = new List<string>();
            Dictionary<string, long> list = new Dictionary<string, long>();
            while (!string.IsNullOrEmpty(line) && !line.Contains("<DIR>")) //文件夹会包含 <DIR>
            {
                string fileName = line.Substring(39);      //文件名是从39的索引开始
                long size = long.Parse(line.Substring(0, 39).Substring(line.Substring(0, 39).Trim().LastIndexOf(' ')).Trim());

                list.Add(fileName, size);
                line = reader.ReadLine();
            }
            reader.Close();
            response.Close();

            return list;
        }

        public long GetFileSize(string fileName)
        {
            fwr = FtpWebRequest.Create(ftpUrl + fileName) as FtpWebRequest;
            fwr.UseBinary = true;
            fwr.Method = WebRequestMethods.Ftp.GetFileSize;
            fwr.Credentials = new NetworkCredential(userName, password);
            FtpWebResponse response = fwr.GetResponse() as FtpWebResponse;
            long length = response.ContentLength;

            response.Close();

            return length;
        }

        public void DownloadFile(string fileName, string localPath)
        {
            try
            {
                fwr = FtpWebRequest.Create(ftpUrl + "//" + fileName) as FtpWebRequest;
                fwr.UseBinary = true;
                fwr.Method = WebRequestMethods.Ftp.DownloadFile;
                fwr.Credentials = new NetworkCredential(userName, password);
                WebResponse response = fwr.GetResponse();

                Stream stream = response.GetResponseStream();
                int bufferSize = 2;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = stream.Read(buffer, 0, bufferSize);
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
                {
                    string logFileName = localPath + "//log.txt";
                    File.AppendAllText(logFileName, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} : {ex.Message}\r\n");
                }
                else
                {
                    DownloadFile(fileName, localPath);
                }
            }
        }
    }
}