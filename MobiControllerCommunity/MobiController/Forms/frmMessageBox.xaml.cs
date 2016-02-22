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
using System.Diagnostics;
using System.Windows.Interop;

using WinAPIWrapper;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        int numberofbuttons = 0; // kind of lazy but will work just fine
        bool isButtonAdd = false;

        public StackPanel ButtonPannel
        {
            get
            {
                return buttonPannel;
            }
        }

        public ProgressBar PrgBar
        {
            get
            {
                return prgbar;
            }
        }

        private bool noFocus;
        public bool NoFocus
        {
            get { return noFocus; }
            set { noFocus = value; }
        }

        public MessageBox(Window owner, String message)
        {
            this.Owner = owner;
            InitializeComponent();
            lblMessage.Text = message;
        }

        public MessageBox(String message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }

        public MessageBox(String message, String caption)
            : this(message)
        {
            this.Title = caption;
        }
        public MessageBox(Window owner, String message, String caption)
            : this(owner, message)
        {
            this.Title = caption;
        }

        public void addButton(String text, Action clickEventCall)
        {
            addButton(text, clickEventCall, true);
        }

        public void addButton(String text, Action clickEventCall, bool close)
        {
            isButtonAdd = true;
            Button b = new Button() { Margin = new Thickness(10, 0, 0, 0), Height = 36, MinWidth = 75, HorizontalContentAlignment= System.Windows.HorizontalAlignment.Center, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Foreground = Brushes.White, FontSize = 12 };
            if (close)
            {
                b.Click += delegate(System.Object o, System.Windows.RoutedEventArgs e) { clickEventCall.Invoke(); Close(); };

            }
            else
            {
                b.Click += delegate(System.Object o, System.Windows.RoutedEventArgs e) { clickEventCall.Invoke(); };
            }
            Viewbox thisViewbox = new Viewbox() { MaxHeight = b.FontSize + 4 };
            TextBlock buttonCaption = new TextBlock() { HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center, Text = "  " + text + "  ", FontFamily = b.FontFamily, FontStretch = b.FontStretch, FontStyle = b.FontStyle, FontWeight = b.FontWeight };
            thisViewbox.Child = buttonCaption;
            b.Content = thisViewbox;
            buttonPannel.Children.Add(b);
            b.Visibility = System.Windows.Visibility.Visible;
        }

        private void frmMessageBox_Loaded(object sender, RoutedEventArgs e)
        {
            WinAPI.FLASH_WINDOW.FLASHWINFO s = WinAPI.FLASH_WINDOW.createFlashWindowInfo(Process.GetCurrentProcess().MainWindowHandle);
            WinAPI.FlashWindowEx(ref s);
            if (!isButtonAdd)
            {
                addButton("Okay", delegate { });
            }
        }

        private void prgbar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public void bringForward()
        {
            IntPtr msgboxHwnd = new WindowInteropHelper(this).Handle;
            WinAPI.SetForegroundWindow(msgboxHwnd);
        }

        
    }
}
