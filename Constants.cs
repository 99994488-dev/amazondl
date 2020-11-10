using System.Diagnostics;
using System.IO;

namespace AmazonDL
{
    class Constants
    {
        //public readonly static string CONTACT = "devine001@protonmail.ch";

        public readonly static string CURRENT_DIRECTORY = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public readonly static string BINARIES_FOLDER = Path.Join(CURRENT_DIRECTORY, "data");
        public readonly static string COOKIES_FOLDER = Path.Join(CURRENT_DIRECTORY, "cookies");
        public readonly static string TEMP_FOLDER = Path.Join(CURRENT_DIRECTORY, "temp");
        public readonly static string OUTPUT_FOLDER = Path.Join(CURRENT_DIRECTORY, "output");

        public readonly static string ARIA_BINARY = "aria2c.exe";
        public readonly static string MP4DECRYPT_BINARY = "mp4decrypt.exe";
        public readonly static string MKVMERGE_BINARY = "mkvmerge.exe";
        public readonly static string FFMPEG_BINARY = "ffmpeg.exe";

        public readonly static string ARIA_BINARY_PATH = Path.Join(BINARIES_FOLDER, ARIA_BINARY);
        public readonly static string MP4DECRYPT_BINARY_PATH = Path.Join(BINARIES_FOLDER, MP4DECRYPT_BINARY);
        public readonly static string MKVMERGE_BINARY_PATH = Path.Join(BINARIES_FOLDER, MKVMERGE_BINARY);
        public readonly static string FFMPEG_BINARY_PATH = Path.Join(BINARIES_FOLDER, FFMPEG_BINARY);
    }
}
