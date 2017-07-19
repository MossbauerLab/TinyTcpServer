using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using MossbauerLab.TinyTcpServer.Core.Client;
using MossbauerLab.TinyTcpServer.Core.Server;
using MossbauerLab.TinyTcpServer.MnGUI.Data;
using MossbauerLab.TinyTcpServer.MnGUI.Factories;

namespace MossbauerLab.TinyTcpServer.MnGUI.View.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Load += (sender, args) => FillControls();
            _startButton.Click += (sender, args) => Start();
            _stopButton.Click += (sender, args) => Stop();
            _restartButton.Click += (sender, args) => Restart();
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
            _serverTypeComboBox.SelectedIndex = 0;

            // fill log level
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
            // getting server type
            if (_serverTypeComboBox.SelectedIndex < 0)
            {
                // log
                return;
            }
            ServerType serverType = ServerType.Scripting;
            foreach (KeyValuePair<ServerType, String> server in _servers)
            {
                if (String.Equals(_serverTypeComboBox.Items[_serverTypeComboBox.SelectedIndex].ToString(), server.Value))
                    serverType = server.Key;
            }
            if (_server == null)
                _server = ServerFactory.Create(serverType, _ipAddressComboBox.Items[_ipAddressComboBox.SelectedIndex].ToString(), UInt16.Parse(_portTextBox.Text));
            _server.Start();
            if (_timers[0] == null)
            {
                System.Threading.Timer periodicalUpdater = new System.Threading.Timer(StateUpdater, null, 500, 500);
                _timers[0] = periodicalUpdater;
            }
            else _timers[0].Change(500, 500);
        }

        public void Stop()
        {
            _server.Stop(false);
            if (_timers[0] != null)
            {
                _timers[0].Change(-1, -1);
            }
            UpdateButtonsState();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        void UpdateButtonsState()
        {
            if (_server == null)
                return;
            _startButton.Enabled = !_server.IsReady;
            _restartButton.Enabled = _server.IsReady;
            _stopButton.Enabled = _server.IsReady;
        }

        void PopulateClients()
        {
            _clientsListBox.Items.Clear();
            IList<TcpClientContext> clients = _server.Clients;
            if (clients != null)
            {
                _clientsListBox.Items.Add(String.Format(TotalClientsTemplate, clients.Count));
                foreach (TcpClientContext client in clients)
                {
                    _clientsListBox.Items.Add(String.Format(ClientInfoTemplate, client.Id, "127.0.0.1"));
                }
            }
        }

        void StateUpdater(Object state)
        {
            BeginInvoke((Action)(UpdateButtonsState));
            BeginInvoke((Action) PopulateClients);
        }

        private const String ConfigFile = @".\settings.txt";
        private const UInt16 DefaultTcpPort = 9999;
        private const String TotalClientsTemplate = "Total connected clients: {0}";
        private const String ClientInfoTemplate = "Client {0}, ip: {1}";

        private readonly IDictionary<ServerType, String> _servers = new Dictionary<ServerType, String>()
        {
            {ServerType.Echo, "Echo server"},
            {ServerType.Time, "Time server"},
        };

        private ITcpServer _server;
        private readonly System.Threading.Timer[] _timers = new System.Threading.Timer[1];
    }
}
