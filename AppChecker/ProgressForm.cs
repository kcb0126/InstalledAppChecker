using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AppChecker
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        private static ProgressForm _instance = null;

        public static ProgressForm Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new ProgressForm();
                }
                return _instance;
            }
        }

        public void ChangeProgress(int progress)
        {
            progressBar.Value = progress;
            lblPercentage.Text = string.Format("{0} %", progress);
        }
    }
}
