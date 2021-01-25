using AngleSharp;
using CliFx;
using CliFx.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Z1FMDownloader.Core;

namespace Z1FMDownloader.CLI
{
    [Command]
    public class DownloadCommand : ICommand
    {
        [CommandParameter(0, Description = "Tracks to download", Name = "tracks")]
        public IEnumerable<string> Tracks { get; set; }
        [CommandOption('o', Description = "Path to output directory")]
        public string Output { get; set; }
        [CommandOption('p', Description = "Prefix for search queries")]
        public string Prefix { get; set; }

        private IBrowsingContext ctx = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private string baseUrl = "https://m.z1.fm/mp3/search?keywords=";


        private async Task DownloadTrack(string track)
        {
            using Downloader downloader = new Downloader();

            var keywords = Uri.EscapeUriString($"{Prefix} {track}");
            using var doc = await ctx.OpenAsync($"{baseUrl}{keywords}");
            var t = doc.QuerySelector("#list-songs > .tracks-item");
            var id = t?.GetAttribute("data-id");
            var title = t?.GetAttribute("data-title"); 
            if (id is null)
            {
                await Console.Out.WriteLineAsync($"Cant find track {track}");
                return;
            }
            var data = await downloader.Download(id);
            var size = (((double)data.Length) / 1024 / 1024).ToString("F2");
            await File.WriteAllBytesAsync(Path.Combine(Output, $"{title}.mp3"), data);
            await Console.Out.WriteLineAsync($"Downloaded {title} with size {size} MiB");
        }


        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (string.IsNullOrWhiteSpace(Output))
            {
                Output = Environment.CurrentDirectory;
            }
            Task.WaitAll(Tracks.Select(
                track => Task.Run
                (
                    () => DownloadTrack(track)
                )
            ).ToArray());
            console.Output.WriteLine("Completed");
        }
    }
}
