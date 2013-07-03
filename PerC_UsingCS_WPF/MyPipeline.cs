using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace PerC_UsingCS_WPF
{
    class MyPipeline : UtilMPipeline
    {
        public MyPipeline()
            : base()
        {
            EnableVoiceRecognition();
            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB32);
            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);
        }

        public override void OnRecognized(ref PXCMVoiceRecognition.Recognition data)
        {
            String content = "您剛剛說的是 : " + data.dictation + ",可靠度為 : " + data.confidence; 
            MainWindow.mainwin.SetVoiceRegcognitionResult(content);
            Response(data.dictation.Trim().ToLower());
        }

        void Response(string content)
        {
            if (content == "how are you")
                Speak("我很好");
            else if (content == "hello")
                Speak("哈囉");
            else if (content == "good night")
                SendCommand("{Right}");
            else if (content == "good morning")
                SendCommand("{Left}");

        }
        void SendCommand(string command)
        {
            SendKeys.SendWait(command);
        }
        void Speak(string response)
        {
            Type type = Type.GetTypeFromProgID("SAPI.SpVoice");
            dynamic synthesizer = Activator.CreateInstance(type);

            synthesizer.Rate = 0;
            synthesizer.Volume = 100;
            synthesizer.Speak(response);
        }
        public override void OnImage(PXCMImage image)
        {
            //Debug.WriteLine("收到影像,格式 : " + image.imageInfo.format);
            PXCMSession session = QuerySession();
            Bitmap bitmapimage; 
            image.QueryBitmap(session, out bitmapimage);
            BitmapSource source = ToWpfBitmap(bitmapimage);
            if (image.imageInfo.format == PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH)
                MainWindow.mainwin.ProcessDepthImage(source);
            else if (image.imageInfo.format == PXCMImage.ColorFormat.COLOR_FORMAT_RGB24)
                MainWindow.mainwin.ProcessColorImage(source);

            //base.OnImage(image);
        }


        public static BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}
