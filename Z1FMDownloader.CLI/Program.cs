using CliFx;
using System; 
using System.Threading.Tasks;

namespace Z1FMDownloader.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
        }
    }
}
