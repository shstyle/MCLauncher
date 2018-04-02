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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp3.Xaml.Login.Syle1
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
         
            InitializeComponent();
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = this.Height;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(850));
            animation.EasingFunction = new System.Windows.Media.Animation.QuadraticEase();
            this.BeginAnimation(HeightProperty, animation);
            
            id.PreviewMouseDown += (object sender, MouseButtonEventArgs e) => {
                id.Text = "";
            };
            pwd.PreviewMouseDown += (object sender, MouseButtonEventArgs e) => {
                pwd.Password = "";
            };
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(id.Text))
            {
                MessageBox.Show("아이디를 입력해주세요!");
                return;
            }

            if (string.IsNullOrEmpty(pwd.Password))
            {
                MessageBox.Show("비밀번호를 입력해주세요!");
                return;
            }

            var result = await MCLoginLib.Launcher.Login(
                id.Text,
                pwd.Password, 
                () => { }, 
                () => { });


            result = true;
            if (result)
            {
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = Width;
                animation.To = 0;
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(850));
                animation.EasingFunction = new System.Windows.Media.Animation.QuadraticEase();
                animation.Completed += async (object p, EventArgs e1) =>
                {
                    this.Hide();
                    MCLoginLib.Launcher.StartMineCraft();
                    this.Close();
       
                };
                this.BeginAnimation(WidthProperty, animation);

            }
            else
            {
                
            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
            this.Close();
        }
    }
}
