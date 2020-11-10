using AmazonDL.Core;
using AmazonDL.Decrypt;
using AmazonDL.UtilLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonDL.Downloader
{
    class Downloader
    {
        DownloaderConfig Config { get; set; }
        Client Client { get; set; }
        string Filename { get; set; }
        List<string> Cleanup { get; set; } = new List<string>();

        public Downloader(DownloaderConfig config)
        {
            Config = config;
            Client = config.Client;
            Filename = config.Filename.Replace(":","").Replace("?","");
        }

        bool DownloadTrack(ITrack track)
        {
            string merged = track.GetFilename(Filename, !track.Encrypted);
            if (File.Exists(merged) && !Config.Overwrite)
                return true;

            if (track.Segments > 1)
            {
                string segmentFile = merged + ".segments";
                if (File.Exists(segmentFile))
                    File.Delete(segmentFile);

                File.Create(segmentFile).Close();

                for (int i = 0; i < track.Segments; i++)
                {
                    var lines = new string[]
                    {
                        track.Urls[i],
                        $" out={Path.GetFileName(merged)}_{i}"
                    };
                    File.AppendAllLines(segmentFile, lines);
                }

                int command = Utils.RunCommandCode(Constants.ARIA_BINARY_PATH, $"-x16 -s16 -i \"{merged}.segments\" -d \"{Path.GetDirectoryName(merged)}\"");

                File.Delete(segmentFile);

                using FileStream output = File.Create(merged);
                for (int i = 0; i < track.Segments; i++)
                {
                    using FileStream input = File.OpenRead(merged + "_" + i);
                    input.CopyTo(output);
                    input.Close();

                    try
                    {
                        File.Delete(merged + "_" + i);
                    }
                    catch
                    {
                        Cleanup.Add(merged + "_" + i);
                    }
                }
            }
            else
            {
                int command = Utils.RunCommandCode(Constants.ARIA_BINARY_PATH, $"-x16 -s16 \"{track.Urls[0]}\" -d \"{Path.GetDirectoryName(merged)}\" -o \"{Path.GetFileName(merged)}\"");
            }

            Cleanup.Add(merged);

            return true;
        }

        bool DoDecrypt(ITrack track, string serverCert)
        {
            var config = new DecryptConfig(Filename, track.GetTrackType(), track.Id, Config.License, null, track.InitDataB64, serverCert);

            string encryptedName = track.GetFilename(Filename, false, false);
            string decryptedName = track.GetFilename(Filename, true, false);

            Cleanup.Add(track.GetFilename(Filename, true, false));

            int retries = 0;
            while (retries < 10)
            {
                Decrypter decrypt = new Decrypter(config);

                var challenge = decrypt.GetChallenge();

                string licenseB64 = Client.GetLicense(challenge);

                if (licenseB64 == null)
                {
                    retries++;
                    continue;
                }

                if (!decrypt.UpdateLicense(licenseB64))
                {
                    retries++;
                    continue;
                }

                decrypt.StartProcess();

                if (File.Exists(encryptedName) && File.Exists(decryptedName) && !Config.SkipCleanup)
                    File.Delete(encryptedName);

                return true;
            }
            return false;
        }

        void DownloadAndConvertSubtitle(SubtitleTrack subtitle)
        {
            string languageCode = subtitle.LanguageCode;
            string id = subtitle.Id;
            Logger.Verbose($"Downloading {languageCode}_{id} subtitles");
            string outputFile = Path.Join(Constants.TEMP_FOLDER, string.Join("_", Filename, languageCode, id, Config.SubtitleFormat));

            if (Config.Overwrite && File.Exists(outputFile))
                File.Delete(outputFile);
            else if (!Config.Overwrite && File.Exists(outputFile))
                return;

            List<string> subtitleSegments = new List<string>(new string[subtitle.Segments]);

            Parallel.For(0, subtitle.Segments, (i) =>
            {
                subtitleSegments[i] = Requests.Request(subtitle.Urls[i], new Dictionary<string, string>() { ["keep-alive"] = "timeout=10, max=1000" });
            });

            string subtitleText = string.Join("\r\n", subtitleSegments);
            SrtSubtitle srtSubtitle = null;

            if (subtitle.GetTrackType() == "webvtt")
            {
                srtSubtitle = SubtitleUtils.ConvertVTTToSubrip(string.Join("\r\n", subtitleSegments));
            }
            else if (subtitle.GetTrackType() == "dfxp")
            {
                srtSubtitle = SubtitleUtils.ConvertDFXPToSubrip(subtitleText);
            }

            if (srtSubtitle != null)
            {
                File.WriteAllText(outputFile, srtSubtitle.ToString());
            }
        }

        void DoMerge(List<VideoTrack> videoTracks, List<AudioTrack> audioTracks, List<SubtitleTrack> subtitleTracks)
        {

            string muxedFilename = Path.Join(Constants.OUTPUT_FOLDER, Filename + ".mkv");

            string mkvMergeArgs = $"--output \"{muxedFilename}\" ";

            if (!Config.AudioOnly)
            {
                foreach (VideoTrack videoTrack in videoTracks.OrderByDescending(x => x.Height))
                {
                    string videoFilename = videoTrack.GetFilename(Filename, true, false);
                    mkvMergeArgs += $"--language 0:und \"{videoFilename}\" ";
                }
            }

            if (!Config.VideoOnly)
            {
                foreach (AudioTrack audioTrack in audioTracks)
                {
                    string audioFilename = audioTrack.GetFilename(Filename, true, false);
                    
                    mkvMergeArgs += $"--language 0:und \"{audioFilename}\" ";
                }
            }

            bool defaultAlreadyAdded = false;

            subtitleTracks = subtitleTracks.OrderBy(subtitle => subtitle.LanguageCode).OrderByDescending(subtitle => subtitle.LanguageCode.Contains("en")).ToList();
            foreach (SubtitleTrack subtitle in subtitleTracks)
            {
                mkvMergeArgs += $"--language 0:und --sub-charset 0:UTF-8 --default-track 0:no \"{subtitle.GetFilename(Filename, Config.SubtitleFormat)}\" ";
            }

            Utils.RunCommand(Constants.MKVMERGE_BINARY_PATH, mkvMergeArgs);
        }

        void EditFilename(List<VideoTrack> videoTracks, List<AudioTrack> audioTracks)
        {
            string videoCodec = "NoVideo";

            if (videoTracks.Count > 0)
            {
                videoCodec = videoTracks.OrderByDescending(x => x.Bitrate).First().Codec;
                if (videoCodec.Contains("h264") || videoCodec.Contains("avc"))
                    videoCodec = "H.264";
                else if (videoCodec.StartsWith("hev") || videoCodec.Contains("hevc"))
                    videoCodec = "H.265";
                else if (videoCodec.Contains("vp9"))
                    videoCodec = "VP9";
            }

            string audioCodec = "NoAudio";

            if (audioTracks.Count > 0)
            {
                audioCodec = "AAC";

                foreach (var audioTrack in audioTracks)
                {
                    if (audioTrack.Codec.Contains("mp4a") || audioTrack.Codec.Contains("aac"))
                    {
                        audioCodec = "AAC";
                        break;
                    }
                }
                foreach (var audioTrack in audioTracks)
                {
                    if (audioTrack.Codec.Contains("ac-3") || audioTrack.Codec.Contains("dd-"))
                    {
                        audioCodec = "DD";
                        break;
                    }
                }
                foreach (var audioTrack in audioTracks)
                {
                    if (audioTrack.Codec.Contains("ec-3") || audioTrack.Codec.Contains("ddp") || audioTrack.Codec.Contains("atmos"))
                    {
                        audioCodec = "DDP";
                        break;
                    }
                }

                if (audioTracks.OrderByDescending(x => x.Channels).First().Channels == 16)
                    audioCodec += "7.1";
                else if (audioTracks.OrderByDescending(x => x.Channels).First().Channels == 6 || audioTracks.OrderByDescending(x => x.Channels).First().Channels == 8)
                    audioCodec += "5.1";
                else if (audioTracks.OrderByDescending(x => x.Channels).First().Channels == 2)
                    audioCodec += "2.0";

                foreach (var audioTrack in audioTracks)
                {
                    if (audioTrack.Codec.Contains("atmos"))
                    {
                        audioCodec += ".Atmos";
                        break;
                    }
                }
            }

            if (videoTracks.OrderByDescending(x => x.Bitrate).First().Hdr != HdrFormat.None)
                audioCodec = "HDR." + audioCodec;

            int resolution = videoTracks.OrderByDescending(x => x.Height).First().Height;

            Filename = Filename.Replace("{resolution}", resolution.ToString()).Replace("{vcodec}", videoCodec).Replace("{acodec}", audioCodec).Replace("{TaG}", Config.Tag);
        }

        public bool Run()
        {
            Console.Title = Config.Filename;

            if (!Directory.Exists(Constants.OUTPUT_FOLDER))
                Directory.CreateDirectory(Constants.OUTPUT_FOLDER);

            if (!Directory.Exists(Constants.TEMP_FOLDER))
                Directory.CreateDirectory(Constants.TEMP_FOLDER);

            if (!Client.Login())
                return false;

            Logger.Verbose("Getting playlist data");
            Playlist playlist = Client.GetPlaylist(Config.VideoIds, Config.AudioIds);

            if (playlist == null)
            {
                Logger.Error("Getting playlist failed");
                return false;
            }

            if (Config.PrintInfo)
            {
                Logger.Info("Info mode complete");
                return true;
            }

            EditFilename(playlist.SelectedVideoTracks, playlist.SelectedAudioTracks);
            
            Console.Title = Config.Filename;

            string oldTitle = Console.Title;

            if (!Config.License)
            {
                Logger.Debug("Download URLs:");
                foreach (ITrack track in new ITrack[] { }.Concat(playlist.SelectedVideoTracks).Concat(playlist.SelectedAudioTracks))
                    if (track.Segments == 1)
                        foreach (var url in track.Urls)
                        {
                            Logger.Debug(track.GetFilename(Filename, false, false));
                            Logger.Debug(track.Urls[0]);
                        }

                if (!(Config.AudioOnly || Config.VideoOnly))
                {
                    if (playlist.SelectedSubtitleTracks.Count > 0)
                    {
                        Logger.Info("Downloading subtitles . . .");
                        foreach (var subtitle in playlist.SelectedSubtitleTracks)
                            DownloadAndConvertSubtitle(subtitle);
                        Logger.Verbose("All subtitles downloaded");
                    }
                }

                if (Config.SubsOnly && !Config.AudioOnly && !Config.VideoOnly)
                {
                    foreach (var subtitle in playlist.SelectedSubtitleTracks)
                    {
                        string subFilename = subtitle.GetFilename(Filename);
                        File.Move(subFilename, subFilename.Replace(Constants.TEMP_FOLDER, Constants.OUTPUT_FOLDER));
                    }
                    Logger.Info("Downloaded subtitles. Done!");
                    return true;
                }

                Requests.ResetCounter();
                Logger.Info("Downloading video . . .");
                if (!Config.AudioOnly)
                {
                    foreach (var videoTrack in playlist.SelectedVideoTracks)
                    {
                        if (!DownloadTrack(videoTrack))
                        {
                            Logger.Error($"Error downloading video track {videoTrack.Id}");
                            Config.License = true;
                        }
                    };
                }

                Requests.ResetCounter();
                Logger.Info("Downloading audio . . .");
                if (!Config.VideoOnly)
                {
                    foreach (var audioTrack in playlist.SelectedAudioTracks)
                    {
                        if (!DownloadTrack(audioTrack))
                        {
                            Logger.Error($"Error downloading audio track {audioTrack.Id}");
                            Config.License = true;
                        }
                    };
                }
            }

            Console.Title = oldTitle;

            if (Config.EncryptedOnly)
            {
                foreach (ITrack track in playlist.SelectedAudioTracks.Cast<ITrack>().Concat(playlist.SelectedVideoTracks))
                {
                    try
                    {
                        string originalFilename = track.GetFilename(Filename, !track.Encrypted, false);
                        File.Move(originalFilename, originalFilename.Replace(Constants.TEMP_FOLDER, Constants.OUTPUT_FOLDER));
                    }
                    catch
                    {

                    }
                }
                if (!(Config.AudioOnly || Config.VideoOnly))
                {
                    foreach (var subtitle in playlist.SelectedSubtitleTracks)
                    {
                        string filename = subtitle.GetFilename(Filename, Config.SubtitleFormat);
                        File.Move(filename, filename.Replace(Constants.TEMP_FOLDER, Constants.OUTPUT_FOLDER));
                    }
                }

                return true;
            }

            Logger.Info("Decrypting tracks . . .");

            List<ITrack> encryptedTracks = new List<ITrack>();
            if (!Config.AudioOnly)
                encryptedTracks.AddRange(playlist.SelectedVideoTracks.Where(track => track.Encrypted == true).Cast<ITrack>());
            if (!Config.VideoOnly)
                encryptedTracks.AddRange(playlist.SelectedAudioTracks.Where(track => track.Encrypted == true).Cast<ITrack>());
            
            foreach (ITrack track in encryptedTracks)
            {
                if (!DoDecrypt(track, playlist.ServerCertificateB64))
                    return false;
            }

            if (Config.License)
                return true;
            else
            {
                Logger.Verbose("All decryption complete");

                if (Config.DontMux)
                {
                    Logger.Info("Moving unmuxed tracks");
                    if (!Config.AudioOnly)
                    {
                        foreach (ITrack videoTrack in playlist.SelectedVideoTracks)
                        {
                            string videoFilename = videoTrack.GetFilename(Filename, true, false);
                            File.Move(videoFilename, videoFilename.Replace(Constants.TEMP_FOLDER, Constants.OUTPUT_FOLDER));
                        }
                    }

                    if (!Config.VideoOnly)
                    {
                        foreach (ITrack audioTrack in playlist.SelectedAudioTracks)
                        {
                            string audioFilename = audioTrack.GetFilename(Filename, true, false);
                            File.Move(audioFilename, audioFilename.Replace(Constants.TEMP_FOLDER, Constants.OUTPUT_FOLDER));
                        }
                    }

                    foreach (var subtitle in playlist.SelectedSubtitleTracks)
                    {
                        string filename = subtitle.GetFilename(Filename, Config.SubtitleFormat);
                        File.Move(filename, filename.Replace(Constants.TEMP_FOLDER, Constants.OUTPUT_FOLDER));
                    }
                }
                else
                {
                    Logger.Info("Muxing tracks . . .");
                    DoMerge(playlist.SelectedVideoTracks, playlist.SelectedAudioTracks, playlist.SelectedSubtitleTracks);
                }

                Logger.Verbose("Muxed file written");
            }

            if (Config.SkipCleanup)
            {
                Logger.Info("Skipping cleanup");
                return true;
            }

            foreach (string file in Directory.GetFiles(Constants.TEMP_FOLDER, "*"))
                if (file.Contains(Filename))
                    File.Delete(file);

            foreach (string file in Cleanup.Distinct())
                if (File.Exists(file))
                    File.Delete(file);

            Logger.Info("Complete");

            return true;
        }
    }
}