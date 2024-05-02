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
using System.Xml.Linq;

namespace OneDriveDaily
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TestyTest
    {
        public TestyTest(TestyTest2 uri, FontWeight weight, SolidColorBrush foreground)
        {
            ImageUri = uri.Name;
            Size = uri.Size + " KB";
            Weight = weight;
            Date = uri.DateType + " - " + uri.Date.ToString();
            try
            {
                Image = File.ReadAllBytes(uri.Name);
                var temp = BitmapFrame.Create(new MemoryStream(Image), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                Resolution = $"{temp.PixelWidth} x {temp.PixelHeight}";
                temp = null;
            }
            catch { }
            Foreground = foreground;
        }

        public string ImageUri { get; set; }

        public string Resolution { get; set; }

        public string Size { get; set; }

        public byte[] Image { get; set; }

        public FontWeight Weight { get; set; }

        public SolidColorBrush Foreground { get; set; }

        public string Date { get; set; }
    }

    public class TestyTest2
    {
        public string Name { get; set; } = "";
        public string Resolution { get; set; } = "";
        public long Size { get; set; } = 0;
        public DateTime Date { get; set; }
        public string DateType { get; set; }
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
            m_nDeletedCurPage = 0;
            m_nDeletedTotal = 0;

            MaxWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth - 50;

            _numberPerRow = MaxWidth / 310;

            if (_numberPerRow != 4)
            {
                decimal value = Math.Ceiling(maxAmount / _numberPerRow);
                maxAmount = value * _numberPerRow;
            }

            _black.Freeze();
            _red.Freeze();

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

        public static DependencyProperty ListProperty9 = DependencyProperty.Register("Text11", typeof(int), typeof(MainWindow));
        public int m_nDeletedCurPage
        {
            get { return (int)GetValue(ListProperty9); }
            set { this.SetValue(ListProperty9, value); }
        }

        public static DependencyProperty ListProperty10 = DependencyProperty.Register("Text10", typeof(int), typeof(MainWindow));
        public int m_nDeletedTotal
        {
            get { return (int)GetValue(ListProperty10); }
            set { this.SetValue(ListProperty10, value); }
        }

        public static DependencyProperty ListProperty4 = DependencyProperty.Register("Text8", typeof(int), typeof(MainWindow));

        public int m_curPage0;
        public int m_curPage
        {
            get { return (int)GetValue(ListProperty4); }
            set { this.SetValue(ListProperty4, value); }
        }

        public static DependencyProperty MaxWidthProp = DependencyProperty.Register("MaxWidth", typeof(int), typeof(MainWindow));

        public int _maxWidth;
        public int MaxWidth
        {
            get { return (int)GetValue(MaxWidthProp); }
            set { this.SetValue(MaxWidthProp, value); }
        }

        public static DependencyProperty ListProperty5 = DependencyProperty.Register("Text9", typeof(int), typeof(MainWindow));

        public int totalPictures;
        public int m_totalPages
        {
            get { return (int)GetValue(ListProperty5); }
            set { this.SetValue(ListProperty5, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler == null) return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Window1 window = null;
        private List<TestyTest2> m_arrFiles2 = null;
        private string  _strFolders = String.Empty;
        private int _numberPerRow;
        private bool IsLoading = false;
        private SolidColorBrush _black = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0,0,0));
        private SolidColorBrush _red = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255,0,0));

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
                m_arrFiles.Add(new TestyTest(item, FontWeights.Normal, Regex.Match(item.Name, "[(][0-9]+[)]").Success ? _red : _black));

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
                        var startDate = DateTime.Today;//.AddDays(-1);
                        var endDate = startDate.AddDays(1);
                        if (_prevDate.Date.AddDays(1) != startDate.Date)
                        {
                            startDate = _prevDate.Date.AddDays(1);
                        }

                        if (CheckDate(startDate, endDate, dateEdited, (infos[i] as FileInfo).Attributes, false))
                        {
                            //infos[i].LastAccessTime = DateTime.Now;
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024, Date = dateEdited, DateType = "Edited" });
                        }
                        else if (CheckDate(startDate, endDate, dateCreated, (infos[i] as FileInfo).Attributes, false))
                        {
                            //infos[i].LastAccessTime = DateTime.Now;
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024, Date = dateCreated, DateType = "Created" });
                        }
                        else if (CheckDate(startDate, endDate, dateAccessed, (infos[i] as FileInfo).Attributes, true))
                        {
                            //infos[i].LastAccessTime = DateTime.Now;
                            files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024, Date = dateAccessed, DateType = "Accessed" });
                        }

                        if (startDate.Month == 2 && startDate.Day == 29)
                        {
                            startDate = startDate.AddDays(1);
                            if (startDate.Day == dateCreated.Day && startDate.Month == dateCreated.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024, Date = dateCreated, DateType = "Created" });
                            else if (startDate.Day == dateEdited.Day && startDate.Month == dateEdited.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024, Date = dateEdited, DateType = "Edited" });
                            else if (startDate.Day == dateAccessed.Day && startDate.Month == dateAccessed.Month)
                                files.Add(new TestyTest2() { Name = infos[i].FullName, Size = (infos[i] as FileInfo).Length / 1024, Date = dateAccessed, DateType = "Accessed" });
                        }
                    }
                }
            }
            return files;
        }

        private bool CheckDate(DateTime startDate, DateTime endDate, DateTime dateCompare, FileAttributes fileAttributes, bool checkCloud)
        {
            if (checkCloud)
            {
                if (dateCompare >= _prevDate || (dateCompare.Day >= startDate.Day && dateCompare.Month >= startDate.Month && dateCompare.Day < endDate.Day && dateCompare.Month <= endDate.Month))
                    return fileAttributes != (FileAttributes)5242912;
            }
            else
            {
                if (dateCompare.Day >= startDate.Day && dateCompare.Month >= startDate.Month && dateCompare.Day < endDate.Day && dateCompare.Month <= endDate.Month)
                    return true;
            }
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
        int number = 0;

        private void Image_KeyDown(object sender, KeyEventArgs e)
        {
            var name = "";
            if(IsLoading) return;

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
                    m_arrFiles.Add(new TestyTest(m_arrFiles2[(m_curPage0 * (int)maxAmount) + (int)maxAmount - 1], FontWeights.Bold, Regex.Match(m_arrFiles2[(m_curPage0 * (int)maxAmount) + (int)maxAmount - 1].Name, "[(][0-9]+[)]").Success ? _red : _black));

                }
                else
                {
                    SaveData();
                }
                m_arrPages = (int)Math.Ceiling(m_arrFiles2.Count / maxAmount);
                m_totalPages = m_arrFiles2.Count;

                m_nDeletedCurPage++;
                m_nDeletedTotal++;

                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_arrPages));
                OnPropertyChanged(nameof(m_nDeletedCurPage));
                OnPropertyChanged(nameof(m_nDeletedTotal));
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
                if (index == this.Test.Items.Count - 1) 
                {
                    if (m_curPage < m_arrPages)
                        Next_Click(null, null);
                }
                else
                {
                    if (Math.Abs(index - LastIndex) > 1)
                    {
                        index = LastIndex;
                        if (m_curPage < m_arrPages)
                            Next_Click(null, null);
                    }

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
                if (index <= _numberPerRow - 1) { }
                else
                {
                    if (Math.Abs(index - LastIndex) > _numberPerRow)
                        index = LastIndex;

                    index -= _numberPerRow;
                    item = m_arrFiles[index];
                    name = item.ImageUri;
                    item2 = this.m_arrFiles[index].ImageUri;
                    this.Test.SelectedItem = item;
                    Test.ScrollIntoView(item);
                }
            }
            else if (e.Key == Key.Down)
            {
                if (index >= this.Test.Items.Count - _numberPerRow) { }
                else
                {
                    if (Math.Abs(index - LastIndex) > _numberPerRow)
                        index = LastIndex;

                    index += _numberPerRow;
                    item = m_arrFiles[index];
                    name = item.ImageUri;
                    this.Test.SelectedItem = item;
                    item2 = this.m_arrFiles[index].ImageUri;
                    Test.ScrollIntoView(item);
                }
            }
            else if (e.Key == Key.F3)
            {
                File.Copy(item.ImageUri, $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Extensions\\Bah\\images\\background{number}.jpg", true);
                number++;
            }
            else if (e.Key == Key.F11)
            {
                var fileInfo = new FileInfo(item.ImageUri);
                File.Move(item.ImageUri, $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\Unsorted\\MoveToPhone\\" + fileInfo.Name, true);
            }
            else if (e.Key == Key.F9)
            {
                var fileInfo = new FileInfo(item.ImageUri);

                //Will remove date metadata. But program doesn't check that so I don't think it matters so much.
                //System.Drawing.Image image = System.Drawing.Image.FromFile(item.ImageUri);
                //image.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\a{fileInfo.Name}", image.RawFormat);
                
                if (fileInfo.LastWriteTime > DateTime.Today.AddDays(-35) || fileInfo.Name.StartsWith("a")) { }
                else
                {
                    File.Copy(item.ImageUri, $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\a" + fileInfo.Name, true);
                    var newFileInfo = new FileInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\a" + fileInfo.Name);
                    newFileInfo.CreationTime = DateTime.Now;
                    newFileInfo.LastWriteTime = DateTime.Now;
                }
            }
            else if (e.Key == Key.F4)
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(item.ImageUri);
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                image.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Extensions\\Bah\\images\\background{number}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                number++;
            }

            if (number > 4)
                number = 0;


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
                m_nDeletedCurPage = 0;
                m_curPage--;
                m_curPage0--;
                m_arrFiles = new ObservableCollection<TestyTest>();
                for (int i = m_curPage0 * (int)maxAmount; i < m_arrFiles2.Count; i++)
                {
                    item = null;
                    try
                    {
                        item = new TestyTest(m_arrFiles2[i], FontWeights.Normal, Regex.Match(m_arrFiles2[i].Name, "[(][0-9]+[)]").Success ? _red : _black);
                    }
                    catch { }
                    if (item != null)
                        m_arrFiles.Add(item);

                    if (m_arrFiles.Count == (int)maxAmount)
                        break;
                }
                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_curPage));
                OnPropertyChanged(nameof(m_nDeletedCurPage));
                this.Test.SelectedItem = m_arrFiles[0];
                Test.ScrollIntoView(m_arrFiles[0]);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            IsLoading = true;
            TestyTest item = null;
            if (m_curPage < m_arrPages)
            {
                m_nDeletedCurPage = 0;
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
                        item = new TestyTest(m_arrFiles2[i], FontWeights.Normal, Regex.Match(m_arrFiles2[i].Name, "[(][0-9]+[)]").Success ? _red : _black);
                    }
                    catch { }
                    if (item != null)
                        m_arrFiles.Add(item);

                    if (m_arrFiles.Count == maxAmount)
                        break;
                }
                OnPropertyChanged(nameof(m_arrFiles));
                OnPropertyChanged(nameof(m_nDeletedCurPage));
                OnPropertyChanged(nameof(m_curPage));
                this.Test.SelectedItem = m_arrFiles[0];
                index = 0;
                item = m_arrFiles[index];
                item2 = this.m_arrFiles[0].ImageUri;
                window.Update(m_arrFiles[0].ImageUri);
                window.Show();
                Test.ScrollIntoView(m_arrFiles[0]);
            }
            IsLoading = false;
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
                    item = new TestyTest(m_arrFiles2[i], FontWeights.Normal, Regex.Match(m_arrFiles2[i].Name, "[(][0-9]+[)]").Success ? _red : _black);
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
