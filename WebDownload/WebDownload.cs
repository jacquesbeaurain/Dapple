using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;
using Utility;

namespace WorldWind.Net
{
    public delegate void DownloadProgressHandler(int bytesRead, int totalBytes);
    public delegate void DownloadCompleteHandler(WebDownload downloadInfo);
    public delegate void DownloadDebugHandler(WebDownload wd);

    public enum DownloadType
    {
        Unspecified,
        Wms
    }

    public class WebDownload : IDisposable
    {
       public enum HttpProtoVersion
       {
          HTTP1_1,
          HTTP1_0
       }

        #region Static proxy properties

        static public HttpProtoVersion useProto;
        static public bool useWindowsDefaultProxy = true;
        static public string proxyUrl = "";
        static public bool useDynamicProxy;
        static public string proxyUserName = "";
        static public string proxyPassword = "";

        #endregion

        public static string UserAgent = String.Format(
           CultureInfo.InvariantCulture,
           "World Wind v{0} ({1}, {2})",
           System.Windows.Forms.Application.ProductVersion,
           Environment.OSVersion.ToString(),
           System.Globalization.CultureInfo.CurrentCulture.Name);

        public string Url;

        /// <summary>
        /// Memory downloads fills this stream
        /// </summary>
        public Stream ContentStream;

        public string SavedFilePath;
        public bool IsComplete;

        /// <summary>
        /// Called when data is being received.  
        /// Note that totalBytes will be zero if the server does not respond with content-length.
        /// </summary>
        public DownloadProgressHandler ProgressCallback;

        /// <summary>
        /// Called to update debug window.
        /// </summary>
        public static DownloadDebugHandler DebugCallback;

        /// <summary>
        /// Called when download is completed.  Call Verify from event handler to throw any exception.
        /// </summary>
        public DownloadCompleteHandler CompleteCallback;

        public DownloadType DownloadType = DownloadType.Unspecified;
        public string ContentType;
        public int BytesProcessed;
        public int ContentLength;

        /// <summary>
        /// The download start time (or MinValue if not yet started)
        /// </summary>
        public DateTime DownloadStartTime = DateTime.MinValue;

        internal HttpWebRequest request;
        internal HttpWebResponse response;

        protected Exception downloadException;

        protected bool isMemoryDownload;
        private bool stopFlag = false;
        protected Thread dlThread;

        /// <summary>
        /// Constructor
        /// </summary>
        public WebDownload(string url)
        {
            this.Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref= "T:WorldWind.Net.WebDownload"/> class.
        /// </summary>
        public WebDownload()
        {
        }

#if DEBUG

        ~WebDownload()
        {
            // Somebody forgot to dispose me...
            // We've done what we can for now (mashi)
            // Debug.Assert(request==null);
        }

#endif

        /// <summary>
        /// Whether the download is currently being processed (active).
        /// </summary>
        public bool IsDownloadInProgress
        {
            get
            {
				return dlThread != null && dlThread.IsAlive;
            }
        }

        /// <summary>
        /// Contains the exception that occurred during download, or null if successful.
        /// </summary>
        public Exception Exception
        {
            get
            {
                return downloadException;
            }
        }

        /// <summary>
        /// Asynchronous download of HTTP data to file. 
        /// </summary>
        public void BackgroundDownloadFile()
        {
            if (CompleteCallback == null)
                throw new ArgumentException("No download complete callback specified.");

            ThreadPool.QueueUserWorkItem(new WaitCallback(Download));
            
            //dlThread = new Thread(new ThreadStart(Download));
            //dlThread.Name = "WebDownload.dlThread";
            //dlThread.IsBackground = true;
            //dlThread.Start();
        }

        public void Download(object statusInfo)
        {
            Download();
        }

        /// <summary>
        /// Asynchronous download of HTTP data to file.
        /// </summary>
        public void BackgroundDownloadFile(DownloadCompleteHandler completeCallback)
        {
            CompleteCallback += completeCallback;
            BackgroundDownloadFile();
        }

        /// <summary>
        /// Download image of specified type. (handles server errors for wms type)
        /// </summary>
        public void BackgroundDownloadFile(DownloadType dlType)
        {
            DownloadType = dlType;
            BackgroundDownloadFile();
        }

        /// <summary>
        /// Asynchronous download of HTTP data to in-memory buffer. 
        /// </summary>
        public void BackgroundDownloadMemory()
        {
            if (CompleteCallback == null)
                throw new ArgumentException("No download complete callback specified.");

            isMemoryDownload = true;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Download));

            //dlThread = new Thread(new ThreadStart(Download));
            //dlThread.Name = "WebDownload.dlThread(2)";
            //dlThread.IsBackground = true;
            //dlThread.Start();
        }

        /// <summary>
        /// Asynchronous download of HTTP data to in-memory buffer. 
        /// </summary>
        public void BackgroundDownloadMemory(DownloadCompleteHandler completeCallback)
        {
            CompleteCallback += completeCallback;
            BackgroundDownloadMemory();
        }

        /// <summary>
        /// Download image of specified type. (handles server errors for WMS type)
        /// </summary>
        public void BackgroundDownloadMemory(DownloadType dlType)
        {
            DownloadType = dlType;
            BackgroundDownloadMemory();
        }

        /// <summary>
        /// Synchronous download of HTTP data to in-memory buffer. 
        /// </summary>
        public void DownloadMemory()
        {
            isMemoryDownload = true;
            Download();
        }

        /// <summary>
        /// Download image of specified type. (handles server errors for WMS type)
        /// </summary>
        public void DownloadMemory(DownloadType dlType)
        {
            DownloadType = dlType;
            DownloadMemory();
        }

        public void DownloadMemory(DownloadProgressHandler progressCallback)
        {
            ProgressCallback += progressCallback;
            DownloadMemory();
        }

        /// <summary>
        /// Synchronous download of HTTP data to in-memory buffer. 
        /// </summary>
        public void DownloadFile(string destinationFile)
        {
            SavedFilePath = destinationFile;

            Download();
        }

        /// <summary>
        /// Download image of specified type to a file. (handles server errors for WMS type)
        /// </summary>
        public void DownloadFile(string destinationFile, DownloadType dlType)
        {
            DownloadType = dlType;
            DownloadFile(destinationFile);
        }

        /// <summary>
        /// Saves a http in-memory download to file.
        /// </summary>
        /// <param name="destinationFilePath">File to save the downloaded data to.</param>
        public void SaveMemoryDownloadToFile(string destinationFilePath)
        {
            if (ContentStream == null)
                throw new InvalidOperationException("No data available.");

            // Cache the capabilities on file system
            ContentStream.Seek(0, SeekOrigin.Begin);
            using (Stream fileStream = File.Create(destinationFilePath))
            {
                if (ContentStream is MemoryStream)
                {
                    // Write the MemoryStream buffer directly (2GB limit)
                    MemoryStream ms = (MemoryStream)ContentStream;
                    fileStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                }
                else
                {
                    // Block copy
                    byte[] buffer = new byte[4096];
                    while (true)
                    {
                        int numRead = ContentStream.Read(buffer, 0, buffer.Length);
                        if (numRead <= 0)
                            break;
                        fileStream.Write(buffer, 0, numRead);
                    }
                }
            }
            ContentStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Aborts the current download. 
        /// </summary>
        public void Cancel()
        {
            CompleteCallback = null;
            ProgressCallback = null;
            if (dlThread != null && dlThread != Thread.CurrentThread)
            {
                if (dlThread.IsAlive)
                    stopFlag = true;
                    if (!dlThread.Join(500))
                    dlThread.Abort();
                dlThread = null;
            }
        }

        /// <summary>
        /// Notify event subscribers of download progress.
        /// </summary>
        /// <param name="bytesRead">Number of bytes read.</param>
        /// <param name="totalBytes">Total number of bytes for request or 0 if unknown.</param>
        protected void OnProgressCallback(int bytesRead, int totalBytes)
        {
            if (ProgressCallback != null)
            {
                ProgressCallback(bytesRead, totalBytes);
            }
        }

        protected static void OnDebugCallback(WebDownload wd)
        {
            if (DebugCallback != null)
            {
                DebugCallback(wd);
            }
        }

        /// <summary>
        /// Synchronous HTTP download
        /// </summary>
        protected virtual void Download()
        {
            Debug.Assert(Url.StartsWith("http://"));
            DownloadStartTime = DateTime.Now;
            try
            {
                try
                {
                    // If a registered progress-callback, inform it of our download progress so far.
                    OnProgressCallback(0, -1);
                    OnDebugCallback(this);

                    // check to see if thread was aborted (multiple such checks within the thread function)
                    if (stopFlag)
                    {
                        IsComplete = true;
                        return;
                    }

					// create content stream from memory or file
                    if (isMemoryDownload && ContentStream == null)
                    {
                        ContentStream = new MemoryStream();
                    }
                    else
                    {
                        // Download to file
                        string targetDirectory = Path.GetDirectoryName(SavedFilePath);
                        if (targetDirectory.Length > 0)
                            Directory.CreateDirectory(targetDirectory);
                        ContentStream = new FileStream(SavedFilePath, FileMode.Create);
                    }

                    // Create the request object.
                    request = (HttpWebRequest)WebRequest.Create(Url);
                    request.UserAgent = UserAgent;

                    if (stopFlag)
                    {
                        IsComplete = true;
                        return;
                    }

                    request.Proxy = ProxyHelper.DetermineProxyForUrl(
                       Url,
                       useWindowsDefaultProxy,
                       useDynamicProxy,
                       proxyUrl,
                       proxyUserName,
                       proxyPassword);

                    request.ProtocolVersion = HttpVersion.Version11;
                    
                    // TODO: probably better done via BeginGetResponse() / EndGetResponse() because this may block for a while
                    // causing warnings in thread abortion.
                    using (response = request.GetResponse() as HttpWebResponse)
                    {
                        // only if server responds 200 OK
                       if (response.StatusCode == HttpStatusCode.OK)
                       {
                          ContentType = response.ContentType;

                          // Find the data size from the headers.
                          string strContentLength = response.Headers["Content-Length"];
                          if (strContentLength != null)
                          {
                             ContentLength = int.Parse(strContentLength, CultureInfo.InvariantCulture);
                          }

                          byte[] readBuffer = new byte[1500];
                          using (Stream responseStream = response.GetResponseStream())
                          {
                             while (true)
                             {
                                //  Pass do.readBuffer to BeginRead.
                                    if (stopFlag)
                                    {
                                        IsComplete = true;
                                        return;
                                    }

									//  Pass do.readBuffer to BeginRead.
                                int bytesRead = responseStream.Read(readBuffer, 0, readBuffer.Length);
                                if (bytesRead <= 0)
                                   break;

                                ContentStream.Write(readBuffer, 0, bytesRead);
                                BytesProcessed += bytesRead;

                                // If a registered progress-callback, inform it of our download progress so far.
                                OnProgressCallback(BytesProcessed, ContentLength);
                                OnDebugCallback(this);

                                // Give up our timeslice, to allow other thread (i.e. GUI progress to respond)
                                Thread.Sleep(0);
                             }
                          }
                       }
                       else
                       {
                          throw new ApplicationException("BAD REQUEST");
                       }
                    }

                    HandleErrors();
                }
                catch (System.Configuration.ConfigurationException)
                {
                    // is thrown by WebRequest.Create if App.config is not in the correct format
                    // TODO: don't know what to do with it
                    throw;
                }
                catch (Exception caught)
                {
                    try
                    {
                        // Remove broken file download
                        if (ContentStream != null)
                        {
                            ContentStream.Close();
                            ContentStream = null;
                        }
                        if (SavedFilePath != null && SavedFilePath.Length > 0)
                        {
                            File.Delete(SavedFilePath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    SaveException(caught);
                }

                if (stopFlag)
                {
                    IsComplete = true;
                    return;
                }

                if (ContentLength == 0)
                {
                    ContentLength = BytesProcessed;
                    // If a registered progress-callback, inform it of our completion
                    OnProgressCallback(BytesProcessed, ContentLength);
                }

                if (ContentStream is MemoryStream)
                {
                    ContentStream.Seek(0, SeekOrigin.Begin);
                }
                else if (ContentStream != null)
                {
                    ContentStream.Close();
                    ContentStream = null;
                }

                OnDebugCallback(this);

                if (CompleteCallback == null)
                {
                   //Verify();
#if DEBUG
                   // Used to be that it verified here, this crashes if canceled from downloads timing out (See GeoSpatialDownloadRequest.Cancel)
                   // Just notify this in debug mode for now, this may need further thought
                   // This class should only be used with CompleteCallback's anyway (there are exceptions thrown)
                   // so should be safe.
                   if (Exception != null)
                   {
                      if (Exception.InnerException != null)
                         System.Diagnostics.Debug.WriteLine("Exception in download: " + Url + "\n\t" + Exception.InnerException.Message);
                      else
                         System.Diagnostics.Debug.WriteLine("Exception in download: " + Url + "\n\t" + Exception.Message);
                   }
#endif
                }
                else
                {
                    CompleteCallback(this);
                }
            }
            catch (ThreadAbortException)
            { }
            finally
            {
                IsComplete = true;
            }
        }

        /// <summary>
        /// Handle server errors that don't get trapped by the web request itself.
        /// </summary>
        private void HandleErrors()
        {
            // HACK: Workaround for TerraServer failing to return 404 on not found
            if (ContentStream.Length == 15)
            {
                // a true 404 error is a System.Net.WebException, so use the same text here
                Exception ex = new FileNotFoundException("The remote server returned an error: (404) Not Found.", SavedFilePath);
                SaveException(ex);
            }

            // TODO: WMS 1.1 content-type != xml
            // TODO: Move WMS logic to WmsDownload
            if (DownloadType == DownloadType.Wms && (
               ContentType.StartsWith("text/xml") ||
               ContentType.StartsWith("application/vnd.ogc.se")))
            {
                // WMS request failure
                SetMapServerError();
            }
        }

        /// <summary>
        /// If exceptions occurred they will be thrown by calling this function.
        /// </summary>
        public void Verify()
        {
            if (Exception != null)
                throw Exception;
        }

        /// <summary>
        /// Log download error to log file
        /// </summary>
        /// <param name="exception"></param>
        private void SaveException(Exception exception)
        {
            // Save the exception 
            downloadException = exception;

            if (Exception is ThreadAbortException)
                // Don't log canceled downloads
                return;

            Log.Write("HTTP", "Error: " + Url);
            Log.Write("HTTP", "     : " + exception.Message);
        }

        /// <summary>
        /// Reads the xml response from the server and throws an error with the message.
        /// </summary>
        private void SetMapServerError()
        {
            try
            {
                XmlDocument errorDoc = new XmlDocument();
                ContentStream.Seek(0, SeekOrigin.Begin);
                errorDoc.Load(ContentStream);
                string msg = "";
				foreach( XmlNode node in errorDoc.GetElementsByTagName("ServiceException"))
                    msg += node.InnerText.Trim() + Environment.NewLine;
                SaveException(new WebException(msg.Trim()));
            }
            catch (XmlException)
            {
                SaveException(new WebException("An error occurred while trying to download " + request.RequestUri.ToString() + "."));
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (dlThread != null && dlThread != Thread.CurrentThread)
            {
                if (dlThread.IsAlive)
                    stopFlag = true;
                    if (!dlThread.Join(500))
                    dlThread.Abort();
                dlThread = null;
            }

            if (request != null)
            {
                request.Abort();
                request = null;
            }

            if (ContentStream != null)
            {
                ContentStream.Close();
                ContentStream = null;
            }

            if (DownloadStartTime != DateTime.MinValue)
                OnDebugCallback(this);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
