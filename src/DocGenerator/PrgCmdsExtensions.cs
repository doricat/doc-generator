using Microsoft.VisualStudio.OLE.Interop;

namespace DocGenerator
{
    public static class PrgCmdsExtensions
    {
        public static void SetInvisible(this OLECMD[] prgCmds)
        {
            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_INVISIBLE;
        }

        public static void SetVisibility(this OLECMD[] prgCmds)
        {
            prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
        }
    }
}