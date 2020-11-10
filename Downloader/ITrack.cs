using System.Collections.Generic;

namespace AmazonDL.Downloader
{
    public interface ITrack
    {
        bool Encrypted { get; set; }
        string Id { get; set; }
        string[] Urls { get; set; }
        int Segments { get; set; }
        public string InitDataB64 { get; set; }
        public List<ContentKey> Keys { get; set; }
        public string Codec { get; set; }

        public enum SecurityLevel
        {
            Unknown,
            Software,
            Hardware
        }

        public string GetTrackType();
        public string GetFilename(string filename, bool decrypted = false, bool _fixed = false);
    }
}
