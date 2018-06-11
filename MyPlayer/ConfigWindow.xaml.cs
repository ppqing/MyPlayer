using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyPlayer
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private static SqLiteHelper sql;
        public ConfigWindow()
        {
            sql = new SqLiteHelper("data source=config.db");
            InitializeComponent();
            LoadConfig();
        }
        void LoadConfig()
        {
            //MainWindow mainWindow = (MainWindow)this.Owner;
            SQLiteDataReader check = sql.ExecuteQuery("select name from sqlite_master where name='Config'");
            if (check.HasRows)
            {
                SQLiteDataReader reader = sql.ExecuteQuery("select Name,Value from Config where Name='theme'");
                if (reader.HasRows)
                {
                    reader.Read();
                    ConfigGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(reader.GetString(1)));
                }
                reader.Close();
            }
            check.Close();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)this.Owner;
            Regex reg = new Regex("^#[0-9a-fA-F]{8}$");
            string query;
            bool OK = true;
            if (reg.IsMatch(ARGB.Text))
            {
                mainWindow.MainGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ARGB.Text));
                //ConfigGrid.Background= new SolidColorBrush((Color)ColorConverter.ConvertFromString(ARGB.Text));
                SQLiteDataReader reader = sql.ExecuteQuery("select name from sqlite_master where name='Config'");
                if (!reader.HasRows)
                {
                    sql.ExecuteQuery("CREATE TABLE Config( Name TEXT NOT NULL,Value TEXT NOT NULL);");
                    query = "INSERT INTO Config(Name,Value)VALUES('theme','"+ARGB.Text+ "');";
                    sql.ExecuteQuery(query);
                }
                else
                {
                    query = "UPDATE Config SET Value='"+ARGB.Text+"' WHERE Name='theme'";
                    sql.ExecuteQuery(query);
                }

            }
            else if(ARGB.Text.Equals("请输入ARGB"))
            {
               //不做任何事
            }
            else
            {
                MessageBox.Show("ARGB输入有错，请确认是否为#FFFFFFFF的格式");
                ARGB.Text = "请输入ARGB";
                OK = false;
            }
            if (OK)
            {
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string query;
            SQLiteDataReader reader = sql.ExecuteQuery("select name from sqlite_master where name='Config'");
            if (reader.HasRows)
            {
                query = "UPDATE Config SET Value='#FFB8B6F4' WHERE Name='theme'";
                sql.ExecuteQuery(query);
                MessageBox.Show("恢复默认值成功");
                //this.Close();
            }
        }
    }
}
