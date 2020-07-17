using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Windows;

namespace SimpleWpfPalette
{
    public class CustomPalette : PaletteSet
    {
        public CustomPalette()
            : base("WPF Palette", "SHOW_PALETTE", new Guid("{1E20F389-33C1-421F-81CB-B3D413E5B05C}"))
        {
            Style = PaletteSetStyles.ShowAutoHideButton |
                    PaletteSetStyles.ShowCloseButton |
                    PaletteSetStyles.ShowPropertiesMenu;
            MinimumSize = new System.Drawing.Size(150, 250);
            AddVisual("Hello", new PaletteUserControl());
        }
    }
}
