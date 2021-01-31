using System;

namespace Z1FMDownloader.Core
{
    public class Song
    {
        public Song(string title, string artist, string iD, TimeSpan duration)
        {
            Title = title;
            Artist = artist;
            ID = iD;
            Duration = duration;
        }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string ID { get; set; }
        public TimeSpan Duration { get; set; }
    }



}
