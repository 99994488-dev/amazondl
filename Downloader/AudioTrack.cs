using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace AmazonDL.Downloader
{
    [Serializable]
    public class AudioTrack : ITrack
    {
        [JsonPropertyName("encrypted")]
        public bool Encrypted { get; set; } = false;
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
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
        [JsonPropertyName("channels")]
        public int Channels { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        [JsonIgnore]
        public ITrack.SecurityLevel SecurityLevel { get; set; } = ITrack.SecurityLevel.Unknown;

        public AudioTrack()
        {

        }

        public AudioTrack(bool encrypted, string id, string name, string initDataB64, string[] urls, int segments, string codec, int bitrate, int channels, string language, ITrack.SecurityLevel securityLevel)
        {
            Encrypted = encrypted;
            Name = name;
            Id = id;
            InitDataB64 = initDataB64;
            Urls = urls;
            Segments = segments;
            Codec = codec;
            Bitrate = bitrate;
            Channels = channels;
            Language = language;
            SecurityLevel = securityLevel;
        }

        public string GetTrackType()
        {
            return "audio";
        }

        public override string ToString()
        {
            return $"(encrypted={Encrypted}, id={Id}, codec={Codec}, bitrate={Bitrate}, language={Language})";
        }

        public string GetFilename(string filename, bool decrypted = false, bool _fixed = false)
        {
            string fn = string.Join("_", filename, "audio", Id);

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