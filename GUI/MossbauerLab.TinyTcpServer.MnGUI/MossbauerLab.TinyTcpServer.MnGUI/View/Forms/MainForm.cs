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
using MossbauerLab.TinyTcpServer.MnGUI.Data;

namespace MossbauerLab.TinyTcpServer.MnGUI.View.Forms
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
            else
            {
                if (_ipAddressComboBox.Items.Count > 0)
                    _ipAddressComboBox.SelectedIndex = 0;
                _portTextBox.Text = DefaultTcpPort.ToString();
            }

            // add server type
            foreach (KeyValuePair<ServerType, String> server in _servers)
                _serverTypeComboBox.Items.Add(server.Value);
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

        private void Start()
        {
        }

        public void Stop()
        {
        }

        public void Restart()
        {
        }

        private const String ConfigFile = @".\settings.txt";
        private const UInt16 DefaultTcpPort = 9999;

        private readonly IDictionary<ServerType, String> _servers = new Dictionary<ServerType, String>()
        {
            {ServerType.Echo, "Echo server"},
            {ServerType.Time, "Time server"},
        };
    }
}
