using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace Shukalova_KIUKI_19_4_Project
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void СreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            treeViewUsers.Items.Clear();
            SortedDictionary<long, DirectoryInfo> users = getAllUsers();
            string labelStr = null;

            foreach (KeyValuePair<long, DirectoryInfo> keyValue in users)
            {
                TreeViewItem item = new TreeViewItem();
                item.Tag = keyValue.Value;
                item.Header = keyValue.Value.ToString();
                item.Items.Add("*");
                treeViewUsers.Items.Add(item);
                labelStr += keyValue.Value.Name + " --> " + keyValue.Key + " bytes\n";
            }

            lblsize.Content = labelStr.Trim();
        }

        private void treeViewUsers_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo dir = (DirectoryInfo)item.Tag;
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.ToString();
                    newItem.Items.Add("*");
                    item.Items.Add(newItem);
                }
            }
            catch
            { }
        }

        private void treeViewUsers_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvItem = (TreeViewItem)e.OriginalSource;
            DirectoryInfo dir = (DirectoryInfo)tvItem.Tag;
            MessageBox.Show("Выбранная папка: " + dir.FullName + "   Размер: " + getAllSize(dir.FullName) + " байт", "Info");
        }

        private SortedDictionary<long, DirectoryInfo> getAllUsers()
        {
            SortedDictionary<long, DirectoryInfo> usersDict = new SortedDictionary<long, DirectoryInfo>(new DescendingComparer<long>());
            ManagementObjectSearcher wmiQuery = new ManagementObjectSearcher("SELECT * FROM Win32_Account");
            ManagementObjectCollection results = wmiQuery.Get();
            foreach (ManagementObject result in results)
            {
                string pathAppData = string.Format("C:\\Users\\{0}", (string)result["Name"]);
                if (Directory.Exists(pathAppData))
                {
                    usersDict.Add(getAllSize(pathAppData), new DirectoryInfo(pathAppData));
                }
            }

            return usersDict;
        }

        private long getAllSize(string rootPath, long alreadyFoundSize = 0)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(rootPath);
                var dirs = di.EnumerateDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    alreadyFoundSize = getAllSize(dir.FullName, alreadyFoundSize);
                }

                var files = Directory.EnumerateFiles(rootPath);
                foreach (string name in files)
                {
                    FileInfo info = new FileInfo(name);
                    alreadyFoundSize += info.Length;
                }
            }
            catch
            { }

            return alreadyFoundSize;
        }
    }
    class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return y.CompareTo(x);
        }
    }
}