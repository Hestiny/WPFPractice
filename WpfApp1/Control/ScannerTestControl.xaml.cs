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

            //if (img != null)
            //{
            //    // from http://stackoverflow.com/questions/18189501/create-thumbnail-image-directly-from-header-less-image-byte-array
            //    var scale = MaxThumbnailSize / img.PixelWidth;
            //    var transform = new ScaleTransform(scale, scale);
            //    var thumbnail = new TransformedBitmap(img, transform);
            //    img = new WriteableBitmap(new TransformedBitmap(img, transform));
            //    img.Freeze();
            //}
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
                MessageBox.Show("图片传输错误:" + e.Exception.Message);
            };
            twain.DataTransferred += _session_DataTransferred;
            twain.SourceDisabled += (s, e) =>
            {
                MessageBox.Show("源数据删除");
            };
            twain.TransferReady += (s, e) =>
            {
                //MessageBox.Show("数据传输就绪");
            };

            twain.SynchronizationContext = System.Threading.SynchronizationContext.Current;
        }

        private void ShowUI_Click(object sender, RoutedEventArgs e)
        {
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
           
            System.Windows.Forms.OpenFileDialog dialog =new System.Windows.Forms.OpenFileDialog();
            dialog.ShowDialog();
        }
    }
}
