namespace MossbauerLab.TinyTcpServer.MnGUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._startButton = new System.Windows.Forms.Button();
            this._stopButton = new System.Windows.Forms.Button();
            this._restartButton = new System.Windows.Forms.Button();
            this._logsGroupBox = new System.Windows.Forms.GroupBox();
            this._logsTextBox = new System.Windows.Forms.RichTextBox();
            this._clientsListBox = new System.Windows.Forms.ListBox();
            this._serverSettingsGroup = new System.Windows.Forms.GroupBox();
            this._logLevelComboBox = new System.Windows.Forms.ComboBox();
            this._logLevelLabel = new System.Windows.Forms.Label();
            this._serverParametersBox = new System.Windows.Forms.GroupBox();
            this._serverParametersView = new System.Windows.Forms.ListView();
            this._applyButton = new System.Windows.Forms.Button();
            this._serverTypeComboBox = new System.Windows.Forms.ComboBox();
            this._portTextBox = new System.Windows.Forms.TextBox();
            this._ipAddressComboBox = new System.Windows.Forms.ComboBox();
            this._serverTypeLabel = new System.Windows.Forms.Label();
            this._portLabel = new System.Windows.Forms.Label();
            this._ipAddressLabel = new System.Windows.Forms.Label();
            this._logsGroupBox.SuspendLayout();
            this._serverSettingsGroup.SuspendLayout();
            this._serverParametersBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _startButton
            // 
            this._startButton.Location = new System.Drawing.Point(624, 352);
            this._startButton.Name = "_startButton";
            this._startButton.Size = new System.Drawing.Size(75, 23);
            this._startButton.TabIndex = 0;
            this._startButton.Text = "Start";
            this._startButton.UseVisualStyleBackColor = true;
            // 
            // _stopButton
            // 
            this._stopButton.Location = new System.Drawing.Point(705, 352);
            this._stopButton.Name = "_stopButton";
            this._stopButton.Size = new System.Drawing.Size(75, 23);
            this._stopButton.TabIndex = 1;
            this._stopButton.Text = "Stop";
            this._stopButton.UseVisualStyleBackColor = true;
            // 
            // _restartButton
            // 
            this._restartButton.Location = new System.Drawing.Point(786, 352);
            this._restartButton.Name = "_restartButton";
            this._restartButton.Size = new System.Drawing.Size(75, 23);
            this._restartButton.TabIndex = 2;
            this._restartButton.Text = "Restart";
            this._restartButton.UseVisualStyleBackColor = true;
            // 
            // _logsGroupBox
            // 
            this._logsGroupBox.Controls.Add(this._logsTextBox);
            this._logsGroupBox.Location = new System.Drawing.Point(12, 381);
            this._logsGroupBox.Name = "_logsGroupBox";
            this._logsGroupBox.Size = new System.Drawing.Size(849, 162);
            this._logsGroupBox.TabIndex = 3;
            this._logsGroupBox.TabStop = false;
            this._logsGroupBox.Text = "Logs:";
            // 
            // _logsTextBox
            // 
            this._logsTextBox.Location = new System.Drawing.Point(7, 20);
            this._logsTextBox.Name = "_logsTextBox";
            this._logsTextBox.Size = new System.Drawing.Size(824, 136);
            this._logsTextBox.TabIndex = 0;
            this._logsTextBox.Text = "";
            // 
            // _clientsListBox
            // 
            this._clientsListBox.FormattingEnabled = true;
            this._clientsListBox.Location = new System.Drawing.Point(624, 12);
            this._clientsListBox.Name = "_clientsListBox";
            this._clientsListBox.Size = new System.Drawing.Size(237, 329);
            this._clientsListBox.TabIndex = 4;
            // 
            // _serverSettingsGroup
            // 
            this._serverSettingsGroup.Controls.Add(this._logLevelComboBox);
            this._serverSettingsGroup.Controls.Add(this._logLevelLabel);
            this._serverSettingsGroup.Controls.Add(this._serverParametersBox);
            this._serverSettingsGroup.Controls.Add(this._applyButton);
            this._serverSettingsGroup.Controls.Add(this._serverTypeComboBox);
            this._serverSettingsGroup.Controls.Add(this._portTextBox);
            this._serverSettingsGroup.Controls.Add(this._ipAddressComboBox);
            this._serverSettingsGroup.Controls.Add(this._serverTypeLabel);
            this._serverSettingsGroup.Controls.Add(this._portLabel);
            this._serverSettingsGroup.Controls.Add(this._ipAddressLabel);
            this._serverSettingsGroup.Location = new System.Drawing.Point(12, 12);
            this._serverSettingsGroup.Name = "_serverSettingsGroup";
            this._serverSettingsGroup.Size = new System.Drawing.Size(597, 363);
            this._serverSettingsGroup.TabIndex = 5;
            this._serverSettingsGroup.TabStop = false;
            this._serverSettingsGroup.Text = "Server settings";
            // 
            // _logLevelComboBox
            // 
            this._logLevelComboBox.FormattingEnabled = true;
            this._logLevelComboBox.Location = new System.Drawing.Point(96, 175);
            this._logLevelComboBox.Name = "_logLevelComboBox";
            this._logLevelComboBox.Size = new System.Drawing.Size(121, 21);
            this._logLevelComboBox.TabIndex = 9;
            // 
            // _logLevelLabel
            // 
            this._logLevelLabel.AutoSize = true;
            this._logLevelLabel.Location = new System.Drawing.Point(19, 183);
            this._logLevelLabel.Name = "_logLevelLabel";
            this._logLevelLabel.Size = new System.Drawing.Size(57, 13);
            this._logLevelLabel.TabIndex = 8;
            this._logLevelLabel.Text = "Log Level:";
            // 
            // _serverParametersBox
            // 
            this._serverParametersBox.Controls.Add(this._serverParametersView);
            this._serverParametersBox.Location = new System.Drawing.Point(230, 21);
            this._serverParametersBox.Name = "_serverParametersBox";
            this._serverParametersBox.Size = new System.Drawing.Size(350, 330);
            this._serverParametersBox.TabIndex = 7;
            this._serverParametersBox.TabStop = false;
            this._serverParametersBox.Text = "Server Parameters";
            // 
            // _serverParametersView
            // 
            this._serverParametersView.Location = new System.Drawing.Point(16, 21);
            this._serverParametersView.Name = "_serverParametersView";
            this._serverParametersView.Size = new System.Drawing.Size(317, 287);
            this._serverParametersView.TabIndex = 0;
            this._serverParametersView.UseCompatibleStateImageBehavior = false;
            // 
            // _applyButton
            // 
            this._applyButton.Location = new System.Drawing.Point(142, 133);
            this._applyButton.Name = "_applyButton";
            this._applyButton.Size = new System.Drawing.Size(75, 23);
            this._applyButton.TabIndex = 6;
            this._applyButton.Text = "Apply";
            this._applyButton.UseVisualStyleBackColor = true;
            // 
            // _serverTypeComboBox
            // 
            this._serverTypeComboBox.FormattingEnabled = true;
            this._serverTypeComboBox.Location = new System.Drawing.Point(97, 94);
            this._serverTypeComboBox.Name = "_serverTypeComboBox";
            this._serverTypeComboBox.Size = new System.Drawing.Size(121, 21);
            this._serverTypeComboBox.TabIndex = 5;
            // 
            // _portTextBox
            // 
            this._portTextBox.Location = new System.Drawing.Point(97, 58);
            this._portTextBox.Name = "_portTextBox";
            this._portTextBox.Size = new System.Drawing.Size(121, 20);
            this._portTextBox.TabIndex = 4;
            // 
            // _ipAddressComboBox
            // 
            this._ipAddressComboBox.FormattingEnabled = true;
            this._ipAddressComboBox.Location = new System.Drawing.Point(97, 21);
            this._ipAddressComboBox.Name = "_ipAddressComboBox";
            this._ipAddressComboBox.Size = new System.Drawing.Size(121, 21);
            this._ipAddressComboBox.TabIndex = 3;
            // 
            // _serverTypeLabel
            // 
            this._serverTypeLabel.AutoSize = true;
            this._serverTypeLabel.Location = new System.Drawing.Point(16, 97);
            this._serverTypeLabel.Name = "_serverTypeLabel";
            this._serverTypeLabel.Size = new System.Drawing.Size(68, 13);
            this._serverTypeLabel.TabIndex = 2;
            this._serverTypeLabel.Text = "Server Type:";
            // 
            // _portLabel
            // 
            this._portLabel.AutoSize = true;
            this._portLabel.Location = new System.Drawing.Point(16, 61);
            this._portLabel.Name = "_portLabel";
            this._portLabel.Size = new System.Drawing.Size(29, 13);
            this._portLabel.TabIndex = 1;
            this._portLabel.Text = "Port:";
            // 
            // _ipAddressLabel
            // 
            this._ipAddressLabel.AutoSize = true;
            this._ipAddressLabel.Location = new System.Drawing.Point(16, 29);
            this._ipAddressLabel.Name = "_ipAddressLabel";
            this._ipAddressLabel.Size = new System.Drawing.Size(61, 13);
            this._ipAddressLabel.TabIndex = 0;
            this._ipAddressLabel.Text = "IP Address:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(883, 567);
            this.Controls.Add(this._serverSettingsGroup);
            this.Controls.Add(this._clientsListBox);
            this.Controls.Add(this._logsGroupBox);
            this.Controls.Add(this._restartButton);
            this.Controls.Add(this._stopButton);
            this.Controls.Add(this._startButton);
            this.Name = "MainForm";
            this.Text = "Server Management Tools";
            this._logsGroupBox.ResumeLayout(false);
            this._serverSettingsGroup.ResumeLayout(false);
            this._serverSettingsGroup.PerformLayout();
            this._serverParametersBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _startButton;
        private System.Windows.Forms.Button _stopButton;
        private System.Windows.Forms.Button _restartButton;
        private System.Windows.Forms.GroupBox _logsGroupBox;
        private System.Windows.Forms.ListBox _clientsListBox;
        private System.Windows.Forms.GroupBox _serverSettingsGroup;
        private System.Windows.Forms.Label _serverTypeLabel;
        private System.Windows.Forms.Label _portLabel;
        private System.Windows.Forms.Label _ipAddressLabel;
        private System.Windows.Forms.ComboBox _ipAddressComboBox;
        private System.Windows.Forms.TextBox _portTextBox;
        private System.Windows.Forms.ComboBox _serverTypeComboBox;
        private System.Windows.Forms.Button _applyButton;
        private System.Windows.Forms.RichTextBox _logsTextBox;
        private System.Windows.Forms.GroupBox _serverParametersBox;
        private System.Windows.Forms.ListView _serverParametersView;
        private System.Windows.Forms.ComboBox _logLevelComboBox;
        private System.Windows.Forms.Label _logLevelLabel;
    }
}

