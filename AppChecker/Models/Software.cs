using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace AppChecker.Models
{
    class Software
    {
        public string Name { get; set; }

        public Software(string name)
        {
            this.Name = name;
        }

        public static List<Software> Softwares = new List<Software> {
            new Software("Eaglesoft"),
            new Software("Dentrix"),
            new Software("ABELDent"),
            new Software("WinOMS"),
            new Software("Softdent"),
            new Software("Open Dental"),
            new Software("Power Practice"),
            new Software("Cleardent"),
            new Software("Dolphin"),
            new Software("XLDent"),
            new Software("OrthoTrac"),
        };

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

        public static string CheckDataLocation(string softwareName)
        {
            bool installed = false;

            foreach(var sc in Services)
            {
                sc.ServiceName.Contains("");
            }

            return "";
        }
    }
}
