using System;
using System.Net;
using System.Text;
using System.IO;
using System.Windows.Threading;
using System.Windows;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Used for downloading and parsing html from the internet
    /// </summary>
    public class HtmlWeb
    {
        string postReq = "";
        #region Delegates

        /// <summary>
        /// Represents the method that will handle the PreHandleDocument event.
        /// </summary>
        public delegate void PreHandleDocumentHandler(HtmlDocument document);

        #endregion

        #region Fields

        /// <summary>
        /// Occurs before an HTML document is handled.
        /// </summary>
        public PreHandleDocumentHandler PreHandleDocument;

        #endregion

        #region Instance Methods

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="container">Cookie</param>

        public void LoadAsync(string url, CookieContainer container)
        {
            LoadAsync(new Uri(url), container, null, null, null);
        }

        public void LoadAsync(string url, CookieContainer container, string[] postVar)
        {
            LoadAsync(new Uri(url), container, null, null, postVar);
        }

        

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="uri">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public void LoadAsync(Uri uri, CookieContainer container, Encoding encoding, NetworkCredential credentials, string[] postVar)
        {
            //var client = new WebClient();
            var request = (HttpWebRequest)WebRequest.Create(uri);

            if (postVar != null)
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                foreach (string var in postVar) 
                {
                    postReq = postReq + "&" + var;
                }
            }
            request.Headers["Cache-Control"] = "no-cache";
            request.Headers["Pragma"] = "no-cache";
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 7.0; Windows Phone OS 7.0; IEMobile/7.0) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            if (credentials == null)
                request.UseDefaultCredentials = true;
            else
                request.Credentials = credentials;

            if (container != null) request.CookieContainer = container;
            
            //client.DownloadStringCompleted += ClientDownloadStringCompleted;

            //client.DownloadStringAsync(uri);
            if (postVar != null)
            {
                // Démarrage de l'opération asynchrone
                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
            }
            else
            {
                var result = (IAsyncResult)request.BeginGetResponse(ResponseCallback, request);
            }
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);
            if (postReq != "")
            {
                // Conversion du string en byte
                byte[] byteArray = Encoding.UTF8.GetBytes(postReq);
                // Enregistrement de la requête vers le string
                postStream.Write(byteArray, 0, postReq.Length);
                postStream.Close();
            }

            // Démarrage de l'opération asynchrone pour avoir la réponse
            var result = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        private void OnLoadCompleted(HtmlDocumentLoadCompleted htmlDocumentLoadCompleted)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, htmlDocumentLoadCompleted);
        }

        #endregion

        #region Event Handling

        private void ResponseCallback(IAsyncResult result)
        {
            try
            {
                var request = (HttpWebRequest)result.AsyncState;
                var response = request.EndGetResponse(result);

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();
                    //Dispatcher.BeginInvoke(() => { httpWebRequestTextBlock.Text = contents; });

                    try
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(contents);
                        if (PreHandleDocument != null)
                            PreHandleDocument(doc);

                        OnLoadCompleted(new HtmlDocumentLoadCompleted(doc));
                    }
                    catch (Exception err)
                    {
                        OnLoadCompleted(new HtmlDocumentLoadCompleted(err));
                    }
                }
            }
            catch (Exception err)
            {
                OnLoadCompleted(new HtmlDocumentLoadCompleted(err));
            }
        }

        //private void ClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //        OnLoadCompleted(new HtmlDocumentLoadCompleted(e.Error));
        //    try
        //    {
        //        var doc = new HtmlDocument();
        //        doc.LoadHtml(e.Result);
        //        if (PreHandleDocument != null)
        //            PreHandleDocument(doc);

        //        OnLoadCompleted(new HtmlDocumentLoadCompleted(doc));
        //    }
        //    catch (Exception err)
        //    {
        //        OnLoadCompleted(new HtmlDocumentLoadCompleted(err));
        //    }
        //}

        #endregion

        #region Event Declarations
        /// <summary>
        /// Fired when a web request has finished
        /// </summary>
        public event EventHandler<HtmlDocumentLoadCompleted> LoadCompleted;

        #endregion

        #region Public Static Methods

        public static void LoadAsync(string path, CookieContainer container, EventHandler<HtmlDocumentLoadCompleted> callback)
        {
            var web = new HtmlWeb();
            web.LoadCompleted += callback;
            web.LoadAsync(path, container);
        }

        public static void LoadAsync(string path, CookieContainer container, string[] postVar, EventHandler<HtmlDocumentLoadCompleted> callback)
        {
            var web = new HtmlWeb();
            web.LoadCompleted += callback;
            web.LoadAsync(path, container, postVar);
        }

        #endregion
    }
}