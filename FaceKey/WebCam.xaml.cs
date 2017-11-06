using System;
using System.Windows;
using WebEye.Controls.Wpf;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Threading;

namespace FaceKey
{

    public partial class WebCam
    {
        public static String dir = @"C:\WebCamSnapshots\";
        //1장의 사진만 찍을 수 있도록 세는 변수
        static int time = 0;
        DispatcherTimer timer = new DispatcherTimer();
        public static String ImageName;

        public String GetImageName()
        {
            return dir + ImageName;
        }
        
        public WebCam()
        {
            InitializeComponent();
        }

        private void ShowCamera(object sender, RoutedEventArgs e)
        {
            var cameras = webCameraControl.GetVideoCaptureDevices().ToArray();
            var cameraId = (WebCameraId)cameras[0];
            webCameraControl.StartCapture(cameraId);
            //5초 후 사진 찍기
            timer.Interval = TimeSpan.FromMilliseconds(3500);
            timer.Tick += new EventHandler(CameraCapture);
            timer.Start();
        }
        //저장할 이미지의 파일 이름 얻는 함수
        private static string GetNewFileName()
        {
            Regex reg = new Regex(@"image(\d)+[.]");

            var list = Directory.GetFiles(dir, "*.jpg").Where(path => reg.IsMatch(path))
                     .ToList();

            if (list.Count == 0)
                return "image1.jpg";

            var lastName =
                list.Select(x => (new FileInfo(x)).Name.Replace("image", "").Replace(".jpg", "")).OrderBy(x => x).Last();
           
            ImageName= string.Format("image{0}.jpg", int.Parse(lastName) + 1);
            return ImageName;
        }

        private void CameraCapture(object sender, EventArgs e)
        {
            time++;
            if (time >= 1)
            {
                webCameraControl.GetCurrentImage().Save(dir + GetNewFileName());
                System.Diagnostics.Debug.WriteLine("사진저장성공");
                timer.Stop();

                if (!timer.IsEnabled)
                {
                    CloseAndNext();
                }
                System.Diagnostics.Debug.WriteLine("3.5초 지남");
                System.Diagnostics.Debug.WriteLine(GetImageName()); 
            }
        }
        //카메라 끄고 다음 화면으로 넘어가기
        private void CloseAndNext()
        {
            System.Diagnostics.Debug.WriteLine("타이머 꺼짐");
            webCameraControl.StopCapture();
            MainWindow face = new MainWindow();
            App.Current.MainWindow = face;
            this.Close();
            face.Show();

        }
    }
}
