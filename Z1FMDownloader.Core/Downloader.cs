using System;
using RandomUserAgent;
using System.Threading.Tasks;
using System.Net.Http;

namespace Z1FMDownloader.Core
{


    public class Downloader : IDisposable
    {
        private HttpClient client = new();
        private bool disposed = false;
        private static readonly string downloadUrl = "https://m.z1.fm/download";
        public Downloader()
        {
            client.DefaultRequestHeaders.Add("User-Agent", RandomUa.RandomUserAgent);
            client.DefaultRequestHeaders.Add("cookie", "__cfduid=dbee0fdb0d09385bc2e4210f8a074f9801611478510; zvAuth=1; zvLang=0; _zvMobile_=0; PHPSESSID=kiiea1hbs9dlnjlma1fifot9q2; YII_CSRF_TOKEN=41f96c1d85f57539da298e6924ae382f4b3b1c5e; _zvBoobs_=%2F%2F_-%29");

        }
        ~Downloader ()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    client.Dispose();
                }
                disposed = true;
            }
        }
        public async Task<byte[]> Download(string id)
        {
            var resp = await client.GetAsync($"{downloadUrl}/{id}");
            return await resp.Content.ReadAsByteArrayAsync();
        }
    }


     
}
