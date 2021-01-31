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
        public string? TrackName { get; set; }
        [CommandOption('o', Description = "Path to output directory")]
        public string? Output { get; set; }

        private IZ1FMService service = new Z1FMService();        

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (string.IsNullOrWhiteSpace(Output))
            {
                Output = Environment.CurrentDirectory;
            }
            int skip = 0;
            var songs = service.Search(TrackName!);
            for(int i = 0;;)
            {
                await foreach(Song song in songs.Skip(skip).Take(10))
                {
                    console.Output.WriteLine($"#{i++}: {song.Title}");
                }
                console.Output.WriteLine("Input song number or press enter to load more:");
                var input = console.Input.ReadLine();
                if(string.IsNullOrWhiteSpace(input)) {
                    skip += 10;
                    continue;
                }
                else
                {
                    if(int.TryParse(input, out int index))
                    {
                        console.Output.WriteLine("Downloading...");
                        var song =await songs.ElementAtAsync(index);
                        var songData = await service.Download(song);
                        await File.WriteAllBytesAsync(Path.Combine(Output, $"{TrackName}.mp3"), songData);
                        var size = ((double)songData.Length) / 1024.0 / 1024.0;
                        console.Output.WriteLine($"Downloaded {TrackName}.mp3 with size {size.ToString("F2")} MiB");
                        break;
                    }
                    else
                    {
                        console.Output.WriteLine("Invalid number");
                        break;
                    }
                }

            }
        }
    }
}
