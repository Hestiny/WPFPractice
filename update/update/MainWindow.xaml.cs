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
using AutoUpdaterDotNET;
using Microsoft.Win32;

namespace update
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdater.Start("http://www.uuu.com:8080/AutoUpdaterTest.xml");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsRegeditItemExist())
                IsRegeditKeyExit();
            AutoUpdater.Start("http://www.uuu.com:8080/AutoUpdaterTest.xml");
        }

        private bool IsRegeditKeyExit()
        {
            string[] subkeyNames;
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE\\update\\AutoUpdater");
            //RegistryKey software = hkml.OpenSubKey("SOFTWARE\\test", true);
            subkeyNames = software.GetValueNames();
            //取得该项下所有键值的名称的序列，并传递给预定的数组中
            foreach (string keyName in subkeyNames)
            {
                if (keyName == "SkippedVersion")  //判断键值的名称
                {
                    RegistryKey key = Registry.CurrentUser;
                    RegistryKey software11 = key.OpenSubKey("software\\update\\AutoUpdater", true); //该项必须已存在
                    software11.SetValue("SkippedVersion", "");
                    hkml.Close();
                    return true;
                }
            }
            hkml.Close();
            return false;
        }

        private bool IsRegeditItemExist()
        {
            string[] subkeyNames;
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE\\update");
            //RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            subkeyNames = software.GetSubKeyNames();
            //取得该项下所有子项的名称的序列，并传递给预定的数组中
            foreach (string keyName in subkeyNames)  //遍历整个数组
            {
                if (keyName == "AutoUpdater") //判断子项的名称
                {
                    hkml.Close();
                    return true;
                }
            }
            hkml.Close();
            return false;
        }
    }

}
