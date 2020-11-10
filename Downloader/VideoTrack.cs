using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace AmazonDL.Downloader
{
    public enum HdrFormat
    {
        None,
        DV,
        HDR10
    }

    [Serializable]
    public class VideoTrack : ITrack
    {
        [JsonPropertyName("encrypted")]
        public bool Encrypted { get; set; } = false;
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("init_data")]
        public string InitDataB64 { get; set; }
        [JsonPropertyName("keys")]
        public List<ContentKey> Keys { get; set; }
        [JsonPropertyName("urls")]
        public string[] Urls { get; set; }
        [JsonPropertyName("segment_count")]
        public int Segments { get; set; }
        [JsonPropertyName("codec")]
        public string Codec { get; set; }
        [JsonPropertyName("bitrate")]
        public int Bitrate { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("frame_rate")]
        public double FrameRate { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; } = 0;
        [JsonPropertyName("vmaf")]
        public int Vmaf { get; set; } = 0;
        [JsonPropertyName("hdr")]
        public HdrFormat Hdr { get; set; } = HdrFormat.None;
        [JsonIgnore]
        public ITrack.SecurityLevel SecurityLevel { get; set; } = ITrack.SecurityLevel.Unknown;

        public VideoTrack()
        {

        }

        public VideoTrack(bool encrypted, string id, string initDataB64, string[] urls, int segments, string codec, int bitrate, int width, int height, double frameRate, ITrack.SecurityLevel securityLevel)
        {
            Encrypted = encrypted;
            Id = id;
            InitDataB64 = initDataB64;
            Urls = urls;
            Segments = segments;
            Codec = codec;
            Bitrate = bitrate;
            Width = width;
            Height = height;
            FrameRate = frameRate;
            SecurityLevel = securityLevel;
        }

        public string GetTrackType()
        {
            return "video";
        }

        public override string ToString()
        {
            return $"(encrypted={Encrypted}, id={Id}, codec={Codec}, bitrate={Bitrate}, width={Width}, height={Height}, frameRate={FrameRate}, hdr={Hdr})";
        }

        public string GetFilename(string filename, bool decrypted = false, bool _fixed = false)
        {
            string fn = string.Join("_", filename, "video", Id);

            if (!Encrypted || decrypted)
                fn += "_decrypted";
            else
                fn += "_encrypted";

            if (_fixed)
                fn += "_fixed";

            return Path.Join(Constants.TEMP_FOLDER, fn + ".mp4");
        }
    }
}
