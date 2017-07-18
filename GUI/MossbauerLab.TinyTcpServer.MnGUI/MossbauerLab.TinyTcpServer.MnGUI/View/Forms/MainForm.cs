using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace MossbauerLab.TinyTcpServer.MnGUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Load += (sender, args) => FillControls();
            
        }

        private void FillControls()
        {
            // fill ip address
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    _ipAddressComboBox.Items.Add(ip.ToString());
            }
            // load last IP address and TCP Port settings
            if (File.Exists(ConfigFile))
            {
                String[] filelines = File.ReadAllLines(ConfigFile);
                ApplySettings(GetOptions(filelines));
            }
        }

        private IDictionary<String, String> GetOptions(String[] lines)
        {
            IDictionary<String, String> options = new Dictionary<String, String>();
            // todo: parse options ....
            return options;
        }

        private void ApplySettings(IDictionary<String, String> options)
        {

        }

        private const String ConfigFile = @".\settings.txt";
    }
}
