﻿using System;
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
        static String dir = @"C:\WebCamSnapshots\";
        //1장의 사진만 찍을 수 있도록 세는 변수
        static int time = 0;
        DispatcherTimer timer = new DispatcherTimer();

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
            timer.Interval = TimeSpan.FromMilliseconds(5000);
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
            
            return string.Format("image{0}.jpg",int.Parse(lastName)+1);
        }

        private void CameraCapture(object sender, EventArgs e)
        {
            time++;
            if (time > 1)
            {
                System.Diagnostics.Debug.WriteLine("5초 지남");
                timer.Stop();
                return;
            }
            webCameraControl.GetCurrentImage().Save(dir+GetNewFileName());
        }
    }
}