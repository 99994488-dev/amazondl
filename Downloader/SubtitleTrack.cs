using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace AmazonDL.Downloader
{
    [Serializable]
    public class SubtitleTrack : ITrack
    {
        [JsonPropertyName("encrypted")]
        public bool Encrypted { get; set; } = false;
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonIgnore]
        public string InitDataB64 { get; set; } = null;
        [JsonIgnore]
        public List<ContentKey> Keys { get; set; } = null;
        [JsonIgnore]
        public string Codec { get; set; } = null;
        [JsonPropertyName("urls")]
        public string[] Urls { get; set; }
        [JsonPropertyName("segment_count")]
        public int Segments { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("language_code")]
        public string LanguageCode { get; set; }
        [JsonPropertyName("default")]
        public bool Default { get; set; }
        [JsonPropertyName("type")]
        string Type { get; set; }

        public SubtitleTrack(string id, string name, string languageCode, bool _default, string[] urls, int segments, string type)
        {
            Id = id;
            Name = name;
            LanguageCode = languageCode;
            Default = _default;
            Urls = urls;
            Segments = segments;
            Type = type;
        }

        public override string ToString()
        {
            return $"(id={Id}, name={Name}, languageCode={LanguageCode}, type={Type})";
        }

        public string GetFilename(string filename, string subtitleFormat)
        {
            string fn = string.Join("_", filename, LanguageCode, Id, subtitleFormat);

            return Path.Join(Constants.TEMP_FOLDER, fn);
        }

        public string GetTrackType()
        {
            return Type;
        }

        public string GetFilename(string filename, bool decrypted = false, bool _fixed = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
