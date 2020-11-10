using AmazonDL.Downloader;
using System.Collections.Generic;
using System.IO;

namespace AmazonDL.Decrypt
{
    public class DecryptConfig
    {
        public string Filename { get; set; }
        public string Tracktype { get; set; }
        public string TrackId { get; set; }
        public bool License { get; set; }
        public string DeviceName { get; set; }
        public string InitDataB64 { get; set; }
        public string CertDataB64 { get; set; }
        public bool ServerCertRequired { get; set; }


        public DecryptConfig(string filename, string tracktype, string trackId, bool license, string deviceName, string initDataB64, string? certDataB64)
        {
            Filename = filename;
            Tracktype = tracktype;
            TrackId = trackId;
            License = license;
            DeviceName = deviceName;
            InitDataB64 = initDataB64;

            if (certDataB64 != null)
            {
                ServerCertRequired = true;
                CertDataB64 = certDataB64;
            }
            else
            {
                ServerCertRequired = false;
            }
        }

        public string GetFilename(bool decrypted)
        {
            return Path.Join(Constants.TEMP_FOLDER, string.Join("_", Filename, Tracktype, TrackId, decrypted ? "decrypted" : "encrypted")) + ".mp4";
        }

        public string[] BuildCommandLine(List<ContentKey> keys)
        {
            string[] commandline = new string[2];
            commandline[0] = Constants.MP4DECRYPT_BINARY_PATH;
            commandline[1] = "";

            foreach (var key in keys)
            {
                if (key.Type == "Content")
                {
                    commandline[1] += $"--key {key} ";
                }
            }
            commandline[1] += $"\"{GetFilename(false)}\" ";
            commandline[1] += $"\"{GetFilename(true)}\" ";

            return commandline;
        }
    }
}
