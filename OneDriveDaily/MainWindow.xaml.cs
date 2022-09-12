using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace OneDriveDaily
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TestyTest
    {
        public TestyTest(TestyTest2 uri)
        {
            ImageUri = uri.Name;
            Size = uri.Size + " KB";
            Image = File.ReadAllBytes(uri.Name);

            var temp = BitmapFrame.Create(new MemoryStream(Image), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            Resolution = $"{temp.PixelWidth} x {temp.PixelHeight}";
            temp = null;
        }

        public string ImageUri { get; set; }

        public string Resolution { get; set; }

        public string Size { get; set; }

        public byte[] Image { get; set; }
    }

    public class TestyTest2
    {
        public string Name { get; set; }
        public string Resolution { get; set; }
        public long Size { get; set; }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            index = 0;

            ChooseFiles();
        }

        //public List<string> m_arrFiles;
        private string[] Paths = new string[5] { ".bmp", ".jpg", ".jpeg", ".png", ".jfif" };

        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(TestyTest), typeof(MainWindow));
        public TestyTest item
        {
            get { return (TestyTest)GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static DependencyProperty TextProperty2 = DependencyProperty.Register("Text7", typeof(string), typeof(MainWindow));
        public string item2
        {
            get { return (string)GetValue(TextProperty2); }
            set { this.SetValue(TextProperty2, value); }
        }

        public static DependencyProperty ListProperty = DependencyProperty.Register("Text4", typeof(ObservableCollection<TestyTest>), typeof(MainWindow));
        public ObservableCollection<TestyTest> m_arrFiles
        {
            get { return (ObservableCollection<TestyTest>)GetValue(ListProperty); }
            set { this.SetValue(ListProperty, value); }
        }

        public static DependencyProperty ListProperty2 = DependencyProperty.Register("Text5", typeof(int), typeof(MainWindow));
        public int index
        {
            get { return (int)GetValue(ListProperty2); }
            set { this.SetValue(ListProperty2, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Window1 window = null;

        private void ChooseFiles()
        {
            var arrFiles = new List<TestyTest2>();

            var systemPath = System.Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            var complete = System.IO.Path.Combine(systemPath, "files2.txt");
            var strFolders = "";
            try
            {
                using (StreamReader iso = new StreamReader(complete))
                {
                    strFolders = iso.ReadLine();
                }
                if (strFolders.Length == 0) return;
            }
            catch
            {
                return;
            }

            var arrFolders = strFolders.Split(',');
            foreach (var item in arrFolders)
            {
                arrFiles.AddRange(CountFiles(new DirectoryInfo(item.Trim()).GetFileSystemInfos()));
            }

            m_arrFiles = new ObservableCollection<TestyTest>();
            arrFiles = arrFiles.OrderBy(c => System.IO.Path.GetFileNameWithoutExtension(c.Name)).ToList();

            foreach (var item in arrFiles)
            {
                m_arrFiles.Add(new TestyTest(item));
            }
        }
        private List<TestyTest2> CountFiles(FileSystemInfo[] infos)
        {
            List<TestyTest2> files = new List<TestyTest2>();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i] is DirectoryInfo)
                {
                    files.AddRange(CountFiles(((DirectoryInfo)infos[i]).GetFileSystemInfos()));
                }
                else if (infos[i] is FileInfo)
                {
                    if (Paths.Contains((((FileInfo)infos[i]).Extension).ToLower()))
                    {
                        var date = infos[i].LastWriteTime;
                        var today = DateTime.Today;

                        if (today.Day == date.Day && today.Month == date.Month)
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                    }
                }
            }
            return files;
        }

        private void Frank_MouseDown(object sender, MouseButtonEventArgs e)
        {
            index = this.Test.SelectedIndex == -1 ? 0 : this.Test.SelectedIndex;
            LastIndex = index;
            item = (TestyTest)Test.Items[index];
            item2 = this.m_arrFiles[index].ImageUri;

            if (window == null || window.isClosed)
            {
                window = new Window1(item2);
                window.Show();
            }
            else
            {
                window.Update(item2);
                window.Show();
            }
        }

        int LastIndex = 0;

        private void Image_KeyDown(object sender, KeyEventArgs e)
        {
            var name = "";

            if (e.Key == Key.Enter)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = item.ImageUri
                };
                Process.Start(startInfo);
            }
            else if (e.Key == Key.Delete)
            {
                var temp = item.ImageUri;

                m_arrFiles.Remove(item);
                OnPropertyChanged(nameof(m_arrFiles));

                File.Delete(temp);

                item = index < m_arrFiles.Count ? m_arrFiles[index] : m_arrFiles[m_arrFiles.Count - 1];
                name = item.ImageUri;
                this.Test.SelectedItem = item;
                item2 = index < m_arrFiles.Count ? this.m_arrFiles[index].ImageUri : m_arrFiles[m_arrFiles.Count - 1].ImageUri;
            }
            else if (e.Key == Key.Left)
            {
                if (index == 0) { }
                else
                {
                    if (Math.Abs(index - LastIndex) > 1) 
                        index = LastIndex;
        
                    index--;
                    item = m_arrFiles[index];
                    name = item.ImageUri;
                    this.Test.SelectedItem = item;
                    item2 = this.m_arrFiles[index].ImageUri;
                    Test.ScrollIntoView(item);
                }
            }
            else if (e.Key == Key.Right)
            {
                if (index == this.Test.Items.Count - 1) { }
                else
                {
                    if (Math.Abs(index - LastIndex) > 1) 
                        index = LastIndex;

                    index++;
                    item = m_arrFiles[index];
                    name = item.ImageUri;
                    this.Test.SelectedItem = item;
                    item2 = this.m_arrFiles[index].ImageUri;
                    Test.ScrollIntoView(item);
                }
            }
            else if (e.Key == Key.Up)
            {
                if (index <= 3) { }
                else
                {
                    if (Math.Abs(index - LastIndex) > 4) 
                        index = LastIndex;

                    index -= 4;
                    item = m_arrFiles[index];
                    name = item.ImageUri;
                    item2 = this.m_arrFiles[index].ImageUri;
                    this.Test.SelectedItem = item;
                    Test.ScrollIntoView(item);
                }
            }
            else if (e.Key == Key.Down)
            {
                if (index >= this.Test.Items.Count - 5) { }
                else
                {
                    if (Math.Abs(index - LastIndex) > 4) 
                        index = LastIndex;

                    index += 4;
                    item = m_arrFiles[index];
                    name = item.ImageUri;
                    this.Test.SelectedItem = item;
                    item2 = this.m_arrFiles[index].ImageUri;
                    Test.ScrollIntoView(item);
                }
            }

            if (name.Length > 0)
            {
                if (window == null || window.isClosed)
                {
                    window = new Window1(name);
                    window.Show();
                }
                else
                {
                    window.Update(name);
                    window.Show();
                }
            }

            LastIndex = index;
            e.Handled = true;
        }
    }
}
