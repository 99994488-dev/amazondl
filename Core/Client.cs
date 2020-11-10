using AmazonDL.Downloader;
using AmazonDL.UtilLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AmazonDL.Core
{
    public class Client
    {
        public bool PreferAndroidL3 { get; set; } = false;

        Config Config { get; set; }
        bool IsLoggedIn { get; set; }
        JObject MetadataJson { get; set; }

        public Client(Config config)
        {
            Logger.Debug("Creating Amazon client");
            Config = config;
            IsLoggedIn = CheckCookie();
            if (!GetInfo())
                throw new Exception($"Could not find corresponding media for {Config.ASIN}");
            Logger.Info($"{GetFilename()}");
        }

        public bool Login()
        {
            return IsLoggedIn;
        }

        bool CheckCookie()
        {
            Logger.Verbose("Checking cookie");

            MetadataJson = Requests.RequestJson(Config.GetContentRequest(Config.Resource.Metadata));

            if (MetadataJson.ContainsKey("catalogMetadata"))
                return true;
            else
            {
                throw new Exception("Invalid asin/cookie/region");
            }
        }

        bool GetInfo()
        {
            Logger.Debug("Catalog response: " + MetadataJson.ToString());
            try
            {
                if (MetadataJson["catalogMetadata"]["catalog"]["entityType"].ToString() == "Movie")
                    Config.Movie = true;

                string title = MetadataJson["catalogMetadata"]["catalog"]["title"].ToString();
                Config.Title = Regex.Replace(title, @"(^\w)|(\s\w)", m => m.Value.ToUpper());

                Config.Duration = int.Parse(MetadataJson["catalogMetadata"]["catalog"]["runtimeSeconds"].ToString());

                if (!Config.Movie)
                {
                    Config.SeasonEpisode = int.Parse(MetadataJson["catalogMetadata"]["catalog"]["episodeNumber"].ToString());
                    Config.SeasonNum = int.Parse(MetadataJson["catalogMetadata"]["family"]["tvAncestors"].Where(x => x["catalog"]["type"].ToString() == "SEASON").FirstOrDefault()["catalog"]["seasonNumber"].ToString());
                    string seriesTitle = MetadataJson["catalogMetadata"]["family"]["tvAncestors"].Where(x => x["catalog"]["type"].ToString() == "SHOW").FirstOrDefault()["catalog"]["title"].ToString().ToLower();
                    Config.SeriesTitle = Regex.Replace(seriesTitle, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                }

                return true;
            }
            catch
            {
                Logger.Error("Could not parse info json");
                return false;
            }
        }

        public string[] GetSeason()
        {
            JObject episodeJson = Requests.RequestJson(Config.GetInfoRequest(Config.ASIN));

            string seasonId;

            try
            {
                seasonId = episodeJson["message"]["body"]["titles"].Where(x => ((JArray)x["ancestorTitles"].ToObject(typeof(JArray))).Count > 0).FirstOrDefault()["ancestorTitles"].Where(x => x["contentType"].ToString() == "SEASON").FirstOrDefault()["titleId"].ToString();
            }
            catch
            {
                Logger.Error("Could not find season asin");
                return null;
            }

            JObject seasonJson = Requests.RequestJson(Config.GetInfoRequest(seasonId));

            List<string> episodes = new List<string>();

            foreach (var episode in seasonJson["message"]["body"]["titles"])
            {
                try
                {
                    string id = episode["titleId"].ToString();
                    episodes.Add(id);

                    Logger.Info($"{id} - " +
                    $"{episode["number"]} - " +
                    $"{episode["title"]}");
                }
                catch
                {
                    Logger.Info($"{episode["titleId"]}");
                }
            }

            return episodes.ToArray();
        }

        public string GetFilename()
        {
            string ret;
            if (Config.Movie)
                ret = Config.MovNamePattern
                    .Replace("{title}", Config.Title.Trim().Replace(" ", "."));
            else
                ret = Config.EpNamePattern
                    .Replace("{ep}", Config.SeasonEpisode.ToString().PadLeft(2, '0'))
                    .Replace("{season_ep}", Config.SeasonEpisode.ToString().ToString().PadLeft(2, '0'))
                    .Replace("{season}", Config.SeasonNum.ToString().ToString().PadLeft(2, '0'))
                    .Replace("{title}", Config.Title.Trim().Replace(" ", "."))
                    .Replace("{series_title}", Config.SeriesTitle.Trim().Replace(" ", "."));

            return ret.Replace(":", "").Replace("?", "").Replace("%", "").Replace("!", "").Replace("|", ".").Replace("\\", ".").Replace("/", ".").Replace(",", "").Replace("’", "").Replace("\'", "").Replace("\"", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace("...", ".").Replace("..", ".").Replace(".-.", ".");

        }

        public DASHPlaylist GetPlaylist(List<string> videoIDs, List<string> audioIDs)
        {
            Logger.Info("Getting media");

            JObject avJson = Requests.RequestJson(Config.GetContentRequest(Config.Resource.Playback));

            Logger.Debug(avJson.ToString());

            if (!avJson.ContainsKey("audioVideoUrls"))
            {
                Logger.Error("No audio/video URLs found");

                if (avJson.ContainsKey("errorsByResource"))
                    Logger.Error(avJson["errorsByResource"].First.First["errorCode"].ToString());

                return null;
            }

            string mpdUrl = avJson["audioVideoUrls"]["avCdnUrlSets"].OrderBy(x => int.Parse(x["cdnWeightsRank"].ToString())).First()["avUrlInfoList"].First()["url"].ToString();

            Match match = Regex.Match(mpdUrl, @"(https?:\/\/.*\/)d.{0,1}\/.*~\/(.*)");
            mpdUrl = match.Groups[1].Value + match.Groups[2].Value;

            if (mpdUrl == null)
            {
                Logger.Error("Could not find a valid manifest");
                return null;
            }

            var audioMetadata = new List<AudioTrackMetadata>();

            foreach (JObject audioTrackMetadata in avJson["audioVideoUrls"]["audioTrackMetadata"])
            {
                if (audioTrackMetadata.ContainsKey("audioTrackId"))
                    audioMetadata.Add(new AudioTrackMetadata
                    {
                        AudioTrackId = audioTrackMetadata["audioTrackId"].ToString(),
                        DisplayName = audioTrackMetadata["displayName"].ToString(),
                        GroupId = audioTrackMetadata["trackGroupId"].ToString()
                    });
                else
                    audioMetadata.Add(new AudioTrackMetadata
                    {
                        AudioTrackId = "_",
                        DisplayName = audioTrackMetadata["displayName"].ToString()
                    });
            }

            var audioRequest = Config.GetContentRequest(Config.Resource.Audio);

            if (Config.Bitrate == "CBR")
                audioRequest["url"] = audioRequest["url"].Replace("CBR", "CVBR%2CCBR&");

            JObject audioJson = Requests.RequestJson(audioRequest);
            string aMpdUrl = audioJson["audioVideoUrls"]["avCdnUrlSets"].OrderBy(x => int.Parse(x["cdnWeightsRank"].ToString())).First()["avUrlInfoList"].First()["url"].ToString();
            match = Regex.Match(aMpdUrl, @"(https?:\/\/.*\/)d.{0,1}\/.*~\/(.*)");
            aMpdUrl = match.Groups[1].Value + match.Groups[2].Value;

            DASHPlaylist playlist = new DASHPlaylist(new string[] { mpdUrl, aMpdUrl }, new bool[] { false, true }, audioMetadata);

            int i = 0;
            foreach (var subtitleTrack in avJson["subtitleUrls"])
            {
                playlist.SelectedSubtitleTracks.Add(new SubtitleTrack(i.ToString(), subtitleTrack["displayName"].ToString(), subtitleTrack["languageCode"].ToString(), false, new string[] { subtitleTrack["url"].ToString() }, 1, "dfxp"));
                i++;
            }

            string hdrFormat = avJson["audioVideoUrls"]["avCdnUrlSets"][0]["hdrFormat"].ToString();
            if (hdrFormat != "None")
            {
                foreach (var videoTrack in playlist.VideoTracks.Where(x => x.Codec.Contains("hev")))
                {
                    videoTrack.Hdr = hdrFormat == "Hdr10" ? HdrFormat.HDR10 : HdrFormat.DV;
                }
            }

            playlist.Validate();
            playlist.SelectTracks(Config.Resolution, null, Config.OnlyEng);
            playlist.SelectTracksByIDs(videoIDs, audioIDs);

            playlist.PrintInfo();

            //if (DeviceConfig.L1Available)
            //playlist.ServerCertificateB64 = null;
            //else
            playlist.ServerCertificateB64 = GetLicense(new byte[] { 0x08, 0x04 });

            return playlist;
        }

        public string GetLicense(byte[] challenge)
        {
            Logger.Verbose("Doing license request");
            Logger.Debug($"challenge: {Convert.ToBase64String(challenge)}");

            string licenseB64;
            Dictionary<string, dynamic> licenseRequest = Config.GetContentRequest(Config.Resource.License);
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                ["widevine2Challenge"] = Convert.ToBase64String(challenge),
                ["includeHdcpTestKeyInLicense"] = "true"
            };
            licenseRequest.Add("data", data);

            try
            {
                JObject licenseJson = Requests.RequestJson(licenseRequest);
                Logger.Debug(licenseJson.ToString());
                licenseB64 = licenseJson["widevine2License"]["license"].ToString();
            }
            catch
            {
                Logger.Warn("Could not request license, trying again");
                return null;
            }

            Logger.Debug(licenseB64);
            Logger.Verbose("License acquired");

            return licenseB64;
        }

        public string GetSubtitleFormat()
        {
            return "srt";
        }
    }
}
