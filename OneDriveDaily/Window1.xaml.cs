using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
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

namespace OneDriveDaily
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {

        public static DependencyProperty TextProperty = DependencyProperty.Register("Text2", typeof(byte[]), typeof(Window1));

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool isClosed = true;
        public byte[] BetterStuff
        {
            get { return (byte[])GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }



        public Window1(string stuff)
        {
            InitializeComponent();

            DataContext = this;

            try
            {
                BetterStuff = File.ReadAllBytes(stuff);
            }
            catch 
            {
                BetterStuff = null;
            }

            isClosed = false;

        }

        public void Update(string stuff, bool d = true)
        {
            try
            {
                BetterStuff = File.ReadAllBytes(stuff);
            }
            catch
            {
                BetterStuff = null;
            }
            if (d)
                OnPropertyChanged(nameof(BetterStuff));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isClosed = true;
        }
    }
}
