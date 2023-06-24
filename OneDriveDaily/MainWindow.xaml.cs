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
using Path = System.IO.Path;

namespace OneDriveDaily
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TestyTest
    {
        public TestyTest(TestyTest2 uri, FontWeight weight)
        {
            ImageUri = uri.Name;
            Size = uri.Size + " KB";
            Weight = weight;
            try
            {
                Image = File.ReadAllBytes(uri.Name);
                var temp = BitmapFrame.Create(new MemoryStream(Image), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                Resolution = $"{temp.PixelWidth} x {temp.PixelHeight}";
                temp = null;
            }
            catch { }
        }

        public string ImageUri { get; set; }

        public string Resolution { get; set; }

        public string Size { get; set; }

        public byte[] Image { get; set; }

        public FontWeight Weight { get; set; }
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

#if DEBUG
            maxAmount = 16;
#endif

            LoadData();


            index = 0;
            m_arrPages = 1;
            m_totalPages = 1;
            m_curPage = 1;
            m_curPage0 = 0;

            ChooseFiles();
        }

        //public List<string> m_arrFiles;
        private string[] Paths = new string[6] { ".bmp", ".jpg", ".jpeg", ".png", ".jfif", ".webp" };

        private decimal maxAmount = 100;

        private DateTime _prevDate;

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

        public static DependencyProperty ListProperty5 = DependencyProperty.Register("Text9", typeof(int), typeof(MainWindow));

        public int totalPictures;
        public int m_totalPages
        {
            get { return (int)GetValue(ListProperty5); }
            set { this.SetValue(ListProperty5, value); }
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
        private string  _strFolders = String.Empty;
        private void ChooseFiles()
        {
            var arrFiles = new List<TestyTest2>();

            var systemPath = System.Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            var complete = System.IO.Path.Combine(systemPath, "files2.txt");
            try
            {
                using (StreamReader iso = new StreamReader(complete))
                {
                    _strFolders = iso.ReadLine();
                }
                if (_strFolders.Length == 0) return;
            }
            catch
            {
                return;
            }


            var arrFolders = _strFolders.Split(',');
            foreach (var item in arrFolders)
            {
                arrFiles.AddRange(CountFiles(new DirectoryInfo(item.Trim()).GetFileSystemInfos()));
            }

            m_arrFiles = new ObservableCollection<TestyTest>();
            arrFiles = arrFiles.OrderBy(c => System.IO.Path.GetFileNameWithoutExtension(c.Name)).ToList();
            
            m_arrFiles2 = arrFiles;

            m_arrPages = (int)Math.Ceiling(arrFiles.Count / maxAmount);
            m_totalPages = arrFiles.Count;

            foreach (var item in arrFiles)
            {
                m_arrFiles.Add(new TestyTest(item, FontWeights.Normal));

                if (m_arrFiles.Count == maxAmount)
                    break;
            }
        }

        public void LoadData()
        {
            var systemPath = System.Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            var complete = Path.Combine(systemPath, "files3.txt");
            DateTime prevDate = DateTime.Today;
            try
            {
                using (StreamReader iso = new StreamReader(complete))
                {
                    string dateString = iso.ReadToEnd();
                    DateTime.TryParse(dateString.Trim(), out prevDate);
                }
            }
            catch
            {
            }
            finally
            {
                _prevDate = prevDate;
            }
        }

        public void SaveData()
        {
            var systemPath = System.Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            var complete = Path.Combine(systemPath, "files3.txt");

            using (StreamWriter iso = new StreamWriter(complete, false))
            {
                iso.WriteLine(DateTime.Now.ToString());
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
                        var dateAccessed = infos[i].LastAccessTime;
                        var today = DateTime.Today;
                        var tomorrow = today.AddDays(1);

                        if (today.Day == dateEdited.Day && today.Month == dateEdited.Month)
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                        else if (today.Day == dateCreated.Day && today.Month == dateCreated.Month)
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                        else if ((today.Day == dateAccessed.Day && today.Month == dateAccessed.Month) ||
                                (dateAccessed >= _prevDate && dateAccessed < tomorrow))
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });

                        if (today.Month == 2 && today.Day == 29)
                        {
                            today = today.AddDays(1);
                            if (today.Day == dateCreated.Day && today.Month == dateCreated.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                            else if (today.Day == dateEdited.Day && today.Month == dateEdited.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024 });
                            else if (today.Day == dateAccessed.Day && today.Month == dateAccessed.Month)
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
        private bool savedData = false;

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
                {
                    m_arrFiles.Add(new TestyTest(m_arrFiles2[(m_curPage0 * (int)maxAmount) + (int)maxAmount - 1], FontWeights.Bold));

                }
                else
                {
                    if (!savedData)
                    {
                        savedData = true;
                        SaveData();
                    }
                }
                m_arrPages = (int)Math.Ceiling(m_arrFiles2.Count / maxAmount);
                m_totalPages = m_arrFiles2.Count;

                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_arrPages));
                OnPropertyChanged(nameof(m_totalPages));

#if DEBUG
#else
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(temp, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
#endif

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
            else if (e.Key == Key.F8)
            {
                var fileInfo = new FileInfo(item.ImageUri);
                File.Move(item.ImageUri, "C:\\Users\\James\\OneDrive\\Pictures\\Pictures\\Unsorted\\MoveToPhone\\" + fileInfo.Name, true);
            }
            else if (e.Key == Key.F9)
            {
                var fileInfo = new FileInfo(item.ImageUri);
                File.Copy(item.ImageUri, "C:\\Users\\James\\OneDrive\\Desktop\\" + fileInfo.Name, true);
            }
            else if (e.Key == Key.F4)
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(item.ImageUri);
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                image.Save("C:\\Users\\James\\OneDrive\\Documents\\Extensions\\Bah\\images\\background.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
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
                        item = new TestyTest(m_arrFiles2[i], FontWeights.Normal);
                    }
                    catch { }
                    if (item != null)
                        m_arrFiles.Add(item);

                    if (m_arrFiles.Count == (int)maxAmount)
                        break;
                }
                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_curPage));
                Test.ScrollIntoView(m_arrFiles[0]);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            TestyTest item = null;
            if (m_curPage < m_arrPages)
            {
                m_curPage++;
                m_curPage0++;
                if (m_curPage == m_arrPages)
                    SaveData();
                m_arrFiles = new ObservableCollection<TestyTest>();
                for (int i = m_curPage0 * (int)maxAmount; i < m_arrFiles2.Count; i++)
                {
                    item = null;
                    try
                    {
                        item = new TestyTest(m_arrFiles2[i], FontWeights.Normal);
                    }
                    catch { }
                    if (item != null)
                        m_arrFiles.Add(item);

                    if (m_arrFiles.Count == maxAmount)
                        break;
                }
                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_curPage));
                Test.ScrollIntoView(m_arrFiles[0]);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            for(var i = 0; i< m_arrFiles2.Count;)
            {
                var item = m_arrFiles2[i];

                if (File.Exists(item.Name))
                {
                    i++;
                }
                else
                {
                    m_arrFiles2.RemoveAt(i);
                }
            }

            var arrFolders = _strFolders.Split(',');
            var arrFiles = new List<TestyTest2>();

            foreach (var item in arrFolders)
            {
                arrFiles.AddRange(CountFiles(new DirectoryInfo(item.Trim()).GetFileSystemInfos()));
            }

            arrFiles = arrFiles.OrderBy(c => System.IO.Path.GetFileNameWithoutExtension(c.Name)).ToList();

            for(var i = 0; i< arrFiles.Count;)
            {
                var item = arrFiles[i];

                if (m_arrFiles2.Where(x => x.Name == item.Name).Any())
                    arrFiles.RemoveAt(i);
                else
                    i++;
            }

            m_arrFiles2.AddRange(arrFiles);

            m_arrPages = (int)Math.Ceiling(m_arrFiles2.Count / maxAmount);
            m_totalPages = m_arrFiles2.Count;

            if (m_curPage > m_arrPages)
            {
                m_curPage = 1;
                m_curPage0 = 0;
            }

            if (m_curPage == m_arrPages)
                SaveData();

            m_arrFiles = new ObservableCollection<TestyTest>();

            for (int i = m_curPage0 * (int)maxAmount; i < m_arrFiles2.Count; i++)
            {
                item = null;
                try
                {
                    item = new TestyTest(m_arrFiles2[i], FontWeights.Normal);
                }
                catch { }
                if (item != null)
                    m_arrFiles.Add(item);

                if (m_arrFiles.Count == maxAmount)
                    break;
            }

            OnPropertyChanged(nameof(m_arrFiles));
            OnPropertyChanged(nameof(m_curPage));
            OnPropertyChanged(nameof(m_totalPages));
        }
    }
}
