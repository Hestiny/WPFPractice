using MaterialDesignThemes.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.Contorl;
using WpfApp1.Control;
using WpfApp1.ViewModel;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitMenu();
        }

        private void InitMenu()
        {
            var menuRegister = new List<SubItem>();
            menuRegister.Add(new SubItem("扫描仪", new ScannerTestControl()));
            menuRegister.Add(new SubItem("打印机"));
            var item1 = new ItemMenu("设备", menuRegister, PackIconKind.Scanner);

            var menuSchedule = new List<SubItem>();
            var item2 = new ItemMenu("测试", menuSchedule, PackIconKind.Bat);
            Menu.Children.Add(new MenuItemControl(item1, this));
            Menu.Children.Add(new MenuItemControl(item2, this));
        }
        internal void SwitchScreen(object sender)
        {
            var screen = ((UserControl)sender);

            if (screen != null)
            {
                StackPanelMain.Children.Clear();
                StackPanelMain.Children.Add(screen);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftPanel.Width == 250)
                LeftPanel.Width = 30;
            else LeftPanel.Width = 250;
        }
    }
}
