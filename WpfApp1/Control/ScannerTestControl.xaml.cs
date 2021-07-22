using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1.Contorl
{
    /// <summary>
    /// ScannerTestControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScannerTestControl : UserControl
    {
        /// <summary>
        /// 是否保存到本地文件夹的标志;点击事件开启,传输事件后关闭
        /// </summary>
        private bool IsScanner = false;

        private TwainSession twain;
        public DataSource DS { get; set; }
        private List<DataSource> DSList=new List<DataSource>();
        private DataSource SelectSource;
        public ScannerTestControl()
        {
            InitializeComponent();
            InitScanner();
        }

        private void InitScanner()
        {
            SetupTwain();
        }

        private void OpenSaveScanner()
        {
            IsScanner = true;
        }

        private void CloseSaveScanner()
        {
            IsScanner = false;
        }

        /// <summary>
        /// 检查是否连接上扫描仪
        /// </summary>
        /// <returns>有连接的扫描仪为true,否则为false</returns>
        private bool ScannerChecked()
        {
            if (ScannerList.SelectedItem == null)
                return false;
            return true;
        }

        void _session_TransferReady(object sender, TransferReadyEventArgs e)
        {
            
            var mech = twain.CurrentSource.Capabilities.ICapXferMech.GetCurrent();
            if (mech == XferMech.File)
            {
                var formats = twain.CurrentSource.Capabilities.ICapImageFileFormat.GetValues();
                var wantFormat = formats.Contains(FileFormat.Tiff) ? FileFormat.Tiff : FileFormat.Bmp;

                var fileSetup = new TWSetupFileXfer
                {
                    Format = wantFormat,
                    FileName = GetUniqueName(System.IO.Path.GetTempPath(), "twain-test", "." + wantFormat)
                };
                var rc = twain.CurrentSource.DGControl.SetupFileXfer.Set(fileSetup);
            }
            else if (mech == XferMech.Memory)
            {
                // ?

            }
        }

        string GetUniqueName(string dir, string name, string ext)
        {
            var filePath =System.IO.Path.Combine(dir, name + ext);
            int next = 1;
            while (File.Exists(filePath))
            {
                filePath = System.IO.Path.Combine(dir, string.Format("{0} ({1}){2}", name, next++, ext));
            }
            return filePath;
        }

        void _session_DataTransferred(object sender, DataTransferredEventArgs e)
        {
            //System.IO.File.WriteAllBytes(FilePath.Content.ToString(), e.GetNativeImageStream());
            if (IsScanner)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(e.GetNativeImageStream().ConvertToWpfBitmap(300, 0)));
                FileStream fileStream = new FileStream(FilePath.Content.ToString(), FileMode.Create, FileAccess.ReadWrite);
                encoder.Save(fileStream);
                fileStream.Close();
                CloseSaveScanner();
            }
           
            ImageSource img = GenerateThumbnail(e);
            if (img != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ScannerImage.Source = img;
                }));
            }
        }

        private ImageSource GenerateThumbnail(DataTransferredEventArgs e)
        {
            BitmapSource img = null;

            switch (e.TransferType)
            {
                case XferMech.Native:
                    using (var stream = e.GetNativeImageStream())
                    {
                        if (stream != null)
                        {
                            img = stream.ConvertToWpfBitmap(300, 0);
                        }
                    }
                    break;
                case XferMech.File:
                    img = new BitmapImage(new Uri(e.FileDataPath));
                    if (img.CanFreeze)
                    {
                        img.Freeze();
                    }
                    break;
                case XferMech.Memory:
                    // TODO: build current image from multiple data-xferred event
                    break;
            }
            return img;
        }

        private void SetupTwain()
        {
            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetEntryAssembly());
            twain = new TwainSession(appId);
            twain.Open();
            foreach (var s in twain.Select(s => new  { DS = s}))
            {
                DSList.Add(s.DS);
            }
            ScannerList.ItemsSource = DSList;
            if (DSList != null)
                ScannerList.SelectedItem = DSList.FirstOrDefault();

            twain.TransferError += (s, e) =>
            {
                CloseSaveScanner();
                MessageBox.Show("图片传输错误:" + e.Exception.Message);
            };
            twain.DataTransferred += _session_DataTransferred;
            twain.SourceDisabled += (s, e) =>
            {
                //MessageBox.Show("源数据已清除");
            };
            twain.TransferReady += (s, e) =>
            {
                //MessageBox.Show("数据传输就绪");
            };

            twain.SynchronizationContext = System.Threading.SynchronizationContext.Current;
        }

        #region 界面事件

        private void ShowUI_Click(object sender, RoutedEventArgs e)
        {
            if (!ScannerChecked())
            {
                MessageBox.Show("没有连接扫描仪");
                return;
            }
            if (FilePath.Content.ToString() == "ClickMe")
            {
                MessageBox.Show("选择保存的文件路径");
                return;
            }
            //直接打开文件选择对话框
            //if (FilePath.Content.ToString() == "ClickMe")
            //{
            //    FilePath_Click(FilePath, null);
            //}
            OpenSaveScanner();
            var windowsPtr = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            ReturnCode a = twain.CurrentSource.Enable(SourceEnableMode.ShowUI, false, windowsPtr);
        }

        private void ScannerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (twain.State == 4)
            {
                twain.CurrentSource.Close();
            }
            if (sender != null)
            {
                ((sender as ListView).SelectedItem as DataSource).Open();
            }
        }

        private void FilePath_Click(object sender, RoutedEventArgs e)
        {
            //文件夹对话弹窗
            //System.Windows.Forms.FolderBrowserDialog dialog =new System.Windows.Forms.FolderBrowserDialog();
            //dialog.ShowDialog();
            //if (dialog.SelectedPath == null)
            //{
            //    (sender as Button).Content = "ClickMe";
            //}
            //else
            //{
            //    (sender as Button).Content = dialog.SelectedPath.Trim();
            //}
            System.Windows.Forms.SaveFileDialog saveDg = new System.Windows.Forms.SaveFileDialog();
            saveDg.Filter = "pdf (*.pdf)|*.pdf |png (*.png)|*.png";
            saveDg.DefaultExt = "png"; 
            saveDg.FileName = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + "";
            saveDg.AddExtension = true;
            saveDg.RestoreDirectory = true;
            if (saveDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (saveDg.FileName == null)
                {
                    (sender as Button).Content = "ClickMe";
                }
                else
                {
                    (sender as Button).Content = saveDg.FileName.Trim();
                }
            }
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            if (!ScannerChecked())
            {
                MessageBox.Show("没有连接扫描仪");
                return;
            }
            var windowsPtr = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            twain.CurrentSource.Enable(SourceEnableMode.NoUI, false, windowsPtr);
        }

        private void Scanner_Click(object sender, RoutedEventArgs e)
        {
            if (!ScannerChecked())
            {
                MessageBox.Show("没有连接扫描仪");
                return;
            }
            if (FilePath.Content.ToString() == "ClickMe")
            {
                MessageBox.Show("选择保存的文件路径");
                return;
            }
            OpenSaveScanner();
            var windowsPtr = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            twain.CurrentSource.Enable(SourceEnableMode.NoUI, false, windowsPtr);
        }

        #endregion
    }
}
