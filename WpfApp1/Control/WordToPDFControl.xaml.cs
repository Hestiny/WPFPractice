using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1.Control
{
    /// <summary>
    /// WordToPDFControl.xaml 的交互逻辑
    /// </summary>
    public partial class WordToPDFControl : UserControl
    {
        public WordToPDFControl()
        {
            InitializeComponent();
        }

        private  void SpireToPDF(String docfilePath)
        {
            Spire.Doc.Document doc = new Spire.Doc.Document();
            doc.LoadFromFile(docfilePath);
            doc.SaveToFile("C:\\Users\\kdanmobile\\Desktop\\pdf\\test\\Newpdf.pdf", Spire.Doc.FileFormat.PDF);
        }

        private void AsposeToPDF(String docfilePath)
        {
            Aspose.Words.Document doc = new Aspose.Words.Document(docfilePath);
            if (doc != null)
            {
                //doc.Save("C:\\Users\\kdanmobile\\Desktop\\pdf\\test\\Newpdf.pdf", SaveFormat.Pdf);
            }
        }

        private bool ExcelToPDF(string sourcePath,string targetPath)
        {
            bool result = false;
            Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook xls = null;
            try
            {
                application.Visible = false;
                xls = application.Workbooks.Open(sourcePath);
                xls.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF,targetPath);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            finally
            {
                xls.Close();
            }
            return result;
        }

        public bool PPTTOPDF(string sourcePath,string targetPath)
        {
            bool result = false;
            Microsoft.Office.Interop.PowerPoint.Application application = new Microsoft.Office.Interop.PowerPoint.Application();
            Microsoft.Office.Interop.PowerPoint.Presentation ppt = null;
            try
            {
                application.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
                ppt = application.Presentations.Open(sourcePath);
                ppt.ExportAsFixedFormat(targetPath, Microsoft.Office.Interop.PowerPoint.PpFixedFormatType.ppFixedFormatTypePDF);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            finally
            {
                ppt.Close();
            }
            return result;
        }

        private bool WordToPDF(string sourcePath, string targetPath)
        {
            bool result = false;
            Microsoft.Office.Interop.Word.Application application = new Microsoft.Office.Interop.Word.Application();
            Document document = null;
            try
            {
                application.Visible = false;
                document = application.Documents.Open(sourcePath);
                document.ExportAsFixedFormat(targetPath, WdExportFormat.wdExportFormatPDF);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            finally
            {
                document.Close();
            }
            return result;
        }

        #region 多线程

        public class VisualHost : FrameworkElement
        {
            Visual child;

            public VisualHost(Visual child)
            {
                if (child == null)
                    throw new ArgumentException("child");

                this.child = child;
                AddVisualChild(child);
            }

            protected override Visual GetVisualChild(int index)
            {
                return (index == 0) ? child : null;
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }
        }

        private void mainLoad()
        {
            HostVisual hostVisual = new HostVisual();

            UIElement content = new VisualHost(hostVisual);
            this.Content = content;

            Thread thread = new Thread(new ThreadStart(() =>
            {
                VisualTarget visualTarget = new VisualTarget(hostVisual);
                var control = new LoadingWait();
                control.Arrange(new Rect(new System.Windows.Point(), content.RenderSize));
                visualTarget.RootVisual = control;

                System.Windows.Threading.Dispatcher.Run();

            }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion

        private void CallRollThead(Visibility visibility)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                RollState(visibility);
            }));
        }

        private void RollState(Visibility visibility)
        {
            Roll.Visibility = visibility;
            Roll.HandleVisibleChanged(Roll.Visibility);
        }


        private void RollState()
        {
            Roll.HandleVisibleChanged(Roll.Visibility);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "*.*|*.*|.docx|*.docx|.pptx|*.pptx|.doc|*.doc|.ppt|*.ppt|.xlsx|*.xlsx";
            if (dialog.ShowDialog() == false) return;
            string _fileName = dialog.FileName;

            FilePath.Text = _fileName;
            //SpireToPDF(_fileName);
            //AsposeToPDF(_fileName);
            //Roll.Visibility = Visibility.Visible;
            //Thread t1 = new Thread(RollState);
            //t1.TrySetApartmentState(ApartmentState.STA);
            //t1.IsBackground = true;
            //t1.Start();
            //CallRollThead(Visibility.Visible);

            //WordToPDF(_fileName, "C:\\Users\\kdanmobile\\Desktop\\pdf\\test\\Newpdf.pdf");
            //PPTTOPDF(_fileName, "C:\\Users\\kdanmobile\\Desktop\\pdf\\test\\Newpdf.pdf");
            ExcelToPDF(_fileName, "C:\\Users\\kdan\\Desktop\\新建文件夹\test\\Newpdf.pdf");
            //C:\Users\kdan\Desktop\新建文件夹\test
            //t1.Abort();
            //Roll.Visibility = Visibility.Collapsed;
            //RollState();
        }
    }
}
