using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MossbauerLab.TinyTcpServer.Core.Client;
using MossbauerLab.TinyTcpServer.Core.Server;
using MossbauerLab.TinyTcpServer.MnGUI.Factories;
using MossbauerLab.TinyTcpServer.MnGUI.Helpers;
using MossbauerLab.TinyTcpServer.MnGUI.LogUtils;
using MossbauerLab.TinyTcpServer.MnGUI.View.Utils;

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
            _serverScriptButton.Click += OnChooseScriptFileButtonClick;
            _serverConfigButton.Click += OnChooseConfigFileButtonClick;
            _logLevelComboBox.SelectedIndexChanged += (sender, args) => ApplyLogLevel();
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
                _configFile = ConfigFile;
                DisplayConfig();
            }

            if (_ipAddressComboBox.Items.Count > 0)
                _ipAddressComboBox.SelectedIndex = 0;
                _portTextBox.Text = DefaultTcpPort.ToString();

            // init logger + fill log level
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(MainForm));
            _richTextBoxAppender = new RichTextBoxAppender(_logsTextBox);
            ((Logger)_logger.Logger).AddAppender(_richTextBoxAppender);
            foreach (KeyValuePair<Level, String> level in _logLevels)
                _logLevelComboBox.Items.Add(level.Value);
            _logLevelComboBox.SelectedIndex = 5;

            UpdateControlsState();
        }

        private void DisplayConfig()
        {
            String[] lines = File.ReadAllLines(_configFile).Where(line => !line.Trim().StartsWith("#")).ToArray();
            _serverParametersView.Items.Clear();
            foreach (String line in lines)
                _serverParametersView.Items.Add(line);
            _serverConfigBox.Text = Path.GetFileName(_configFile);
        }


        private void Start()
        {
            UInt16 port = Convert.ToUInt16(_portTextBox.Text);
            if (_server == null)
            {
                if (!String.IsNullOrEmpty(_configFile))
                    _serverConfig = TcpServerConfigBuilder.Build(_configFile);
                if (_ipAddressComboBox.SelectedIndex >= 0 && _portTextBox.Text != null && !String.IsNullOrEmpty(_scriptFile))
                    _server = ServerFactory.Create(_ipAddressComboBox.Items[_ipAddressComboBox.SelectedIndex].ToString(), port, _scriptFile, _logger, _serverConfig);
                else
                {
                    MessageBox.Show(@"Can not start server, please select IP address, port and server script");
                    return;
                }
                _server.Start();
            }
            else
            {
                if(_configChanged)
                    _server = ServerFactory.Create(_ipAddressComboBox.Items[_ipAddressComboBox.SelectedIndex].ToString(), port, _scriptFile, _logger, _serverConfig);
                _server.Start(_ipAddressComboBox.Items[_ipAddressComboBox.SelectedIndex].ToString(), port);
            }

            if (_timers[0] == null)
            {
                System.Threading.Timer periodicalUpdater = new System.Threading.Timer(StateUpdater, null, 500, 500);
                _timers[0] = periodicalUpdater;
            }
            else _timers[0].Change(500, 500);
        }

        private void Stop()
        {
            _server.Stop(false);
            if (_timers[0] != null)
            {
                _timers[0].Change(-1, -1);
            }
            UpdateControlsState();
        }

        private void Restart()
        {
            Stop();
            Start();
        }

        private void OnChooseScriptFileButtonClick(Object sender, EventArgs args)
        {
            OpenFileDialog openScriptFile = new OpenFileDialog();
            String file = openScriptFile.Run(@" CSharp files (*.cs)|*.cs", Path.GetFullPath("."), @"Choose C# script file", 0);
            if (file != String.Empty)
            {
                _scriptFile = file;
                _serverScriptBox.Text = Path.GetFileName(_scriptFile);
            }
        }

        private void OnChooseConfigFileButtonClick(Object sender, EventArgs args)
        {
            OpenFileDialog openScriptFile = new OpenFileDialog();
            String file = openScriptFile.Run(@" Text files (*.txt)|*.txt|Config files (*.conf)|*.conf|Config files (*.cfg)|*.cfg|Any file (*.*)|*.*", 
                                             Path.GetFullPath("."), @"Choose Server settings file", 0);
            if (file != String.Empty)
            {
                _configFile = file;
                DisplayConfig();
                _configChanged = true;
            }
        }

        private void UpdateControlsState()
        {
            if (_server == null)
            {
                _startButton.Enabled = true;
                _restartButton.Enabled = false;
                _stopButton.Enabled = false;
                return;
            }
            
            _startButton.Enabled = !_server.IsReady;
            _restartButton.Enabled = _server.IsReady;
            _stopButton.Enabled = _server.IsReady;
            _applyButton.Enabled = !_server.IsReady;
            _logLevelComboBox.Enabled = !_server.IsReady;
            _ipAddressComboBox.Enabled = !_server.IsReady;
            _portTextBox.Enabled = !_server.IsReady;
        }

        private void PopulateClients()
        {
            _clientsListBox.Items.Clear();
            IList<TcpClientContext> clients = _server.Clients;
            if (clients != null)
            {
                _clientsListBox.Items.Add(String.Format(TotalClientsTemplate, clients.Count));
                foreach (TcpClientContext client in clients)
                {
                    String ipAddress = ((IPEndPoint)client.Client.Client.RemoteEndPoint).Address.ToString();
                    _clientsListBox.Items.Add(String.Format(ClientInfoTemplate, client.Id, ipAddress));
                }
            }
        }

        private void StateUpdater(Object state)
        {
            BeginInvoke((Action)(UpdateControlsState));
            BeginInvoke((Action) PopulateClients);
        }

        private void ApplyLogLevel()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Threshold =_logLevels.First(item => item.Value.Equals(_logLevelComboBox.Items[_logLevelComboBox.SelectedIndex].ToString())).Key;
        }

        private const String ConfigFile = @".\settings.txt";
        private const UInt16 DefaultTcpPort = 9999;
        private const String TotalClientsTemplate = "Total connected clients: {0}";
        private const String ClientInfoTemplate = "Client {0}, ip: {1}"; // todo: umv add sctipt and settings ....


        private readonly IDictionary<Level, String> _logLevels = new Dictionary<Level, String>()
        {
            {Level.Alert, "Alert"}, 
            {Level.Critical, "Critical"}, 
            {Level.Debug, "Debug"},
            {Level.Emergency, "Emergency"}, 
            {Level.Error, "Error"}, 
            {Level.Info, "Info"}, 
            {Level.Warn, "Warn"}
        };

        
        private ILog _logger;
        private ITcpServer _server;
        private String _scriptFile;
        private String _configFile;
        private TcpServerConfig _serverConfig = new TcpServerConfig();
        private Boolean _configChanged = false;
        private RichTextBoxAppender _richTextBoxAppender;
        private readonly System.Threading.Timer[] _timers = new System.Threading.Timer[1];
    }
}
