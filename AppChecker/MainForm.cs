using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using AppChecker.Models;
using System.ServiceProcess;
using System.Net.Sockets;
using Microsoft.Win32;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.Threading;

namespace AppChecker
{
    public partial class MainForm : Form
    {
        private const string _serverURL = "https://api.harristax.ca/";
//        private const string _serverURL = "http://127.0.0.1:8081/";
        private const string _uploadPath = "api/upload";

        private static string UploadURI {
            get
            {
                return string.Format("{0}{1}", _serverURL, _uploadPath);
            }
        }

        public MainForm()
        {
            InitializeComponent();

            uploadWorker = new BackgroundWorker();
            uploadWorker.DoWork += UploadWorker_DoWork;
            uploadWorker.ProgressChanged += UploadWorker_ProgressChanged;
            uploadWorker.RunWorkerCompleted += UploadWorker_RunWorkerCompleted;
            uploadWorker.WorkerReportsProgress = true;
            uploadWorker.WorkerSupportsCancellation = true;

            foreach (var software in SoftwareNames)
            {
                cboSoftware.Items.Add(software);
            }
        }

        private static MainForm _instance = null;

        public static MainForm Instance 
        {
            get 
            {
                if(_instance == null)
                {
                    _instance = new MainForm();
                }
                return _instance;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private bool checkValidation()
        {
            if(txtCompany.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please enter company name.");
                return false;
            }

            if(txtFirstname.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please enter first name.");
                return false;
            }

            if (txtLastname.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please enter last name.");
                return false;
            }

            Regex emailChecker = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
            if (!emailChecker.IsMatch(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.");
                return false;
            }

            if (cboSoftware.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please enter software name.");
                return false;
            }

            var location = txtDatalocation.Text.Trim();

            if (!Directory.Exists(location))
            {
                MessageBox.Show("Please enter a valid data location.");
                return false;
            }

            return true;
        }

        BackgroundWorker uploadWorker;

        List<WebClient> uploadClients = new List<WebClient>();

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (checkValidation())
            {
                var filename = string.Format("{0}_{1}_{2}.zip", txtCompany.Text, cboSoftware.Text, DateTime.Now.ToString("yyyyMMdd"));

                var p = System.Diagnostics.Process.Start("rar", string.Format("a {0} \"{1}\"", filename, txtDatalocation.Text));
                p.WaitForExit();

                ProgressForm.Instance.Text = "Uploading...";
                ProgressForm.Instance.Show();

                uploadWorker.RunWorkerAsync(filename);
            }
        }

        private List<string> uploadPieceNames;

        private void UploadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = e.Argument as string;

            uploadPieceNames = SplitFile(fileName, 2 * 1024 * 1024);

            totalCount = uploadPieceNames.Count;
            uploadedCount = 0;

            uploadClients.Clear();
            var uploadUri = new Uri(UploadURI);

            int count = uploadPieceNames.Count;

            foreach (var piece in uploadPieceNames)
            {
                var uploadClient = new WebClient();
                uploadClients.Add(uploadClient);
                uploadClient.UploadFileCompleted += Client_UploadFileCompleted;
                uploadClient.UploadFileAsync(uploadUri, piece);
            }
        }

        private void UploadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void UploadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressForm.Instance.ChangeProgress(e.ProgressPercentage);
        }

        private int uploadedCount = 0;
        private int totalCount = 0;

        private List<string> SplitFile(string inputFile, int chunkSize)
        {
            const int BUFFER_SIZE = 2 * 1024 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            List<string> fileNames = new List<string>();

            using (Stream input = File.OpenRead(inputFile))
            {
                int index = 0;
                long count = (input.Length + (chunkSize - 1)) / chunkSize;
                while(input.Position < input.Length)
                {
                    string fileName = inputFile + "__" + count + "_" + index;
                    fileNames.Add(fileName);

                    using (Stream output = File.Create(fileName))
                    {
                        int remaining = chunkSize, bytesRead;
                        while(remaining > 0 && (bytesRead = input.Read(buffer, 0, Math.Min(remaining, BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            remaining -= bytesRead;
                        }
                        output.Close();
                    }
                    index++;
                    // Thread.Sleep(500);
                }
            }

            File.Delete(inputFile);

            return fileNames;
        }

        private void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            if(++uploadedCount == totalCount)
            {
                ProgressForm.Instance.Hide();
                foreach(var piece in uploadPieceNames)
                {
                    File.Delete(piece);
                }
            }
            ProgressForm.Instance.ChangeProgress(uploadedCount * 100 / totalCount);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                txtDatalocation.Text = dlg.SelectedPath;
            }
            
        }

        private void cboSoftware_TextChanged(object sender, EventArgs e)
        {
            _installed = false;
            if(cboSoftware.Text.ToLower() == "eaglesoft")
            {
                CheckEagleSoft();
            }
        }

        private static string[] SoftwareNames = new string[] {
            "Eaglesoft",
            "Dentrix",
//            "ABELDent",
//            "WinOMS",
            "Softdent",
//            "Open Dental",
//            "Power Practice",
//            "Cleardent",
//            "Dolphin",
//            "XLDent",
//            "OrthoTrac",
        };

        private bool _installed = false;
        private string _version = "";

        private void CheckApp(string softwareName)
        {

        }

        private static ServiceController[] _services = null;
        private static ServiceController[] Services
        {
            get
            {
                if (_services == null)
                {
                    _services = ServiceController.GetServices();
                }
                return _services;
            }
        }
        
        private void CheckEagleSoft()
        {
            ////////////// Check if this software is installed ////////////////

            foreach(var sc in Services)
            {
                if(sc.ServiceName.Contains("SQL Anywhere - PattersonDBServer"))
                {
                    _installed = true;
                    break;
                }
            }

            using(TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect("127.0.0.1", 2638);
                    _installed = true;
                }
                catch(Exception)
                {
                    //
                }
            }

            if(!_installed)
            {
                return;
            }

            //////////////////// Check the version of this software /////////////////
            try
            {
                var versionkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Eaglesoft\Select");
                _version = versionkey.GetValue("Version").ToString();
            }
            catch(Exception)
            {
                _installed = false;
                return;
            }

            try
            {
                var pathKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Eaglesoft\Paths");
                var data = pathKey.GetValue("Data").ToString();
                txtDatalocation.Text = data;
            }
            catch(Exception)
            {
                _installed = false;
                return;
            }


        }
    }
}
