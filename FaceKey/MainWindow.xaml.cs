using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using System.Linq;

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
            new FaceServiceClient("a5b89150a0a648858b1ab1f16db3a253", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
        Face[] faces;
        // The list of detected faces.        
        String[] faceDescriptions;
        // The list of descriptions for the detected faces.       
        double resizeFactor;
        // The resize factor for the displayed image.
        String filePath;
        //open filename

        //Directory contains image files
        string friend1ImagerDir = @"C:\Users\yyeon\Pictures\Approved_People\Anna";
        string friend2ImagerDir = @"C:\Users\yyeon\Pictures\Approved_People\Bill";
        string friend3ImagerDir = @"C:\Users\yyeon\Pictures\Approved_People\Clare";

        const string personGroupId = "my_friend2";

        public MainWindow()
        {
            InitializeComponent();
        }

        //Define people for the person group and detect,add person
        private async Task CreatePersonGroup()
        {
            System.Diagnostics.Debug.WriteLine("그룹만들기 시작");
            //Call the Face API.         
            try
            {
                await faceServiceClient.CreatePersonGroupAsync(personGroupId, "Approved_People");
                System.Diagnostics.Debug.WriteLine("그룹만들기");
                //Define Anna
                CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(
                   // Id of the person group that the person belonged to
                   personGroupId,
                    //Name of the person
                    "Anna");
                System.Diagnostics.Debug.WriteLine("Anna 추가");
                // Define Bill
                CreatePersonResult friend2 = await faceServiceClient.CreatePersonAsync(
                    //Id of the person group that the person belonged to
                    personGroupId,
                    // Name of the person
                    "Bill");
                System.Diagnostics.Debug.WriteLine("Bill 추가");
                //Define Clare
                CreatePersonResult friend3 = await faceServiceClient.CreatePersonAsync(
                    //Id of the person group that the person belonged to
                    personGroupId,
                    //Name of the person
                    "Clare");
                System.Diagnostics.Debug.WriteLine("Clare 추가");

                foreach (string imagePath in Directory.GetFiles(friend1ImagerDir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        System.Diagnostics.Debug.WriteLine("이미지 들어가는 중");
                        //Detect faces in the image and add to Anna
                        await faceServiceClient.AddPersonFaceAsync(
                            personGroupId, friend1.PersonId, s);

                    }
                    System.Diagnostics.Debug.WriteLine("Anna에 얼굴추가");
                }//Anna

                foreach (string imagePath in Directory.GetFiles(friend2ImagerDir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        await faceServiceClient.AddPersonFaceAsync(
                            personGroupId, friend2.PersonId, s);

                    }
                    System.Diagnostics.Debug.WriteLine("Bill에 얼굴추가");
                }//Bill

                foreach (string imagePath in Directory.GetFiles(friend3ImagerDir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        await faceServiceClient.AddPersonFaceAsync(
                            personGroupId, friend3.PersonId, s);

                    }
                    System.Diagnostics.Debug.WriteLine("Clare에 얼굴추가");
                }//Clare
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
            catch (Microsoft.ProjectOxford.Common.ClientException ex)
            {
                Console.WriteLine(ex.Error.Code);
                Console.WriteLine(ex.Error.Message);
                System.Diagnostics.Debug.WriteLine("트레이닝 오류" + ex);
            }
        }

        //Displays the image and calls Detect Faces.
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PersonGroup personGroup = await faceServiceClient.GetPersonGroupAsync(personGroupId);
                string PGID = personGroup.PersonGroupId;//그룹이 이미 존재한다면 안 만들어도 된다.

                if (PGID != personGroupId)
                {
                    await CreatePersonGroup();
                    await TrainPersonGroup();
                }
            }
             //Catch and display Face API errors.           
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorMessage, f.ErrorCode);
            }
            //Catch and display all other errors.          
            catch (Exception a)
            {
                MessageBox.Show(a.Message, "Error");
            }

            //Get the image file to scan from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();
            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";

            bool? result = openDlg.ShowDialog(this);
            //Return if canceled.
            if (!(bool)result)
                {
                    return;
                }
            //Display the image file.            
            filePath = openDlg.FileName;
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
                    //Store the face description
                    faceDescriptions[i] = FaceDescription(face);
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

                //Set the status bar.text
                faceDescriptionStatusBar.Text = "Place the mouse pointer over a face to see the face description.";
                System.Diagnostics.Debug.WriteLine("비트맵 그리기");
            }
        }
        //Displays the face description when the mouse is over a face rectangle.
        private void FacePhoto_MouseMove(object sender, MouseEventArgs e)
        {
            //If the REST call has not completed, return from this method.
            if (faces == null)
                return;
            //Find the mouse position relative to the image.            
            Point mouseXY = e.GetPosition(FacePhoto);
            ImageSource imageSource = FacePhoto.Source;
            BitmapSource bitmapSource = (BitmapSource)imageSource;
            //Scale adjustment between the actual size and displayed size.
           var scale = FacePhoto.ActualWidth / (bitmapSource.PixelWidth / resizeFactor);
            //Check if this mouse position is over a face rectangle.            
            bool mouseOverFace = false;
            for (int i = 0; i < faces.Length; ++i)
            {
                FaceRectangle fr = faces[i].FaceRectangle;
                double left = fr.Left * scale;
                double top = fr.Top * scale;
                double width = fr.Width * scale;
                double height = fr.Height * scale;
                //Display the face description for this face if the mouse is over this face rectangle.                
                if (mouseXY.X >= left && mouseXY.X <= left + width && mouseXY.Y >= top && mouseXY.Y <= top + height)
                {
                    faceDescriptionStatusBar.Text = faceDescriptions[i];
                    mouseOverFace = true;
                    break;
                }
            }
            //If the mouse is not over a face rectangle.
            if (!mouseOverFace)
                faceDescriptionStatusBar.Text = "Place the mouse pointer over a face to see the face description.";
            System.Diagnostics.Debug.WriteLine("마우스 이동");
        }

        //Uploads the image file and calls Detect Faces.
        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            //The list of Face attributes to return.
           IEnumerable < FaceAttributeType > faceAttributes =
               new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };
            //Call the Face API.         
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true,
                        returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
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
        //Returns a string that describes the given face.
        private string FaceDescription(Face face)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Face: ");
            //Add the gender, age, and smile.            
            sb.Append(face.FaceAttributes.Gender);
            sb.Append(face.FaceAttributes.Gender);
            sb.Append(", ");
            sb.Append(face.FaceAttributes.Age);
            sb.Append(", ");
            sb.Append(String.Format("smile {0:F1}%, ", face.FaceAttributes.Smile * 100));
            //Add the emotions.Display all emotions over 10 %.
          sb.Append("Emotion: ");
            EmotionScores emotionScores = face.FaceAttributes.Emotion;

            if (emotionScores.Anger >= 0.1f) sb.Append(String.Format("anger {0:F1}%, ", emotionScores.Anger * 100));
            if (emotionScores.Contempt >= 0.1f) sb.Append(String.Format("contempt {0:F1}%, ", emotionScores.Contempt * 100));
            if (emotionScores.Disgust >= 0.1f) sb.Append(String.Format("disgust {0:F1}%, ", emotionScores.Disgust * 100));
            if (emotionScores.Fear >= 0.1f) sb.Append(String.Format("fear {0:F1}%, ", emotionScores.Fear * 100));
            if (emotionScores.Happiness >= 0.1f) sb.Append(String.Format("happiness {0:F1}%, ", emotionScores.Happiness * 100));
            if (emotionScores.Neutral >= 0.1f) sb.Append(String.Format("neutral {0:F1}%, ", emotionScores.Neutral * 100));
            if (emotionScores.Sadness >= 0.1f) sb.Append(String.Format("sadness {0:F1}%, ", emotionScores.Sadness * 100));
            if (emotionScores.Surprise >= 0.1f) sb.Append(String.Format("surprise {0:F1}%, ", emotionScores.Surprise * 100));
            //Add glasses.            
            sb.Append(face.FaceAttributes.Glasses);
            sb.Append(", ");
            //Add hair.            
            sb.Append("Hair: ");
            //Display baldness confidence if over 1 %.
            if (face.FaceAttributes.Hair.Bald >= 0.01f)
                sb.Append(String.Format("bald {0:F1}% ", face.FaceAttributes.Hair.Bald * 100));
            //Display all hair color attributes over 10 %.
           HairColor[] hairColors = face.FaceAttributes.Hair.HairColor;
            foreach (HairColor hairColor in hairColors)
            {
                if (hairColor.Confidence >= 0.1f)
                {
                    sb.Append(hairColor.Color.ToString());
                    sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
                }
            }
            //Return the built string.
           System.Diagnostics.Debug.WriteLine("얼굴설명");
            return sb.ToString();
        }
    }
}
