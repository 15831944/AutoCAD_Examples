using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace SimpleWpfPalette
{
    public class Commands
    {
        static CustomPalette palette;

        [CommandMethod("SHOW_PALETTE")]
        public static void ShowPalette()
        {
            if (palette == null)
                palette = new CustomPalette();
            palette.Visible = true;
        }

        [CommandMethod("Hello")]
        public static void Hello()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var pr = ed.GetString("\nEnter your name: ");
            if (pr.Status == PromptStatus.OK)
                Application.ShowAlertDialog("Hello " + pr.StringResult);
        }
    }
}
