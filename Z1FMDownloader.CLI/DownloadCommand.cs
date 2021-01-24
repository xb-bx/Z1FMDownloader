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
        [CommandParameter(0, Description = "Tracks to download",Name = "tracks")]
        public IEnumerable<string> Tracks { get; set; }
        [CommandOption('o', Description = "Path to output directory")]
        public string Output { get; set; }

        private IBrowsingContext ctx = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private string baseUrl = "https://m.z1.fm/mp3/search?keywords=";

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (string.IsNullOrWhiteSpace(Output))
            {
                Output = Environment.CurrentDirectory;
            } 
            using Downloader downloader = new();
            Tracks.AsParallel()
                .ForAll(
                    async track =>
                    {
                        var keywords = Uri.EscapeUriString(track);
                        using var doc = await ctx.OpenAsync($"{baseUrl}{keywords}");
                        var id = doc.QuerySelector("#list-songs > .tracks-item > .tracks-description > a:nth-child(3)");
                        if (id is null)
                        {
                            console.Output.WriteLine($"Cant find track {track}");
                            return;
                        }
                        var data = await downloader.Download(id.GetAttribute("href").Substring(6));
                        var size = (((double)data.Length) / 1024 / 1024).ToString("F2");
                        await console.Output.WriteLineAsync($"Downloaded {track} with size {size} MiB");
                        await File.WriteAllBytesAsync(Path.Combine(Output,$"{track}.mp3"), data);
                    }
                );
            console.Input.Read();
        }
    }
}
