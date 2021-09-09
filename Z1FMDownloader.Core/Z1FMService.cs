using System;
using RandomUserAgent;
using AngleSharp;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace Z1FMDownloader.Core
{
    public class Z1FMService : IZ1FMService
    {
        private HttpClient client = new();
        private bool disposed = false;

        public Z1FMService()
        {
            client.DefaultRequestHeaders.Add("User-Agent", RandomUa.RandomUserAgent);
            client.DefaultRequestHeaders.Add("cookie", "__cfduid=dbee0fdb0d09385bc2e4210f8a074f9801611478510; zvAuth=1; zvLang=0; _zvMobile_=0; PHPSESSID=kiiea1hbs9dlnjlma1fifot9q2; YII_CSRF_TOKEN=41f96c1d85f57539da298e6924ae382f4b3b1c5e; _zvBoobs_=%2F%2F_-%29");
            client.BaseAddress = new("https://z2.fm");
        }


        public async Task<byte[]> Download(Song song)
        {
            return await client.GetByteArrayAsync($"/download/{song.ID}");
        }

        public async Task<Song> GetSongById(string id)
        {
            var content = await client.GetStringAsync($"/download/{id}");
            var doc = await context.OpenAsync(req => req.Content(content));
            var titlebox = doc.QuerySelector(".title-box");
            var title = titlebox.GetElementsByTagName("h2").First().TextContent;
            var artist = titlebox.GetElementsByTagName("h1").First().TextContent;
            var duration = TimeSpan.Parse(doc.QuerySelector(".sb_item:nth-child(4) > b").TextContent);
            return new Song(title, artist, id, duration);
        }
        private IBrowsingContext context = BrowsingContext.New(Configuration.Default);

        private Song? ParseSong(IElement song)
        {
            string id = song.GetAttribute("data-play");
            var duration = song.GetElementsByClassName("song-time").FirstOrDefault();
            if (duration is null)
            {
                return null;
            }
            if (!TimeSpan.TryParse(duration.TextContent, out TimeSpan dur))
                return null;
            var artist = song.GetElementsByClassName("song-artist").FirstOrDefault()?.GetElementsByTagName("span")?.FirstOrDefault()?.TextContent;
            if (artist is null)
                return null;
            var title = song.GetElementsByClassName("song-name").FirstOrDefault()?.GetElementsByTagName("span")?.FirstOrDefault()?.TextContent;
            if (title is null)
                return null;
            return new Song(title, artist, id, dur);
        }


        public async IAsyncEnumerable<Song> Search(string query)
        {
            var prevpage = "";
            for (int page = 1; page.ToString() != prevpage; page++)
            {
                var content = await client.GetStringAsync($"/mp3/search?keywords={Uri.EscapeUriString(query)}&page={page}");
                var doc = await context.OpenAsync(req => req.Content(content));
                prevpage = doc.QuerySelector("#yw1 > b").TextContent;
                var songInfos = doc.QuerySelectorAll(".whb_box > .songs-list > .songs-list-item > .songs-list-item > .song-wrap > .song");
                foreach (var item in songInfos)
                {
                    var song = ParseSong(item);
                    if(song == null) continue;
                    yield return song;
                }
            }
        }

        ~Z1FMService()
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
    }



}
