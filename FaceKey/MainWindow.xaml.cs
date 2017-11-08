using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Linq;
using Microsoft.ProjectOxford.Common;
using System.Net.Sockets;

namespace FaceKey
{
    public partial class MainWindow : Window
    {
        // Replace the first parameter with your valid subscription key.        
        //        
        // Replace or verify the region in the second parameter.        
        //        
        // You must use the same region in your REST API call as you used to obtain your subscription keys.        
        // For example, if you obtained your subscription keys from the westus region, replace        
        // "westcentralus" in the URI below with "westus".        
        //        
        // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using       
        // a free trial subscription key, you should not need to change this region.        
        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("ccaadce75e7d41c6854398bf6b20e25b", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
        Face[] faces;
        // The list of detected faces.        
        String[] faceDescriptions;
        // The list of descriptions for the detected faces.       
        double resizeFactor;
        // The resize factor for the displayed image.
        String filePath;
        //open filename

        //웹캠 변수사용
        static WebCam webCam = new WebCam();
        //Directory contains image files
        string friend1ImagerDir = @"C:\Users\yyeon\Pictures\Approved_People\Bill";
        string friend2ImagerDir = @"C:\Users\yyeon\Pictures\Approved_People\Clare";
        string friend3ImagerDir = @"C:\Users\yyeon\Pictures\Approved_People\YK";

        const string personGroupId = "my_friend5";

        public MainWindow()
        {
            InitializeComponent();
        }

        ////통신
        //public static void sendImage()
        //{
        //    //클라이언트 소켓 생성
        //    Socket mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    System.Diagnostics.Debug.WriteLine("소켓생성성공");
        //    //서버와 연결
        //    String IP = "192.168.24.233";
        //    mySocket.Connect(IP, 11111);
        //    System.Diagnostics.Debug.WriteLine("소켓생성성공");
        //    //파일의 엶
        //    String name = webCam.GetImageName();
        //    System.Diagnostics.Debug.WriteLine("파일열기성공");
        //    FileStream fileStr = new FileStream(name, FileMode.Open, FileAccess.Read);
        //    //파일 크기를 가져옴
        //    int fileLength = (int)fileStr.Length;
        //    System.Diagnostics.Debug.WriteLine("파일크기 가져오기 성공");
        //    //파일크기를 서버에 전송하기 위해 바이트 배열로 변환
        //    byte[] buffer = BitConverter.GetBytes(fileLength);
        //    System.Diagnostics.Debug.WriteLine("변환성공");          
        //    //파일을 보낼 횟수
        //    int count = fileLength / 1024 + 1;
        //    //파일을 읽기 위해 BinaryRead 객체 생성
        //    BinaryReader reader = new BinaryReader(fileStr);
        //    System.Diagnostics.Debug.WriteLine("객체 생성성공");

        //    //파일송신작업
        //    for(int i=0; i < count; i++)
        //    {
        //        //파일을 읽음
        //        buffer = reader.ReadBytes(1024);
        //        //읽은 파일을 서버로 전송
        //        mySocket.Send(buffer);
        //    }
        //    System.Diagnostics.Debug.WriteLine("서버전송성공");
        //    //종료작업
        //    reader.Close();
        //    mySocket.Close();
        //}
        //Define people for the person group and detect,add person
        private async Task CreatePersonGroup()
        {
            System.Diagnostics.Debug.WriteLine("그룹만들기 시작");
            //Call the Face API.         
            try
            {
                await faceServiceClient.CreatePersonGroupAsync(personGroupId, "Approved_People");
                System.Diagnostics.Debug.WriteLine("그룹만들기");
                // Define Bill
                CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(
                    //Id of the person group that the person belonged to
                    personGroupId,
                    // Name of the person
                    "Bill");
                System.Diagnostics.Debug.WriteLine("Bill 추가");
                //Define Clare
                CreatePersonResult friend2 = await faceServiceClient.CreatePersonAsync(
                    //Id of the person group that the person belonged to
                    personGroupId,
                    //Name of the person
                    "Clare");
                System.Diagnostics.Debug.WriteLine("Clare 추가");
                //Define 연경
                CreatePersonResult friend3 = await faceServiceClient.CreatePersonAsync(
                    //id of the person group that the person belonged to
                    personGroupId,
                    //name of the person
                    "YeonKyeong");
                System.Diagnostics.Debug.WriteLine("연경 추가");

                foreach (string imagePath in Directory.GetFiles(friend1ImagerDir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        await faceServiceClient.AddPersonFaceAsync(
                            personGroupId, friend1.PersonId, s);                    
                    }
                    System.Diagnostics.Debug.WriteLine("Bill에 얼굴추가");
                }//Bill

                foreach (string imagePath in Directory.GetFiles(friend2ImagerDir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        await faceServiceClient.AddPersonFaceAsync(
                            personGroupId, friend2.PersonId, s);
                    }
                    System.Diagnostics.Debug.WriteLine("Clare에 얼굴추가");
                }//Clare
                foreach (string imagePath in Directory.GetFiles(friend3ImagerDir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        await faceServiceClient.AddPersonFaceAsync(
                            personGroupId, friend3.PersonId, s);
                    }
                    System.Diagnostics.Debug.WriteLine("연경에 얼굴추가");
                }//연경
            }
            //Catch and display Face API errors.           
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                System.Diagnostics.Debug.WriteLine("그룹만들기 FaceAPIException=" + f);
            }
            //Catch and display all other errors.          
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                System.Diagnostics.Debug.WriteLine("그룹만들기 오류 Exception=" + e);
            }
        }

        //Train the person group
        private async Task TrainPersonGroup()
        {
            try
            {
                while (true)
                {
                    await faceServiceClient.TrainPersonGroupAsync(personGroupId);
                    System.Diagnostics.Debug.WriteLine("트레이닝 시작");

                    TrainingStatus trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);
                    System.Diagnostics.Debug.WriteLine("트레이닝");
                    if (trainingStatus.Status.ToString() != "running")
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
            }
            catch (ClientException ex)
            {
                Console.WriteLine(ex.Error.Code);
                Console.WriteLine(ex.Error.Message);
                System.Diagnostics.Debug.WriteLine("트레이닝 오류" + ex);
            }
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                System.Diagnostics.Debug.WriteLine("그룹만들기 FaceAPIException=" + f);
            }
        }

        private async Task CheckGroup()
        {
            try
            {   //그룹이 이미 존재한다면 안 만들어도 된다.
                System.Diagnostics.Debug.WriteLine("그룹체크 시작");         
                PersonGroup personGroup = await faceServiceClient.GetPersonGroupAsync(personGroupId);
            }
            //Catch and display Face API errors.           
            catch (ClientException f)
            {
                System.Diagnostics.Debug.WriteLine("체크오류-그룹" + f);
                await CreatePersonGroup();
                System.Diagnostics.Debug.WriteLine("체크오류-트레이닝" + f);
                await TrainPersonGroup();
            }
            catch (FaceAPIException f)
            {
                System.Diagnostics.Debug.WriteLine("체크오류-그룹" + f);
                await CreatePersonGroup();
                System.Diagnostics.Debug.WriteLine("체크오류-트레이닝" + f);
                await TrainPersonGroup();
            }
        }

        //Displays the image and calls Detect Faces.
        private async void ImageShowAndDetect(object sender, RoutedEventArgs e)
        {
            await CheckGroup();
            //Display the image file.            
            filePath = webCam.GetImageName();
            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();
            FacePhoto.Source = bitmapSource;

            //Detect any faces in the image.           
            Title = "Detecting...";
            faces = await UploadAndDetectFaces(filePath);
            Title = String.Format("Detection Finished. {0} face(s) detected", faces.Length);
            if (faces.Length > 0)
            {
                //Prepare to draw rectangles around the faces.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                resizeFactor = 96 / dpi;
                faceDescriptions = new String[faces.Length];
                for (int i = 0; i < faces.Length; ++i)
                {
                    Face face = faces[i];
                    //Draw a rectangle on the face.   
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(
                            face.FaceRectangle.Left * resizeFactor,
                            face.FaceRectangle.Top * resizeFactor,
                            face.FaceRectangle.Width * resizeFactor,
                            face.FaceRectangle.Height * resizeFactor
                            )
                      );
                }

                drawingContext.Close();

                //Display the image with the rectangle around the face.
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;

                System.Diagnostics.Debug.WriteLine("비트맵 그리기");
            }
        }
        
       //Uploads the image file and calls Detect Faces.
        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            //Call the Face API.         
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true,
                        returnFaceLandmarks: false);
                    System.Diagnostics.Debug.WriteLine("얼굴감지");

                    //Identify face against person group
                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    System.Diagnostics.Debug.WriteLine("얼굴식별시작");
                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine("Result of face:{0}", identifyResult.FaceId);
                        System.Diagnostics.Debug.WriteLine("Result of face:{0}", identifyResult.FaceId);
                        if (identifyResult.Candidates.Length == 0)
                        {
                            MessageBox.Show("외부인이 침입했습니다. 시동을 제한합니다.");
                            System.Diagnostics.Debug.WriteLine("외부인이 침입했습니다.");
                            //sendImage();
                        }
                        else
                        {
                            //Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            MessageBox.Show("승인된 사람입니다. " + person.Name);
                            System.Diagnostics.Debug.WriteLine("Identified as {0}", person.Name);
                        }
                    }
                    return faces;
                }
            }
             //Catch and display Face API errors.           
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                System.Diagnostics.Debug.WriteLine("얼굴 식별 & 감지" + f);
                return new Face[0];
            }
            //Catch and display all other errors.          
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                System.Diagnostics.Debug.WriteLine("얼굴 식별 & 감지" + e);
                return new Face[0];
            }
        }
    }
}
