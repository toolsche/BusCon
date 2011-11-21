using System;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Text;

namespace BusCon.Utility
{
    public class FileDownloader
    {
        private Action<string> _downloadCompleteCallback;
        private Action<byte[]> _downloadImageCompleteCallback;
        private bool _isDownloadImage;
        private AutoResetEvent wait;
        private bool _utfEncoding;

        public static string StripHtml(string sz)
        {
            return Regex.Replace(sz, "<(.|\\n)*?>", string.Empty).Replace('\n', ' ').Replace("&#8211;", " - ").Replace("&#8212;", " - ").Replace("&#8217;", "'").Replace("&#8220;", "\"").Replace("&#8221;", "\"").Trim();
        }

        public void Download(string fileUri, Action<string> callback, bool utfEncoding = true)
        {
            this.Download(new Uri(fileUri, UriKind.Absolute), callback, utfEncoding);
        }

        public void Download(Uri uri, Action<string> callback, bool utfEncoding = true)
        {
            this._utfEncoding = utfEncoding;
            this._downloadCompleteCallback = callback;
            new Thread(new ParameterizedThreadStart(this.DownloadThreadFunc)).Start((object)uri);
        }

        public void DownloadImage(string uriString, Action<byte[]> callback)
        {
            if (uriString == null)
                return;
            this.DownloadImage(new Uri(uriString, UriKind.Absolute), callback);
        }

        public void DownloadImage(Uri uri, Action<byte[]> callback)
        {
            this._downloadImageCompleteCallback = callback;
            this._isDownloadImage = true;
            new Thread(new ParameterizedThreadStart(this.DownloadThreadFunc)).Start((object)uri);
        }

        private void DownloadThreadFunc(object parameter)
        {
            try
            {
                this.wait = new AutoResetEvent(false);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(parameter as Uri);
                object state = new object();
                IAsyncResult response1 = httpWebRequest.BeginGetResponse(new AsyncCallback(this.RespCallback), state);
                this.wait.WaitOne();
                WebResponse response2 = httpWebRequest.EndGetResponse(response1);
                if (this._isDownloadImage)
                {
                    byte[] numArray = this.ReadImage(response2);
                    if (this._downloadImageCompleteCallback == null)
                        return;
                    this._downloadImageCompleteCallback(numArray);
                }
                else
                {
                    string str = this.Read(response2);
                    if (this._downloadCompleteCallback == null)
                        return;
                    this._downloadCompleteCallback(str);
                }
            }
            catch (WebException ex)
            {
                if (this._isDownloadImage && this._downloadImageCompleteCallback != null)
                {
                    this._downloadImageCompleteCallback((byte[])null);
                }
                else
                {
                    if (this._downloadCompleteCallback == null)
                        return;
                    this._downloadCompleteCallback((string)null);
                }
            }
            catch (Exception ex)
            {
                if (this._isDownloadImage && this._downloadImageCompleteCallback != null)
                {
                    this._downloadImageCompleteCallback((byte[])null);
                }
                else
                {
                    if (this._downloadCompleteCallback == null)
                        return;
                    this._downloadCompleteCallback((string)null);
                }
            }
        }

        private byte[] ReadImage(WebResponse resp)
        {
            byte[] numArray = (byte[])null;
            using (BinaryReader binaryReader = new BinaryReader(resp.GetResponseStream()))
            {
                numArray = binaryReader.ReadBytes((int)resp.ContentLength);
                binaryReader.Close();
            }
            return numArray;
        }

        private string Read(WebResponse resp)
        {
            StreamReader streamReader = !this._utfEncoding ? new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1")) : new StreamReader(resp.GetResponseStream());
            string str = streamReader.ReadToEnd();
            streamReader.Close();
            return str;
        }

        private void RespCallback(IAsyncResult asynchronousResult)
        {
            this.wait.Set();
        }
    }
}
