using System;
using System.IO;
using System.Collections;
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
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace MyPlayer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private static SqLiteHelper sql;
        bool playflag = false;
        long playingID=-1;
        ImageBrush img1 = new ImageBrush();
        ImageBrush img2 = new ImageBrush();
        string musicPath;
        DispatcherTimer timer = null;
        System.Timers.Timer closeTimer = new System.Timers.Timer();
        public MainWindow()
        {
            img1.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/play.png"));
            img2.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/pause.png"));
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            sql = new SqLiteHelper("data source=config.db");
            //SQLiteDataReader reader =sql.ExecuteQuery("select name from sqlite_master where name='Music'");
            InitializeComponent();
            VolumeSlider.Value = 0.5;
            LoadConfig();
            LoadList();
        }

        public class MusicItem :ListBoxItem
        {
            public long id;
            public string name;
            public string path;
        }
        void LoadConfig()
        {
            SQLiteDataReader check = sql.ExecuteQuery("select name from sqlite_master where name='Config'");
            if (check.HasRows)
            {
                SQLiteDataReader reader = sql.ExecuteQuery("select Name,Value from Config where Name='theme'");
                if (reader.HasRows)
                {
                    reader.Read();
                    MainGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(reader.GetString(1)));
                }
                reader.Close();
            }
            check.Close();
        }
        void LoadList()
        {
            SQLiteDataReader check = sql.ExecuteQuery("select name from sqlite_master where name='Music'");
            int i=0;
            if (check.HasRows)
            {
                SQLiteDataReader reader = sql.ExecuteQuery("select ID,Name,Path from Music");
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        //ListViewItem lvi = new ListViewItem();
                        //lvi.Content = reader.GetString(0);
                        MusicItem m = new MusicItem();
                        m.Content = reader.GetString(1);
                        m.id = reader.GetInt64(0);
                        m.name = reader.GetString(1);
                        m.path = reader.GetString(2);
                        MusicList.Items.Add(m);
                        i++;
                    }
                }
                reader.Close();
            }
            check.Close();
            
        }
        int SearchMusic(string str)
        {
            DirectoryInfo directory = new DirectoryInfo(str);
            Regex reg = new Regex("[.](mp3|wav)$");
            string query;
            SQLiteDataReader reader = sql.ExecuteQuery("select name from sqlite_master where name='Music'");
            if (!reader.HasRows)
            {
                sql.ExecuteQuery("CREATE TABLE Music( ID INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Path TEXT NOT NULL);");
            }
            foreach (FileInfo finfo in directory.GetFiles())
            {
                if (!reg.IsMatch(finfo.Name)) continue;
                MusicItem m = new MusicItem();
                m.Content = finfo.Name;
                m.path = str;
                m.name = finfo.Name;
                MusicList.Items.Add(m);
                query = "INSERT INTO Music(Name,Path)VALUES('"+finfo.Name+ "','"+str+ "');";
                sql.ExecuteQuery(query);
            }
            reader.Close();
            return 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                musicPath = fbd.SelectedPath;
                SearchMusic(musicPath);
            }
        }

        void Play()
        {
            MusicItem m = MusicList.SelectedItem as MusicItem;
            MediaPlayer.Source = new Uri(m.path+"\\"+m.name,UriKind.Absolute);
            MediaPlayer.Play();
            PlayingLable.Content = m.Content;
            playingID = MusicList.SelectedIndex;
            playflag = true;
        }

        void Pause()
        {
            MediaPlayer.Pause();
            playflag = false;
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (playflag)
            {
                clip.Fill = img1;
                Pause();
            }
            else
            {
                
                clip.Fill = img2;
                if (MusicList.SelectedIndex == playingID)
                {
                    MediaPlayer.Play();
                    playflag = true;
                }
                else
                {
                    Play();
                }   
            }
        }

        private void Config(object sender, RoutedEventArgs e)
        {
            ConfigWindow cfg = new ConfigWindow();
            cfg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cfg.Owner = this;
            cfg.Show();
        }

        private void ChangeVol(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Volume = (double)VolumeSlider.Value;
        }

        private void MusicListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MusicList.SelectedIndex != playingID)
            {
                Play();
                clip.Fill = img2;;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if(MusicList.SelectedIndex == MusicList.Items.Count-1)
            {
                MusicList.SelectedIndex = 0;
            }
            else
            {
                MusicList.SelectedIndex++;
            }
            if (playflag)
            {
                Play();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (MusicList.SelectedIndex == 0)
            {
                MusicList.SelectedIndex = MusicList.Items.Count - 1;
            }
            else
            {
                MusicList.SelectedIndex--;
            }
            if (playflag)
            {
                Play();
            }
        }

        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            PlayingSlider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            //媒体文件打开成功
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();
        }
        private void timer_tick(object sender, EventArgs e)
        {
            PlayingSlider.Value = MediaPlayer.Position.TotalSeconds;
        }

        private void ChangePosition(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Position = TimeSpan.FromSeconds(PlayingSlider.Value);
        }
    }
    
}
