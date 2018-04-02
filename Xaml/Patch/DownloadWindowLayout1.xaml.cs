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
using System.Windows.Shapes;
using System.Threading;
namespace WpfApp3
{
    /// <summary>C:\Users\jsh\Desktop\MCLauncher\MCLauncher\Core\Patch\JAVAInstaller.cs
    /// DownloadWindowLayout1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DownloadWindowLayout1 : Window
    {
        public static DownloadWindowLayout1 instance;
        public PatchManager patchManager = new PatchManager();
        public DownloadWindowLayout1()
        {
            instance = this;
            InitializeComponent();
            StartPatch();
        }
        public void StartPatch()
        {

            patchManager.InitPatchList();


            patchManager.downloadCompleteDelegate = (int a, int b, int c) => {
                downloadtext.Text = "잠시만 기다려주세요...";
                this.Hide();
#if STYLE_1
                WpfApp3.Xaml.Login.Style.Style1 window = new WpfApp3.Xaml.Login.Style.Style1();
#endif
                window.Show();
                window.Activate();
                this.Close();

            };


            patchManager.PatchStart((int files, int compleate, int failed) => {
                downloadtext.Text = files.ToString() + "개의 파일중 " + compleate + "개 다운로드 완료됨";
            });


    
        }
        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
