using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace SimpleWpfPalette
{
    /// <summary>
    /// Interaction logic for PaletteUserControl.xaml
    /// </summary>
    public partial class PaletteUserControl : UserControl
    {
        public PaletteUserControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// This method can call a custom AutoCAD command with sendStringToExecute to avoid 
        /// having to lock the current document and set the focus to AutoCAD application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var doc = AcAp.DocumentManager.MdiActiveDocument;
            if (doc != null)
                doc.SendStringToExecute("HELLO " + nameBox.Text + "\n", false, false, false);
        }
    }
}
