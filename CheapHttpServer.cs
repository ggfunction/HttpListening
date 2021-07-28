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

        private readonly EventWaitHandle keepOnListening;

        private readonly EventWaitHandle waitToQuit;

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

            this.Port = port >= 0 ? port : this.GetFreePort();

            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(string.Format("http://localhost:{0}/", this.Port));
            this.httpListener.Start();

            this.keepOnListening = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.waitToQuit = new EventWaitHandle(false, EventResetMode.AutoReset);

            this.DirectoryIndex = new string[]
            {
                "index.html", "index.htm", "default.html", "default.htm", "content/index.html",
            };

            this.PermittedMimeTypes = this.LoadMimeTypes();

            ThreadPool.QueueUserWorkItem(new WaitCallback(this.WaitCallback), null);
        }

        public string ContentRoot { get; private set; }

        public string[] DirectoryIndex { get; private set; }

        public bool IsListening
        {
            get { return this.keepOnListening.WaitOne(0); }
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
            this.keepOnListening.Set();
        }

        public void Stop()
        {
            this.keepOnListening.Reset();
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

            /* throw new NotImplementedException(); */
            return true;
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

            return false;
        }

        private void WaitCallback(object data)
        {
            while (!this.waitToQuit.WaitOne(0))
            {
                try
                {
                    var context = this.httpListener.GetContext();
                    var request = context.Request;
                    using (var response = context.Response)
                    {
                        if (!this.IsListening)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            continue;
                        }

                        string contentPath;

                        if (!this.TryResolvePath(request.RawUrl, out contentPath))
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            continue;
                        }

                        var mimeType = MimeTypes.GetMimeType(contentPath);

                        if (!this.HasPermissions(mimeType))
                        {
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            continue;
                        }

                        var bytes = System.IO.File.ReadAllBytes(contentPath);
                        response.ContentLength64 = bytes.Length;
                        response.OutputStream.Write(bytes, 0, bytes.Length);
                        response.StatusCode = (int)HttpStatusCode.OK;
                    }
                }
                catch (ObjectDisposedException)
                {
                }
                catch (HttpListenerException)
                {
                }
            }
        }
    }
}
