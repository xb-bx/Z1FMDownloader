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
        public string? Output { get; set; }
        [CommandOption('p', Description = "Prefix for search queries")]
        public string? Prefix { get; set; }

        private IZ1FMService service = new Z1FMService();



        private async Task DownloadTrack(string track)
        {  
            var song = await service.Search($"{Prefix} {track}").FirstOrDefaultAsync();
            if(song is not null) 
            {
                var songData = await service.Download(song);
                var size = ((double)songData.Length) / 1024.0 / 1024.0;
                await File.WriteAllBytesAsync(Path.Combine(Output!,song.Title), songData);
                await Console.Out.WriteLineAsync($"Downloaded {song.Title} with size {size.ToString("F2")} MiB");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Failed to find {track}");
            }
        }


        public ValueTask ExecuteAsync(IConsole console)
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
            return default;
        }
    }
}
