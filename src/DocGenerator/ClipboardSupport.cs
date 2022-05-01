using System.Windows.Forms;

namespace DocGenerator
{
    public static class ClipboardSupport
    {
        public static void SetClipboardData(string unicode)
        {
            var data = new DataObject();

            if (unicode != null)
            {
                data.SetText(unicode, TextDataFormat.UnicodeText);
                data.SetText(unicode, TextDataFormat.Text);
            }

            try
            {
                Clipboard.SetDataObject(data, false);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                // Ignored
            }
        }
    }
}