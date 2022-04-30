﻿using System;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DocGenerator
{
    internal sealed class GeneratingJsonCommandFilter : IOleCommandTarget
    {
        private readonly IOleCommandTarget _nextCommandTargetInChain;
        private readonly IWpfTextView _wpfTextView;

        public GeneratingJsonCommandFilter(IVsTextView textView, IWpfTextView wpfTextView)
        {
            _wpfTextView = wpfTextView;
            textView.AddCommandFilter(this, out _nextCommandTargetInChain);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == GuidConstant.GeneratingJsonCommandId && cCmds == 1 && prgCmds[0].cmdID == 0x100)
            {
                GenerateContext(x =>
                {
                    if (x.IntendedSyntax)
                    {
                        prgCmds.SetVisibility();
                    }
                    else
                    {
                        prgCmds.SetInvisible();
                    }
                });

                return VSConstants.S_OK;
            }

            return _nextCommandTargetInChain.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == GuidConstant.GeneratingJsonCommandId && nCmdID == 0x100)
            {
                GenerateContext(x =>
                {
                    if (x.IntendedSyntax)
                    {
                        
                    }
                });
            }

            return _nextCommandTargetInChain.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void GenerateContext(Action<SyntaxContext> action)
        {
            if (action == null)
            {
                return;
            }

            var snapshot = _wpfTextView.TextBuffer.CurrentSnapshot;
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document != null)
            {
                var syntaxTree = ThreadHelper.JoinableTaskFactory.Run(() => document.GetSyntaxTreeAsync());
                if (syntaxTree != null)
                {
                    var root = ThreadHelper.JoinableTaskFactory.Run(() => syntaxTree.GetRootAsync());
                    var node = root.FindNode(_wpfTextView.GetTextSpan());
                    action(new SyntaxContext(document, node));
                }
            }
        }

        public static void Register(IVsTextView textView, IWpfTextView wpfTextView)
        {
            var _ = new GeneratingJsonCommandFilter(textView, wpfTextView);
        }
    }
}