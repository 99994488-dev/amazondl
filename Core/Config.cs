using AmazonDL.UtilLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmazonDL.Core
{
    public class Config
    {
        static string TOKEN_URL { get; } = "https://api.amazon.com/auth/token";


        static Dictionary<string, string> BASE_URLS = new Dictionary<string, string>()
        {
            ["pv"] = "atv-ps.primevideo.com",
            ["pv-eu"] = "atv-ps-eu.primevideo.com",
            ["pv-fe"] = "atv-ps-fe.primevideo.com",
            ["us"] = "atv-ps.amazon.com",
            ["uk"] = "atv-ps-eu.amazon.co.uk",
            ["de"] = "atv-ps-eu.amazon.de",
            ["jp"] = "atv-ps-fe.amazon.co.jp"
        };

        static Dictionary<string, string> MARKETPLACE_IDS = new Dictionary<string, string>()
        {
            ["pv"] = "ART4WZ8MWBX2Y",
            ["pv-eu"] = "ART4WZ8MWBX2Y",
            ["pv-fe"] = "A3K6Y4MI8GDYMT",
            ["us"] = "ATVPDKIKX0DER",
            ["uk"] = "A1F83G8C2ARO7P",
            ["de"] = "A1PA6795UKMFR9",
            ["jp"] = "A30VQA4BHCLSN"
        };

        static string[] REGIONS = new string[]
        {
            "pv", "pv-eu", "pv-fe", "us", "uk", "de", "jp"
        };

        static string ACCOUNT_ID { get; } = "A1SDJQ7W4W2U7U";
        static string ACCOUNT_TOKEN { get; } = "49e67d978b019128a2c8f9622ed639ae";
        static string CLIENT_ID { get; } = "f22dbddb-ef2c-48c5-8876-bed0d47594fd";
        static string DEVICE_ID { get; } = "22b438204aa9c6d90574b8b80b54411038f10dd0dabc10916bef0352";
        static string DEVICE_TYPE { get; } = "AOAGZA014O5RE";

        string Region { get; set; } = "us";

        public string ASIN { get; set; }
        public int Duration { get; set; }
        public bool Movie { get; set; } = false;
        public int SeasonEpisode { get; set; }
        public int SeasonNum { get; set; }
        public string SeriesTitle { get; set; }
        public string Title { get; set; }

        public string Codec { get; set; } = "H264";
        public string Bitrate { get; set; } = "CVBR%2CCBR&";
        public string EpNamePattern { get; set; } = "{series_title}.S{season}E{season_ep}.{title}.{resolution}p.AMZN.WEB-DL.{acodec}.{vcodec}-NOGRP";
        public string MovNamePattern { get; set; } = "{title}.year.{resolution}p.AMZN.WEB-DL.{acodec}.{vcodec}-NOGRP";
        public bool OnlyEng { get; set; } = false;
        public string Range { get; set; } = "none";
        public int Resolution { get; set; } = 0;

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>()
        {
            ["accept"] = "*/*",
            ["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36",
            ["connection"] = "keep-alive"
        };

        public Config(string asin, string region, int? resolution, string? hdr, string bitrate, string codec, bool? onlyEng, string? namePattern)
        {
            if (region != null)
                Region = region.ToLower();

            try
            {
                if (REGIONS.Contains(Region))
                    Region = region.ToLower();
                else
                    throw new Exception("Invalid region, available regions: " + string.Join(", ", REGIONS));
            }
            catch
            {
                throw new Exception("Invalid region, available regions: " + string.Join(", ", REGIONS));
            }

            GetCredentials();

            ASIN = asin;

            if (resolution != null)
                Resolution = (int)resolution;
            if (hdr == "hdr10")
                Range = "HDR10";
            else if (hdr == "dv")
                Range = "dolbyvision";
            if (bitrate != null && bitrate.ToLower() == "cbr")
                Bitrate = "CBR";
            if (codec != null && (codec.ToLower() == "h265" || codec.ToLower() == "hevc"))
                Codec = "H265";
            if (onlyEng != null)
                OnlyEng = (bool)onlyEng;

            if (Range != "none")
            {
                Codec = "H265";
            }

            if (namePattern != null)
            {
                EpNamePattern = namePattern;
                MovNamePattern = namePattern;
            }
        }

        public enum Resource
        {
            Metadata,
            Playback,
            Audio,
            License
        }

        public Dictionary<string, dynamic> GetContentRequest(Resource resource)
        {
            string desiredResource;
            if (resource == Resource.License)
                desiredResource = "Widevine2License";
            else if (resource == Resource.Playback || resource == Resource.Audio)
                desiredResource = "AudioVideoUrls%2CSubtitleUrls";
            else
                desiredResource = "CatalogMetadata";

            string gasc = Region.Contains("pv").ToString().ToLower();

            string url = $"https://{BASE_URLS[Region]}/cdp/catalog/GetPlaybackResources?deviceID={DEVICE_ID}&deviceTypeID={DEVICE_TYPE}&gascEnabled={gasc}&marketplaceID={MARKETPLACE_IDS[Region]}&firmware=1&playerType=xp&operatingSystemName=Windows&operatingSystemVersion=10.0&asin={ASIN}&consumptionType=Streaming&desiredResources={desiredResource}&resourceUsage=ImmediateConsumption&videoMaterialType=Feature&deviceProtocolOverride=Https&deviceStreamingTechnologyOverride=DASH&deviceDrmOverride=CENC&deviceBitrateAdaptationsOverride={Bitrate}&deviceVideoQualityOverride=HD&deviceHdrFormatsOverride={Range}&deviceVideoCodecOverride={Codec}&customerID={ACCOUNT_ID}&clientId={CLIENT_ID}&token={ACCOUNT_TOKEN}&userWatchSessionId={Guid.NewGuid().ToString("D")}&titleDecorationScheme=primary-content&playerAttributes=%7B%22middlewareName%22%3A%22Chrome%22%2C%22middlewareVersion%22%3A%2283.0.4103.116%22%2C%22nativeApplicationName%22%3A%22Chrome%22%2C%22nativeApplicationVersion%22%3A%2283.0.4103.116%22%2C%22supportedAudioCodecs%22%3A%22AAC%22%2C%22frameRate%22%3A%22HFR%22%2C%22H264.codecLevel%22%3A%225.2%22%2C%22H265.codecLevel%22%3A%225.2%22%7D";

            if (resource == Resource.License)
                url.Replace("CVBR%2CCBR&", "CVBR%2CCBR");

            return new Dictionary<string, dynamic>()
            {
                ["url"] = url,
                ["headers"] = Headers
            };
        }

        void GetCredentials()
        {
            string filename = Path.Join(Constants.COOKIES_FOLDER, Region + ".txt");

            if (!Directory.Exists(Constants.COOKIES_FOLDER))
                Directory.CreateDirectory(Constants.COOKIES_FOLDER);

            if (File.Exists(filename))
            {
                try
                {
                    string cookie = Requests.ParseNetscapeCookies(File.ReadAllText(filename));
                    Headers.Add("cookie", cookie);
                }
                catch
                {
                    throw new Exception("Could not retrieve credentials, fill " + filename + " with your cookies");
                }
            }
            else
            {
                File.Create(filename).Dispose();
                throw new Exception("Cookie file empty, fill " + filename + " with your cookies");
            }
        }

        public Dictionary<string, dynamic> GetInfoRequest(string asin)
        {
            string url = $"https://{BASE_URLS[Region]}/cdp/catalog/Browse?deviceID={DEVICE_ID}&deviceTypeID={DEVICE_TYPE}&format=json&marketplaceID={MARKETPLACE_IDS[Region]}&firmware=1&IncludeAll=T=&version=2&SeasonASIN={asin}&NumberOfResults=1000&StartIndex=0";

            return new Dictionary<string, dynamic>()
            {
                ["url"] = url,
                ["headers"] = Headers
            };
        }
    }
}
