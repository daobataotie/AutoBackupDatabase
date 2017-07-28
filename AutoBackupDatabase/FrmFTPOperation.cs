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
    public partial class FrmFTPOperation : Form
    {
        bool flag = true;
        FTPHelper ftp;
        string backupFolder = "F:\\Backup";

        public FrmFTPOperation()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.notifyIcon1.Visible = false;

            MenuItem menuItem = new MenuItem("退出程序");
            menuItem.Click += MenuItem_Click;
            this.notifyIcon1.ContextMenu = new ContextMenu(new MenuItem[] { menuItem });
            this.Show();

            timer1.Interval = 1000;
            ftp = new FTPHelper("ftp://211.149.171.190/");
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

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (this.flag)
            {
                this.lbl_StartTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : 开始监视";
                this.btn_Start.ForeColor = Color.Red;
                this.btn_Start.Text = "停止";
                this.flag = !this.flag;

                this.timer1.Start();
            }
            else
            {
                this.lbl_StartTime.Text = null;
                this.btn_Start.ForeColor = Color.Green;
                this.btn_Start.Text = "开始";
                this.flag = !this.flag;

                this.timer1.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            List<string> fileNames = ftp.GetFileNames();
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
            string[] files = Directory.GetFiles(backupFolder);

            List<string> downloadFiles = fileNames.Where(F => !files.Any(f => f.Contains(F))).ToList();
            downloadFiles.ForEach(F =>
            {
                ftp.DownloadFile(F, backupFolder);
            });
        }
    }
}
