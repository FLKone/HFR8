using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using Microsoft.Phone.Shell;

namespace HFR7
{
    public partial class HFRrehost : PhoneApplicationPage
    {
        string currentTheme;
        string currentOrientation;
        CameraCaptureTask cameraCaptureTask;
        PhotoChooserTask photoChooserTask;
        BitmapImage bmp;
        IsolatedStorageFileStream myFileStream;
        byte[] imageByte;
        string urlImage;
        string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
        string pageNumber;
        string idTopic;
        string idCat;
        string topicName;
        string hash;
        string answer;
        string action;
        string numberOfPages;
        string position;
        string numRep;
        System.IO.IsolatedStorage.IsolatedStorageSettings store = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
        public HFRrehost()
        {
            InitializeComponent();
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(photoCaptureOrSelectionCompleted);
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoCaptureOrSelectionCompleted);

        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if ((string)store["disableLandscape"] == "true") HFRrehostPage.SupportedOrientations = SupportedPageOrientation.Portrait;
            // GET
            NavigationContext.QueryString.TryGetValue("pagenumber", out pageNumber);
            NavigationContext.QueryString.TryGetValue("idtopic", out idTopic);
            NavigationContext.QueryString.TryGetValue("idcat", out idCat);
            NavigationContext.QueryString.TryGetValue("topicname", out topicName);
            NavigationContext.QueryString.TryGetValue("action", out action);
            NavigationContext.QueryString.TryGetValue("numberofpages", out numberOfPages);
            NavigationContext.QueryString.TryGetValue("position", out position);
            NavigationContext.QueryString.TryGetValue("numrep", out numRep);
        }

        private void choosePicButton_Click(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();
        }

        private void takePicButton_Click(object sender, RoutedEventArgs e)
        {
            cameraCaptureTask.Show();
        }

        void photoCaptureOrSelectionCompleted(object sender, PhotoResult e)
        {
            try
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    bmp = new BitmapImage();
                    bmp.SetSource(e.ChosenPhoto);
                    convertImageToBytes(bmp);
                    //myImage.Source = bmp;
                    //myImage.Stretch = Stretch.Uniform;
                }
                else
                {
                    if (e.TaskResult.ToString() != "Cancel") MessageBox.Show("Erreur : " + e.TaskResult.ToString());
                }
            }
            catch
            {
                MessageBox.Show("Erreur interne lors du choix de la photo");
            }
        }

        private void convertImageToBytes(BitmapImage imageToConvert)
        {
            try
            {
                // Cachage du panel de sélection et affichage du panel d'attente
                choosePhotoStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitStackPanel.Visibility = System.Windows.Visibility.Visible;
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.Text = "Chargement...";

                // Create a filename for JPEG file in isolated storage.
                String tempJPEG = "TempJPEG";
                // Create virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(tempJPEG))
                {
                    myStore.DeleteFile(tempJPEG);
                }
                myFileStream = myStore.CreateFile(tempJPEG);

                WriteableBitmap wb = new WriteableBitmap(imageToConvert);
                Extensions.SaveJpeg(wb, myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                myFileStream.Position = 0;
                var br = new BinaryReader(myFileStream);
                imageByte = br.ReadBytes((int)myFileStream.Length);

                // Upload
                UploadImage();
            }
            catch (Exception myError)
            {
                MessageBox.Show("Erreur : " + myError.Message);
            }
        }
        private void UploadImage()
        {
            // Création de l'objet HttpWebRequest.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://reho.st/upload");

            request.ContentType = "multipart/form-data;boundary=" + boundary;
                
            // Méthode en POST
            request.Method = "POST";

            // Ligne spécifique à SFR : headers EXPECT 100 CONTINUE
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 7.6) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            // Démarrage de l'opération asynchrone
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }


        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                // Arrêt de l'opération
                Stream postStream = request.EndGetRequestStream(asynchronousResult);

                // Enregistrement de la requête vers le string
                StringBuilder payload = new StringBuilder();

                payload.AppendFormat("--" + boundary + "{0}", Environment.NewLine);
                payload.AppendFormat("Content-Disposition: form-data; name=\"fichier\"; filename=\"{1}\"{0}", Environment.NewLine, "fichier.jpg");
                payload.AppendFormat("Content-Type: {0}{1}{1}", "image/jpeg", Environment.NewLine);
                string postdata = Encoding.GetEncoding("iso-8859-1").GetString(imageByte, 0, imageByte.Length);
                payload.AppendLine(postdata);
                payload.AppendFormat("--" + boundary + "--");
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(payload.ToString());

                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();
                // Démarrage de l'opération asynchrone pour avoir la réponse
                request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);

            }
            catch
            {
                MessageBox.Show("Erreur lors de l'encodage de la photo.");
            }
        }


        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                string codeImage = reader.ReadToEnd();
                response.Close();

                Dispatcher.BeginInvoke(() =>
                    {
                        // Récupération de l'URL de l'image
                        int firstUrl = HttpUtility.HtmlDecode(codeImage).IndexOf("[url=http://reho.st/view/self/") + "[url=http://reho.st/view/self/".Length;
                        int lastUrl = HttpUtility.HtmlDecode(codeImage).IndexOf("][img]", firstUrl);
                        urlImage = HttpUtility.HtmlDecode(codeImage).Substring(firstUrl, lastUrl - firstUrl);

                        // Cachage du panel d'attente
                        waitStackPanel.Visibility = System.Windows.Visibility.Collapsed;

                        // Affichage des TextBox
                        copyUrlStackPanel.Visibility = System.Windows.Visibility.Visible;
                        SystemTray.ProgressIndicator.IsVisible = false;

                        // Complétion des TextBox
                        miniatureAvecLienTextBox.Text = "[url=http://reho.st/self/" + urlImage + "][img]http://reho.st/thumb/self/" + urlImage + "[/img][/url]";
                        previewAvecLienTextBox.Text = "[url=http://reho.st/self/" + urlImage + "][img]http://reho.st/preview/self/" + urlImage + "[/img][/url]";
                        reelleSansLienTextBox.Text = "[img]http://reho.st/self/" + urlImage + "[/img]";
                        urlTextBox.Text = "http://reho.st/self/" + urlImage;

                        // Fermeture des flux
                        myFileStream.Close();
                        
                    });
            }
            catch
            {
                Dispatcher.BeginInvoke(() => 
                    {
                        MessageBox.Show("Erreur. Vérifiez l'état de votre connectivité ou celle de hfr-rehost.net.");
                        NavigationService.GoBack();
                    });
            }
        }

        private void reelleSansLienTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            reelleSansLienTextBox.SelectAll();
        }

        private void previewAvecLienTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            previewAvecLienTextBox.SelectAll();
        }

        private void miniatureAvecLienTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            miniatureAvecLienTextBox.SelectAll();
        }

        private void urlTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            urlTextBox.SelectAll();
        }

        private void retourButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void previewAvecLienButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&hfrrehost=" + previewAvecLienTextBox.Text + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
        }

        private void miniatureAvecLienButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&hfrrehost=" + miniatureAvecLienTextBox.Text + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
        }

        private void urlButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&hfrrehost=" + urlTextBox.Text + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
        }

        private void reelleSansLienButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&hfrrehost=" + reelleSansLienTextBox.Text + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            store.Add("navigatedBack", "true");
        }
    }
}