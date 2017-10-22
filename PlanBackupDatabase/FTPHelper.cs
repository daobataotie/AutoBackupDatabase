using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Globalization;

namespace PlanBackupDatabase
{
    internal class FTPHelper
    {
        string userName;
        string password;
        string localPath;
        FtpWebRequest fwr;
        string ftpUrl;

        public FTPHelper(string url, string userName, string password, string localpath)
        {
            this.userName = userName;
            this.password = password;
            this.localPath = localpath;
            ftpUrl = url;
        }

        public Dictionary<string, long> GetFileNames()
        {
            try
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
                    string fileName = line.Substring(49);      //文件名是从39的索引开始，换了个服务器又是从49开始的   //-rw-r--r-- 1 ftp ftp        1260032 Aug 12 14:45 CRM_27_backup_2017_08_12_000002_9882893.bak
                    long size = long.Parse(line.Substring(0, 35).Substring(line.Substring(0, 35).Trim().LastIndexOf(' ')).Trim());
                    DateTime createDate = DateTime.ParseExact(line.Substring(0, 49).Substring(35).Trim(), "MMM dd HH:mm", CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None);
                    if ((DateTime.Now - createDate).Days > 30) //超过30天的备份文件删除
                    {
                        //DeleteFile(fileName);
                    }
                    else
                        list.Add(fileName, size);

                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();

                return list;
            }
            catch (Exception ex)
            {
                string logFileName = localPath + "//log.txt";
                File.AppendAllText(logFileName, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} : {ex.Message}\r\n");

                return new Dictionary<string, long>();
            }
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

        public void DownloadFile(string fileName)
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
                    DownloadFile(fileName);
                }
            }
        }

        public void DeleteFile(string fileName)
        {
            try
            {
                fwr = FtpWebRequest.Create(ftpUrl + "//" + fileName) as FtpWebRequest;
                fwr.UseBinary = true;
                fwr.Method = WebRequestMethods.Ftp.DeleteFile;
                fwr.Credentials = new NetworkCredential(userName, password);

                FtpWebResponse response = fwr.GetResponse() as FtpWebResponse;
                long size = response.ContentLength;
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                sr.ReadToEnd();

                sr.Close();
                stream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                string logFileName = localPath + "//log.txt";
                File.AppendAllText(logFileName, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} : {ex.Message}\r\n");
            }
        }
    }
}