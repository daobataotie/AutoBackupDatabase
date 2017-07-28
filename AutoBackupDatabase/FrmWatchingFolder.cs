using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AutoBackupDatabase
{
    public partial class FrmWatchingFolder : Form
    {
        string watchingFolder = @"D:\Source folder";
        string backupFolder = @"F:\Backup";
        bool flag = true;
        FileSystemWatcher fsw = null;

        public FrmWatchingFolder()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.notifyIcon1.Visible = false;

            MenuItem menuItem = new MenuItem("退出程序");
            menuItem.Click += MenuItem_Click;

            this.notifyIcon1.ContextMenu = new ContextMenu(new MenuItem[] { menuItem });
            this.Show();

            fsw = new FileSystemWatcher(watchingFolder);
            fsw.Created += Fsw_Created;
            fsw.Changed += Fsw_Created;
            fsw.IncludeSubdirectories = false;
            fsw.Filter = "*.bak";
        }
        
        private void MenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
            this.Dispose();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.notifyIcon1.Visible = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Hide();
            this.notifyIcon1.Visible = true;
            e.Cancel = true;
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            string fileName = Path.GetFileName(e.FullPath);
            string destFileName = Path.Combine(backupFolder, fileName);

            CopyFile(destFileName, e.FullPath);
        }

        private void CopyFile(string destFileName, string sourceFileName)
        {
            try
            {
                if (!File.Exists(destFileName))
                {
                    File.Copy(sourceFileName, destFileName);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("正由另一进程使用，因此该进程无法访问此文件"))
                    CopyFile(destFileName, sourceFileName);
                else
                    throw new Exception(ex.Message);
            }
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (this.flag)
            {
                this.lbl_StartTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : 开始监视";
                this.btn_Start.ForeColor = Color.Red;
                fsw.EnableRaisingEvents = true;
                this.btn_Start.Text = "停止";
                this.flag = !this.flag;
            }
            else
            {
                this.lbl_StartTime.Text = null;
                this.btn_Start.ForeColor = Color.Green;
                fsw.EnableRaisingEvents = false;
                this.btn_Start.Text = "开始";
                this.flag = !this.flag;
            }
        }
    }
}
