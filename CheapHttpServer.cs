namespace HttpListening
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    public class CheapHttpServer : IDisposable
    {
        private readonly HttpListener httpListener;

        private readonly EventWaitHandle waitToListen;

        private readonly EventWaitHandle waitToQuit;

        private int concurrentRequests;

        public CheapHttpServer()
            : this(Environment.CurrentDirectory, 8080)
        {
        }

        public CheapHttpServer(string contentRoot, int port)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("System.Net.HttpListener");
            }

            this.ContentRoot = System.IO.Directory.Exists(contentRoot) ?
                contentRoot : Environment.CurrentDirectory;

            this.Port = port > 0 ?
                port : this.GetFreePort();

            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(string.Format("http://localhost:{0}/", this.Port));
            this.httpListener.Start();

            this.waitToListen = new EventWaitHandle(true, EventResetMode.ManualReset);
            this.waitToQuit = new EventWaitHandle(false, EventResetMode.AutoReset);

            this.ConcurrentRequests = Environment.ProcessorCount;

            this.DirectoryIndex = new HashSet<string>
            {
                "index.html", "index.htm", "default.html", "default.htm", "/content/index.html",
            };

            this.PermittedMimeTypes = this.LoadMimeTypes();

            ThreadPool.QueueUserWorkItem(new WaitCallback(this.WaitCallback), null);
        }

        public string ContentRoot { get; private set; }

        public int ConcurrentRequests
        {
            get
            {
                return this.concurrentRequests;
            }

            set
            {
                this.concurrentRequests = Math.Min(Math.Max(1, value), Environment.ProcessorCount);
            }
        }

        public HashSet<string> DirectoryIndex { get; private set; }

        public bool IsListening
        {
            get { return !this.waitToListen.WaitOne(0); }
        }

        public int Port { get; private set; }

        public HashSet<string> PermittedMimeTypes { get; private set; }

        public void Close()
        {
            this.Stop();
            this.waitToQuit.Set();
            this.httpListener.Close();
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Start()
        {
            this.waitToListen.Reset();
        }

        public void Stop()
        {
            this.waitToListen.Set();
        }

        private WaitHandle BeginGetContext()
        {
            try
            {
                var ar = this.httpListener.BeginGetContext(new AsyncCallback(this.ListenerCallback), null);
                return ar.AsyncWaitHandle;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (HttpListenerException)
            {
            }

            return new EventWaitHandle(true, EventResetMode.AutoReset);
        }

        private void ListenerCallback(IAsyncResult ar)
        {
            try
            {
                var context = this.httpListener.EndGetContext(ar);
                var request = context.Request;
                var response = context.Response;

                try
                {
                    if (!this.IsListening)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return;
                    }

                    if (request.HttpMethod != "GET" && request.HttpMethod != "HEAD")
                    {
                        response.StatusCode = (int)HttpStatusCode.NotImplemented;
                        return;
                    }

                    string contentPath;

                    if (!this.TryResolvePath(request.RawUrl, out contentPath))
                    {
                        response.StatusCode = request.RawUrl.EndsWith("/") ?
                            (int)HttpStatusCode.Forbidden :
                            (int)HttpStatusCode.NotFound;
                        return;
                    }

                    var mimeType = MimeTypes.GetMimeType(contentPath);

                    if (!this.HasPermissions(mimeType))
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }

                    var bytes = System.IO.File.ReadAllBytes(contentPath);
                    response.ContentLength64 = bytes.Length;
                    response.OutputStream.Write(bytes, 0, bytes.Length);
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch (System.IO.IOException)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                finally
                {
                    response.Close();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (HttpListenerException)
            {
            }
        }

        private int GetFreePort()
        {
            var tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            try
            {
                tcpListener.Start();
                return ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        private bool HasPermissions(string mimeType)
        {
            if (mimeType == null)
            {
                throw new ArgumentNullException("mimeType");
            }

            return this.PermittedMimeTypes.Contains(mimeType);
        }

        private HashSet<string> LoadMimeTypes()
        {
            return new HashSet<string>
            {
                "text/plain",
                "text/html",
                "text/css",
                "text/javascript",
                "text/xml",
                "image/bmp",
                "image/png",
                "image/jpeg",
                "image/gif",
                "image/webp",
                "application/json",
                "application/xml",
            };
        }

        private bool TryResolvePath(string requestPath, out string contentPath)
        {
            requestPath = System.Web.HttpUtility.UrlDecode(requestPath);
            contentPath = string.Empty;

            var test = System.IO.Path.Combine(
                this.ContentRoot,
                requestPath.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar));
            var result = System.IO.File.Exists(test);

            if (result)
            {
                contentPath = test;
                return true;
            }

            if (requestPath.EndsWith("/") && System.IO.Directory.Exists(test))
            {
                test = this.DirectoryIndex
                    .Select(x => x.IndexOf('/') == 0 ?
                        System.IO.Path.Combine(this.ContentRoot, x.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar)) :
                        System.IO.Path.Combine(test, x.Replace('/', System.IO.Path.DirectorySeparatorChar)))
                    .FirstOrDefault(x => System.IO.File.Exists(x));
                contentPath = test;
                result = !string.IsNullOrEmpty(test);
            }

            return result;
        }

        private void WaitCallback(object data)
        {
            var waitHandles = new WaitHandle[]
            {
                this.waitToQuit,
                this.waitToListen,
            };

            var requests = new HashSet<WaitHandle>();

            for (var i = 0; i < this.ConcurrentRequests; i++)
            {
                requests.Add(this.BeginGetContext());
            }

            while (true)
            {
                var waitHandlesArray = waitHandles.Concat(requests).ToArray();
                var index = WaitHandle.WaitAny(waitHandlesArray);

                if (index == Array.IndexOf(waitHandlesArray, this.waitToQuit))
                {
                    break;
                }

                if (index == Array.IndexOf(waitHandlesArray, this.waitToListen))
                {
                    continue;
                }

                requests.Remove(waitHandlesArray[index]);

                for (var i = requests.Count; i < this.ConcurrentRequests; i++)
                {
                    requests.Add(this.BeginGetContext());
                }
            }
        }
    }
}
