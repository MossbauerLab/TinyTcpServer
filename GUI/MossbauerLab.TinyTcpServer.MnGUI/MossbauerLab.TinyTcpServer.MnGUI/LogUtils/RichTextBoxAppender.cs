using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace MossbauerLab.TinyTcpServer.MnGUI.LogUtils
{
    public class RichTextBoxAppender : MemoryAppender
    {
        public RichTextBoxAppender()
        {
        }

        public RichTextBoxAppender(RichTextBox richTextBox, Int32 maximumRichTextBoxLinesQuantity = DefaultRichTextBoxLines)
        {
            if(richTextBox == null)
                throw new ArgumentNullException("richTextBox");
            _richTextBox = richTextBox;
            _richTextBox.BackColor = Color.Black;;
            _maximumRichTextBoxLinesQuantity = maximumRichTextBoxLinesQuantity;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            String logMessage = String.Format(LogMessageTemplate, loggingEvent.Level, loggingEvent.TimeStamp, loggingEvent.RenderedMessage);
            _richTextBox.BeginInvoke(new Action<RichTextBox>(richTextBox =>
            {
                richTextBox.AppendText(logMessage + Environment.NewLine);
                richTextBox.Select(richTextBox.TextLength - logMessage.Length, logMessage.Length);
            }), _richTextBox);
            ApplyRichTextBoxFont(loggingEvent.Level);
            if (_richTextBox.Lines.Length > _maximumRichTextBoxLinesQuantity)
                _richTextBox.Clear();
            base.Append(loggingEvent);
        }

        private void ApplyRichTextBoxFont(Level level)
        {
            _richTextBox.BeginInvoke(new Action<RichTextBox>(richTextBox =>
            {
                Color richTextBoxTextColor = _richTextBoxLogsColors.ContainsKey(level) ? _richTextBoxLogsColors[level] : _defaultColor;
                richTextBox.SelectionColor = richTextBoxTextColor;
                richTextBox.SelectionFont = _defaultFont;
            }), _richTextBox);
        }

        private const Int32 DefaultRichTextBoxLines = 10000;
        private const String DefaultFontFamily = "Times New Roman";
        private const float DefaultFontSize = 10.0f;
        private const String LogMessageTemplate = "[{0}] [{1}] : {2}";

        private readonly Font _defaultFont = new Font(DefaultFontFamily, DefaultFontSize, FontStyle.Bold);
        private readonly Color _defaultColor = Color.Azure;
        private readonly RichTextBox _richTextBox;
        private readonly IDictionary<Level, Color> _richTextBoxLogsColors = new Dictionary<Level, Color>
        {
            {Level.Error, Color.OrangeRed},
            {Level.Warn, Color.Orange},
            {Level.Info, Color.LimeGreen},
            {Level.Debug, Color.Olive},
            {Level.All, Color.Azure}
        };

        private readonly Int32 _maximumRichTextBoxLinesQuantity;
    }
}
