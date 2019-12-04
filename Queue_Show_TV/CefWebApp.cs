using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xilium.CefGlue;

namespace Queue_Show_TV
{
    internal sealed class CefWebApp : CefApp
    {
        //  private CefRenderProcessHandler _renderProcessHandler = new DemoRenderProcessHandler();

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {

        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            // return _renderProcessHandler;\
            return null;
        }
    }
}            