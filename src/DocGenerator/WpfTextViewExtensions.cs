using Microsoft.VisualStudio.Text.Editor;

namespace DocGenerator
{
    public static class WpfTextViewExtensions
    {
        public static Microsoft.CodeAnalysis.Text.TextSpan GetTextSpan(this IWpfTextView view)
        {
            var selection = view.Selection;
            var start = selection.StreamSelectionSpan.Start.Position.Position;
            return new Microsoft.CodeAnalysis.Text.TextSpan(start, selection.StreamSelectionSpan.Length);
        }
    }
}