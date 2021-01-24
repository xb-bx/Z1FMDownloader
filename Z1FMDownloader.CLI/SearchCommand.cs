using AngleSharp;
using CliFx;
using CliFx.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Z1FMDownloader.Core;

namespace Z1FMDownloader.CLI
{
    [Command("search", Description = "Search tracks")]
    public class SearchCommand : ICommand
    {
        private static string baseUrl = "https://m.z1.fm/mp3/search?keywords=";

        [CommandParameter(0, Name = "track")]
        public string TrackName { get; set; }
        [CommandOption('o', Description = "Path to output directory")]
        public string Output { get; set; }


        private IBrowsingContext ctx = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (string.IsNullOrWhiteSpace(Output))
            {
                Output = Environment.CurrentDirectory;
            }
            string url = $"{baseUrl}{Uri.EscapeUriString(TrackName)}";
            var doc = await ctx.OpenAsync(url);
            var names = doc.QuerySelectorAll("#list-songs > .tracks-item > div > a > .tracks-name-title").Select(x=>x.TextContent);
            var artists = doc.QuerySelectorAll("#list-songs > .tracks-item > div > a > .tracks-name-artist").Select(x=>x.TextContent);
            var ids = doc.QuerySelectorAll("#list-songs > .tracks-item > .tracks-description > a:nth-child(3)").Select(x=>x.GetAttribute("href").Replace("/song/", null));
            console.Output.WriteLine($"{names.Count()} {artists.Count()} {ids.Count()}");
            
            var songs = names.Zip(artists).Zip(ids).Select(x => (x.First.First, x.First.Second, x.Second)).ToArray();

            for(int index = 0; index < songs.Length; index++)
            {
                console.Output.WriteLine($"#{index}: {songs[index].Item2} - {songs[index].First} | id: {songs[index].Item3}");
            }
            console.Output.WriteLine("Input song number: ");
            if(int.TryParse(console.Input.ReadLine(), out int i))
            {
                using Downloader downloader = new();
                var data = await downloader.Download(songs[i].Item3);
                await File.WriteAllBytesAsync(Path.Combine(Output, $"{songs[i].First}.mp3"), data);
            }
        }
    }
}
