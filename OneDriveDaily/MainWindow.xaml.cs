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
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Interop;

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
        public string Name { get; set; } = "";
        public string Resolution { get; set; } = "";
        public long Size { get; set; } = 0;
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            index = 0;
            m_arrPages = 1;
            m_curPage = 1;
            m_curPage0 = 0;

            ChooseFiles();
        }

        //public List<string> m_arrFiles;
        private string[] Paths = new string[6] { ".bmp", ".jpg", ".jpeg", ".png", ".jfif", ".webp" };

        private decimal maxAmount = 255;

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

        public static DependencyProperty ListProperty3 = DependencyProperty.Register("Text6", typeof(int), typeof(MainWindow));
        public int m_arrPages
        {
            get { return (int)GetValue(ListProperty3); }
            set { this.SetValue(ListProperty3, value); }
        }

        public static DependencyProperty ListProperty4 = DependencyProperty.Register("Text8", typeof(int), typeof(MainWindow));

        public int m_curPage0;
        public int m_curPage
        {
            get { return (int)GetValue(ListProperty4); }
            set { this.SetValue(ListProperty4, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Window1 window = null;
        private List<TestyTest2> m_arrFiles2 = null;
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

            m_arrFiles2 = arrFiles;

            m_arrPages = (int)Math.Ceiling(arrFiles.Count / maxAmount);

            foreach (var item in arrFiles)
            {
                m_arrFiles.Add(new TestyTest(item));

                if (m_arrFiles.Count == maxAmount)
                    break;
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
                        var dateCreated = infos[i].CreationTime;
                        var dateEdited = infos[i].LastWriteTime;
                        var today = DateTime.Today;

                        //var fileAdded = false;

                        /*using (var stream = new FileStream(infos[i].FullName, FileMode.Open))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var baseImage = System.Drawing.Image.FromStream(stream, false, false);

                            var index = Array.FindIndex(baseImage.PropertyIdList, 0, EndsWithSaurus);
                            if (index != -1)
                            {
                                var property = baseImage.PropertyItems[index];
                                if (property != null && property.Value != null && property.Value.Length == 6)
                                {
                                    var data = System.Text.Encoding.ASCII.GetString(property.Value).Split(':', ' ');
                                    var month = Int32.Parse(data[1]);
                                    var day = Int32.Parse(data[2]);
                                    if (day == date.Day && month == date.Month)
                                    {
                                        files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                                        fileAdded = true;
                                    }
                                }
                            }
                            baseImage.Dispose();
                        }*/

                        //var test2 = System.Drawing.Image.FromFile(infos[i].FullName, false);

                        //DateTime test3;
                        //var result = DateTime.TryParse((test.Metadata as BitmapMetadata).DateTaken, out test3);

                        //if (result && test3.Day == date.Day && test3.Month == date.Month)
                        //  files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                        //else 
                        if (dateCreated > dateEdited)
                        {
                            if (today.Day == dateEdited.Day && today.Month == dateEdited.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                        }
                        else
                        {
                            if (today.Day == dateEdited.Day && today.Month == dateEdited.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                            else if (today.Day == dateCreated.Day && today.Month == dateCreated.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                        }

                        if (today.Month == 2 && today.Day == 29)
                        {
                            today = today.AddDays(1);
                            if (today.Day == dateCreated.Day && today.Month == dateCreated.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                            else if (today.Day == dateEdited.Day && today.Month == dateEdited.Month && dateEdited.Year != dateCreated.Year)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                        }
                    }
                }
            }
            return files;
        }

        private static bool EndsWithSaurus(int num)
        {
            if (num == 36867)
                return true;
            return false;
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
                m_arrFiles2.RemoveAt(m_arrFiles2.FindIndex(m_curPage0 * (int)maxAmount, r => r.Name == item.ImageUri));
                if (m_arrFiles2.Count >= (m_curPage0 * (int)maxAmount) + (int)maxAmount)
                    m_arrFiles.Add(new TestyTest(m_arrFiles2[(m_curPage0 * (int)maxAmount) + (int)maxAmount - 1]));
                m_arrPages = (int)Math.Ceiling(m_arrFiles2.Count / maxAmount);

                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_arrPages));

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
            else if (e.Key == Key.F3)
            {
                File.Copy(item.ImageUri, "C:\\Users\\James\\OneDrive\\Documents\\Extensions\\Bah\\images\\background.jpg", true);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TestyTest item = null;
            if (m_curPage0 > 0)
            {
                m_curPage--;
                m_curPage0--;
                m_arrFiles = new ObservableCollection<TestyTest>();
                for (int i = m_curPage0 * (int)maxAmount; i < m_arrFiles2.Count; i++)
                {
                    item = null;
                    try
                    {
                        item = new TestyTest(m_arrFiles2[i]);
                    }
                    catch { }
                    if (item != null)
                        m_arrFiles.Add(item);

                    if (m_arrFiles.Count == (int)maxAmount)
                        break;
                }
                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_curPage));
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            TestyTest item = null;
            if (m_curPage < m_arrPages)
            {
                m_curPage++;
                m_curPage0++;
                m_arrFiles = new ObservableCollection<TestyTest>();
                for (int i = m_curPage0 * (int)maxAmount; i < m_arrFiles2.Count; i++)
                {
                    item = null;
                    try
                    {
                        item = new TestyTest(m_arrFiles2[i]);
                    }
                    catch { }
                    if (item != null)
                        m_arrFiles.Add(item);

                    if (m_arrFiles.Count == maxAmount)
                        break;
                }
                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_curPage));
            }
        }
    }
}
