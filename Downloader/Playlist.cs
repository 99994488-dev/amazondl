using AmazonDL.UtilLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmazonDL.Downloader
{
    public class Playlist
    {
        public List<string> OriginalPlaylists { get; set; } = new List<string>();
        public string ServerCertificateB64 { get; set; }
        public List<VideoTrack> VideoTracks { get; set; } = new List<VideoTrack>();
        public List<AudioTrack> AudioTracks { get; set; } = new List<AudioTrack>();
        public List<VideoTrack> SelectedVideoTracks { get; set; } = new List<VideoTrack>();
        public List<AudioTrack> SelectedAudioTracks { get; set; } = new List<AudioTrack>();
        public List<SubtitleTrack> SelectedSubtitleTracks { get; set; } = new List<SubtitleTrack>();

        static string[] VideoTrackPropertyNames { get; set; } = new string[] { "ID", "Bitrate", "Codec", "Frame Rate", "Width", "Height", "HDR" };
        static string[] AudioTrackPropertyNames { get; set; } = new string[] { "ID", "Bitrate", "Codec", "Name", "Language", "Channels" };
        static string[] SubtitleTrackPropertyNames { get; set; } = new string[] { "ID", "Name", "Language" };

        public void PrintInfo()
        {
            Logger.Track($"Video tracks:");
            if (VideoTracks.Count > 0)
            {
                if (Logger.IsTrack)
                {
                    foreach (var track in VideoTracks)
                    {
                        Console.WriteLine($"Bitrate: {track.Bitrate}, Codec: {track.Codec}, Width: {track.Width}, Height: {track.Height}, FPS: {track.FrameRate}");
                    }
                }
            }

            Logger.Track($"Audio tracks:");
            if (AudioTracks.Count > 0)
            {
                if (Logger.IsTrack)
                {
                    foreach (var track in AudioTracks)
                    {
                        Console.WriteLine($"Bitrate: {track.Bitrate}, Codec: {track.Codec}, Channels: {track.Channels}");
                    }
                }
            }

            if (SelectedVideoTracks.Count > 0)
            {
                Logger.Info("Selected video track:");

                foreach (var track in SelectedVideoTracks)
                {
                    Console.WriteLine($"Bitrate: {track.Bitrate}, Codec: {track.Codec}, Width: {track.Width}, Height: {track.Height}");
                }
            }

            if (SelectedAudioTracks.Count > 0)
            {
                Logger.Info("Selected audio track:");

                foreach (var track in SelectedAudioTracks)
                {
                    Console.WriteLine($"Bitrate: {track.Bitrate}, Codec: {track.Codec}, Channels: {track.Channels}");
                }
            }

            Logger.Info($"Subtitle tracks:");
            if (SelectedSubtitleTracks.Count > 0)
            {
                foreach (var track in SelectedSubtitleTracks)
                {
                    Console.WriteLine($"Code: {track.LanguageCode}, Name: {track.Name}");
                }
            }
        }

        public virtual void SelectTracks(int resolution, string codec, bool engOnly)
        {
            VideoTracks = VideoTracks.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Bitrate).OrderBy(x => x.Height).ToList();
            AudioTracks = AudioTracks.GroupBy(x => x.Id).Select(x => x.First()).OrderByDescending(x => x.Codec.Contains("dd")).OrderByDescending(x => x.Codec.Contains("ddp")).OrderByDescending(x => x.Codec.Contains("atmos")).OrderBy(x => x.Bitrate).ToList();
            SelectedSubtitleTracks = SelectedSubtitleTracks.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Name).ThenByDescending(x => x.LanguageCode.Contains("en")).ToList();

            List<VideoTrack> videoTrackPool = VideoTracks;
            List<AudioTrack> audioTrackPool = AudioTracks;

            /*if (DeviceConfig.L1Available)
            {
                videoTrackPool = VideoTracks.Where(x => x.SecurityLevel != ITrack.SecurityLevel.Hardware).ToList();
                audioTrackPool = AudioTracks.Where(x => x.SecurityLevel != ITrack.SecurityLevel.Hardware).ToList();
            }*/

            if (codec != null)
            {
                if (codec == "H264")
                    videoTrackPool = videoTrackPool.Where(x => x.Codec.Contains("avc")).ToList();
                else if (codec == "H265")
                    videoTrackPool = videoTrackPool.Where(x => x.Codec.Contains("hvc") || x.Codec.Contains("hev")).ToList();
            }

            if (resolution == 0)
            {
                SelectedVideoTracks.Add(videoTrackPool.Last());
            }
            else
            {
                try
                {
                    SelectedVideoTracks.Add(videoTrackPool.Where(x => x.Height == resolution).Last());
                }
                catch
                {
                    SelectedVideoTracks.Add(videoTrackPool.Last());
                }
            }

            string origLang = "en";

            foreach (var track in AudioTracks)
            {
                if (track.Name.Contains("Original"))
                {
                    origLang = track.Language;
                    break;
                }
            }

            SelectedAudioTracks = audioTrackPool.GroupBy(x => x.Language + x.Name.Contains("Description").ToString()).Select(x => x.OrderByDescending(y => y.Name.Contains("Description")).OrderByDescending(y => y.Bitrate).FirstOrDefault()).ToList();
            SelectedAudioTracks.AddRange(audioTrackPool.Where(x => x.Name.Contains("Description")).GroupBy(x => x.Language).Select(x => x.OrderByDescending(y => y.Name.Contains("Description")).OrderByDescending(y => y.Bitrate).FirstOrDefault()));
            SelectedAudioTracks = SelectedAudioTracks.OrderByDescending(y => y.Channels).OrderByDescending(y => y.Bitrate).OrderBy(x => x.Language).OrderByDescending(x => x.Language.Contains("en")).OrderByDescending(x => x.Language.Contains("en-us")).OrderByDescending(x => x.Language == origLang).ToList();

            SelectedSubtitleTracks = SelectedSubtitleTracks.OrderBy(x => x.LanguageCode).OrderByDescending(x => x.LanguageCode == origLang).OrderByDescending(x => x.LanguageCode.Contains("en")).Where(x => !x.Name.Contains("Forced")).ToList();

            if (engOnly)
                SelectedAudioTracks = SelectedAudioTracks.Where(x => (x.Language.Contains("en") || x.Language == origLang) && !x.Name.Contains("Description")).ToList();
            if (engOnly)
                SelectedSubtitleTracks = new List<SubtitleTrack>() { SelectedSubtitleTracks.Where(x => x.LanguageCode.Contains("en") || x.LanguageCode == origLang).FirstOrDefault() };

            SelectedAudioTracks = SelectedAudioTracks.Distinct().ToList();
            SelectedSubtitleTracks = SelectedSubtitleTracks.Distinct().ToList();

            /*
            foreach (var subtitleTrack in SelectedSubtitleTracks)
            {
                if (subtitleTrack.LanguageCode == origLang)
                {
                    subtitleTrack.Default = true;
                    break;
                }
            }
            */
        }

        public void SelectTracksByIDs(List<string> videoIDs, List<string> audioIds)
        {
            if (videoIDs != null && videoIDs.Count > 0)
            {
                List<VideoTrack> newTracks = new List<VideoTrack>();
                foreach (var track in VideoTracks)
                {
                    if (videoIDs.Contains(track.Id))
                        newTracks.Add(track);
                }
                if (newTracks.Count > 0)
                    SelectedVideoTracks = newTracks;
            }
            if (audioIds != null && audioIds.Count > 0)
            {
                List<AudioTrack> newTracks = new List<AudioTrack>();
                foreach (var track in AudioTracks)
                {
                    if (audioIds.Contains(track.Id))
                        newTracks.Add(track);
                }
                if (newTracks.Count > 0)
                    SelectedAudioTracks = newTracks;
            }
        }

        public void Validate()
        {
            if (VideoTracks.Count == 0)
            {
                //Logger.Warn("No video tracks found");
            }

            if (AudioTracks.Count == 0)
            {
                //Logger.Warn("No audio tracks found");
            }
        }
    }
}
