using AmazonDL.Core;
using System.Collections.Generic;

namespace AmazonDL.Downloader
{
    class DownloaderConfig
    {
        public Client Client { get; }
        public string Filename { get; }
        public string SubtitleFormat { get; }
        public bool PrintInfo { get; }
        public bool SkipCleanup { get; }
        public bool DontMux { get; }
        public bool License { get; set; }
        public bool Overwrite { get; set; }
        public string Tag { get; }
        public bool AudioOnly { get; set; }
        public bool VideoOnly { get; set; }
        public bool SubsOnly { get; set; }
        public bool EncryptedOnly { get; set; }
        public List<string> VideoIds { get; set; }
        public List<string> AudioIds { get; set; }

        public DownloaderConfig(Client client, string filename, string subtitleFormat, bool printInfo, bool skipCleanup, bool dontMux, bool license, bool overwrite, string tag, bool audioonly, bool videoonly, bool subsonly, bool encryptedonly, bool customName, List<string> videoIds, List<string> audioIds)
        {
            if (!printInfo && filename.Contains(".") && !System.Text.RegularExpressions.Regex.IsMatch(filename.Substring(0, filename.IndexOf(".")), "^[a-zA-Z0-9]*$"))
            {
                throw new System.Exception("Filename contains non-ASCII characters, use a custom filename using -o <name>");
            }

            Client = client;
            Filename = filename;
            SubtitleFormat = subtitleFormat;
            PrintInfo = printInfo;
            SkipCleanup = skipCleanup;
            DontMux = dontMux;
            License = license;
            Overwrite = overwrite;
            Tag = tag;
            AudioOnly = audioonly;
            VideoOnly = videoonly;
            SubsOnly = subsonly;
            EncryptedOnly = encryptedonly;
            VideoIds = videoIds;
            AudioIds = audioIds;

            if (audioonly || subsonly)
            {
                DontMux = true;
            }
        }
    }
}
