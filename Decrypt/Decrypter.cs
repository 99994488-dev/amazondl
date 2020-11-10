using AmazonDL.Downloader;
using AmazonDL.UtilLib;
using System.Collections.Generic;

namespace AmazonDL.Decrypt
{
    public class Decrypter
    {
        DecryptConfig Config { get; set; }
        CDMApi CDM { get; set; }
        public List<ContentKey> Keys { get; set; }

        public Decrypter(DecryptConfig config)
        {
            Config = config;
            CDM = new CDMApi();
        }

        public void StartProcess()
        {
            if (Keys == null)
                Keys = CDM.GetKeys();

            string[] commandline = Config.BuildCommandLine(Keys);
            //Logger.Info(commandline[0] + " " + commandline[1]);

            foreach (var key in Keys)
            {
                Logger.Key($"{Config.Tracktype} - {key}", Config.Filename);
            }

            if (!Config.License)
            {
                Logger.Verbose(LogMessage("Starting mp4decrypt process"));
                Utils.RunCommand(commandline[0], commandline[1]);
                Logger.Verbose(LogMessage("mp4decrypt succesful"));
            }
        }

        public byte[] GetChallenge()
        {
            return CDM.GetChallenge(Config.InitDataB64, Config.CertDataB64, false, false);
        }

        public bool UpdateLicense(string licenseB64)
        {
            return CDM.ProvideLicense(licenseB64);
        }

        public string LogMessage(string message)
        {
            return $"{Config.Tracktype}_{Config.TrackId} : {message}";
        }
    }
}
