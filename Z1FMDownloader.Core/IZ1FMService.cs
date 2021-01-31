using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Z1FMDownloader.Core
{
    public interface IZ1FMService : IDisposable
    {
        Task<byte[]> Download(Song song);
        IAsyncEnumerable<Song> Search(string query);
        Task<Song> GetSongById(string id);

    }



}
