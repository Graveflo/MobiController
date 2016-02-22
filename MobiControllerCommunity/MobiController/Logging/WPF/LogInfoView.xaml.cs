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
using System.Windows.Shapes;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for LogInfoView.xaml
    /// </summary>
    public partial class LogInfoView : Window
    {
        public LogInfoView(String message)
        {
            InitializeComponent();
            txtMessage.Text = message;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            base.OnClosing(e);
        }
    }
}
