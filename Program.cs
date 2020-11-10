using AmazonDL.Core;
using AmazonDL.Downloader;
using System;

namespace AmazonDL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 6)
            {
                bool license = false;
                if (args[0] == "license")
                    license = true;

                try
                {
                    var config = new Config(args[1], args[2], int.Parse(args[3]), null, args[4], args[5], true, null);
                    var client = new Client(config);
                    var downloaderConfig = new DownloaderConfig(client, client.GetFilename(), "srt", false, false, false, license, true, "NOGRP", false, false, false, false, false, null, null);
                    var downloader = new Downloader.Downloader(downloaderConfig);
                    downloader.Run();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

#if (DEBUG == true)
                    Console.WriteLine(e.ToString());
#endif
                }
            }
            else
            {
                Console.WriteLine("Usage: amazondl <mode> <asin> <region> <resolution> <bitrate> <codec>\n" +
                    "Regions: us, uk, jp, de, pv, pv-eu, pv-fe\n" +
                    "Bitrates: CBR, VBR\n" +
                    "Codecs: H264, H265\n" +
                    "Modes: download, license");
            }

            Console.WriteLine("Press CTRL+C to exit . . .");
        }
    }
}
