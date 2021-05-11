using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flagship.Tests.Utils
{
    public class TestHttpHandler : HttpClientHandler
    {
        public string Content = "";
        public string Url = "";
        public HttpMethod Method;
        public bool ThrowError { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ThrowError)
            {
                throw new HttpRequestException();
            }
            Stream stream = new MemoryStream();
            await request.Content.CopyToAsync(stream).ConfigureAwait(false);
            stream.Position = 0;
            using (StreamReader sr = new StreamReader(stream))
            {
                Content = sr.ReadToEnd();
            }
            Url = request.RequestUri.AbsoluteUri;
            Method = request.Method;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
