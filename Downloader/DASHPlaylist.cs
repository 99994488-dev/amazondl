using AmazonDL.UtilLib;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AmazonDL.Downloader
{
    public class AudioTrackMetadata
    {
        public string AudioTrackId { get; set; }
        public string DisplayName { get; set; }
        public string GroupId { get; set; }
    }

    public class DASHPlaylist : Playlist
    {
        public DASHPlaylist(string[] playlistUrls, bool[] playlistsAudioOnly, List<AudioTrackMetadata> audioMetadata = null)
        {
            foreach ((string playlistUrl, bool audioOnly) in playlistUrls.Zip(playlistsAudioOnly))
            {
                string originalPlaylist = Requests.Request(playlistUrl, null);

                XmlDocument xmlDoc = new XmlDocument();

                try
                {
                    xmlDoc.LoadXml(originalPlaylist);
                    OriginalPlaylists.Add(originalPlaylist);
                }
                catch
                {
                    continue;
                }

                string baseUrl = playlistUrl.Substring(0, playlistUrl.LastIndexOf("/") + 1);

                Dictionary<string, string> initDataPairs = new Dictionary<string, string>();

                foreach (XmlElement adaptationSet in xmlDoc["MPD"]["Period"].GetElementsByTagName("AdaptationSet"))
                {
                    try
                    {
                        string keyId;
                        try
                        {
                            keyId = adaptationSet.GetElementsByTagName("ContentProtection").Cast<XmlElement>().Where(x => x.HasAttribute("cenc:default_KID")).FirstOrDefault().GetAttribute("cenc:default_KID");
                        }
                        catch
                        {
                            keyId = adaptationSet.GetElementsByTagName("ContentProtection").Cast<XmlElement>().Where(x => x.HasAttribute("_:default_KID")).FirstOrDefault().GetAttribute("_:default_KID");
                        }
                        string initData = adaptationSet.GetElementsByTagName("ContentProtection").Cast<XmlElement>().Where(x => x.GetAttribute("schemeIdUri").ToLower().Contains("edef8ba9-79d6-4ace-a3c8-27dcd51d21ed")).FirstOrDefault().FirstChild.InnerText;
                        if (!initDataPairs.ContainsKey(keyId))
                            initDataPairs.Add(keyId, initData);
                    }
                    catch { }
                }

                foreach (XmlElement adaptationSet in xmlDoc["MPD"]["Period"].GetElementsByTagName("AdaptationSet"))
                {
                    bool isEncrypted = false;
                    string initDataB64 = null;

                    XmlNodeList contentProtectionElements = adaptationSet.GetElementsByTagName("ContentProtection");

                    if (contentProtectionElements.Count > 0)
                    {
                        isEncrypted = true;
                        string keyId;
                        try
                        {
                            keyId = contentProtectionElements.Cast<XmlElement>().Where(x => x.HasAttribute("cenc:default_KID")).FirstOrDefault().GetAttribute("cenc:default_KID");
                        }
                        catch
                        {
                            keyId = contentProtectionElements.Cast<XmlElement>().Where(x => x.HasAttribute("_:default_KID")).FirstOrDefault().GetAttribute("_:default_KID");
                        }
                        initDataB64 = initDataPairs[keyId];
                    }

                    foreach (XmlElement representation in adaptationSet.GetElementsByTagName("Representation"))
                    {
                        string url = representation.GetElementsByTagName("BaseURL")[0].InnerText;

                        if (!url.Contains("https://") && !url.Contains("http://"))
                            url = baseUrl + url;

                        if (url.StartsWith("http:"))
                            url = url.Replace("http:", "https:");

                        if (adaptationSet.GetAttribute("contentType").ToLower() == "audio" || adaptationSet.GetAttribute("mimeType").ToLower().Contains("audio"))
                        {
                            string name = null;
                            if (adaptationSet.HasAttribute("label"))
                                name = adaptationSet.GetAttribute("label");

                            string codec = null;

                            if (representation.HasAttribute("codecs"))
                            {
                                codec = representation.GetAttribute("codecs");
                            }
                            else
                            {
                                codec = adaptationSet.GetAttribute("codecs");
                            }

                            int channels = 2;

                            try
                            {
                                XmlElement audioChannelConfiguration = null;

                                try
                                {
                                    audioChannelConfiguration = representation.GetElementsByTagName("AudioChannelConfiguration").Cast<XmlElement>().FirstOrDefault();
                                }
                                catch
                                {
                                    audioChannelConfiguration = adaptationSet.GetElementsByTagName("AudioChannelConfiguration").Cast<XmlElement>().FirstOrDefault();
                                }

                                if (audioChannelConfiguration == null)
                                {
                                    audioChannelConfiguration = adaptationSet.GetElementsByTagName("AudioChannelConfiguration").Cast<XmlElement>().FirstOrDefault();
                                }

                                string channelValue = audioChannelConfiguration.GetAttribute("value");

                                if (channelValue == "2")
                                    channels = 2;
                                else if (channelValue == "A000")
                                    channels = 2;
                                else if (channelValue == "F801" || channelValue == "6")
                                    channels = 6;
                                else if (channelValue == "FA01" || channelValue == "8")
                                    channels = 8;
                            }
                            catch
                            {
                                channels = 2;
                            }

                            int bandwidth = int.Parse(representation.GetAttribute("bandwidth"));

                            string id = representation.GetAttribute("id");
                            if (adaptationSet.HasAttribute("audioTrackId"))
                                id = adaptationSet.GetAttribute("audioTrackId") + "_" + id;

                            string lang;

                            try
                            {
                                //amzn
                                lang = id.Split("_")[0];
                            }
                            catch
                            {
                                lang = adaptationSet.GetAttribute("lang");
                            }

                            if (lang == null || lang == "")
                                lang = "en";

                            if (audioMetadata != null && audioMetadata.Count > 1 && adaptationSet.HasAttribute("audioTrackId"))
                            {
                                string trackId = adaptationSet.GetAttribute("audioTrackId");

                                name = audioMetadata.FirstOrDefault(x => x.AudioTrackId == trackId).DisplayName;
                            }
                            else if (audioMetadata != null && audioMetadata.Count == 1)
                            {
                                name = audioMetadata[0].DisplayName;
                            }

                            if (name == null)
                                name = lang;

                            var audioTrack = new AudioTrack
                            {
                                Encrypted = isEncrypted,
                                Id = id,
                                Name = name,
                                InitDataB64 = initDataB64,
                                Urls = new string[] { url },
                                Segments = 1,
                                Codec = codec,
                                Bitrate = bandwidth,
                                Channels = channels,
                                Language = lang,
                                SecurityLevel = ITrack.SecurityLevel.Unknown
                            };

                            AudioTracks.Add(audioTrack);
                        }
                        else if ((adaptationSet.GetAttribute("contentType").ToLower() == "video" || adaptationSet.GetAttribute("mimeType").ToLower().Contains("video")) && !audioOnly)
                        {
                            string frameRate = "24000/1001";

                            if (representation.HasAttribute("frameRate"))
                            {
                                frameRate = representation.GetAttribute("frameRate");
                            }
                            else
                            {
                                frameRate = adaptationSet.GetAttribute("frameRate");
                            }

                            var frameRateValue = 0.0;
                            if (frameRate.Contains("/"))
                                frameRateValue = Utils.EvaluateEquation(frameRate);
                            else
                                frameRateValue = float.Parse(frameRate);

                            int bandwidth = int.Parse(representation.GetAttribute("bandwidth"));

                            try
                            {
                                bandwidth = int.Parse(representation.GetAttribute("id").Split("=")[1]);
                            }
                            catch { }

                            var videoTrack = new VideoTrack
                            {
                                Encrypted = isEncrypted,
                                Id = representation.GetAttribute("id"),
                                InitDataB64 = initDataB64,
                                Urls = new string[] { url },
                                Segments = 1,
                                Codec = representation.GetAttribute("codecs"),
                                Bitrate = bandwidth,
                                Width = int.Parse(representation.GetAttribute("width")),
                                Height = int.Parse(representation.GetAttribute("height")),
                                FrameRate = frameRateValue,
                                SecurityLevel = ITrack.SecurityLevel.Unknown
                            };

                            VideoTracks.Add(videoTrack);
                        }
                    }
                }
            }
        }
    }
}
