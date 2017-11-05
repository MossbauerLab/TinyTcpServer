using System;
using System.Windows.Forms;

namespace MossbauerLab.TinyTcpServer.MnGUI.View.Utils
{
    public static class OpenDialogRunner
    {
        public static String Run(this OpenFileDialog fileDlg, String filterString, String initialPath, String dialogTitle, UInt16 filterIndex)
        {
            fileDlg.Title = dialogTitle;
            if (!String.IsNullOrEmpty(initialPath))
                fileDlg.InitialDirectory = initialPath;
            fileDlg.RestoreDirectory = true;
            fileDlg.Filter = filterString;
            fileDlg.FilterIndex = filterIndex;

            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                return fileDlg.FileName;
            }
            return String.Empty;
        }
    }
}
