using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace AmazonDL.Downloader
{
    class SubtitleUtils
    {
        public static SrtSubtitle ConvertDFXPToSubrip(string dfxpSubtitle)
        {
            var subtitle = new SrtSubtitle();

            dfxpSubtitle = dfxpSubtitle.Replace("<tt:br/>", "breakline;;");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(dfxpSubtitle);

            foreach (XmlElement div in xmlDoc["tt:tt"]["tt:body"].Cast<XmlElement>().Where(x => x.Name == "tt:div"))
            {
                foreach (XmlElement p in div)
                {
                    var srtItem = new SrtSubtitleItem();

                    srtItem.Start = DateTime.Parse(p.GetAttribute("begin"));
                    srtItem.End = DateTime.Parse(p.GetAttribute("end"));

                    string text = p.InnerText.Replace("breakline;;", "\r\n");
                    srtItem.Text.AppendLine(text);
                    srtItem.Text.Append("\r\n");

                    subtitle.Items.Add(srtItem);
                }
            }

            return subtitle;
        }

        public static SrtSubtitle ConvertVTTToSubrip(string webvttSubtitle)
        {
            var subtitle = new SrtSubtitle();

            StringReader reader = new StringReader(webvttSubtitle);

            double timeOffset = 0.0;

            int lineNumber = 1;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("X-TIMESTAMP-MAP=MPEGTS"))
                {
                    timeOffset = (double.Parse(line.Substring(line.IndexOf(":") + 1, line.IndexOf(",") - line.IndexOf(":") - 1)) - 900000) / 90000.0;
                }
                else if (IsTimecode(line))
                {
                    var srtItem = new SrtSubtitleItem();

                    lineNumber++;

                    line = line.Replace('.', ',');

                    line = DeleteCueSettings(line);

                    string timeSrt1 = line.Substring(0, line.IndexOf('-'));
                    string timeSrt2 = line.Substring(line.IndexOf('>') + 1);
                    DateTime timeAux1 = DateTime.ParseExact(timeSrt1.Trim(), "hh:mm:ss,fff", CultureInfo.InvariantCulture).AddSeconds(timeOffset);
                    DateTime timeAux2 = DateTime.ParseExact(timeSrt2.Trim(), "hh:mm:ss,fff", CultureInfo.InvariantCulture).AddSeconds(timeOffset);

                    srtItem.Start = timeAux1;
                    srtItem.End = timeAux2;

                    bool foundCaption = false;
                    while (true)
                    {
                        if ((line = reader.ReadLine()) == null)
                        {
                            if (foundCaption)
                                break;
                            else
                                throw new Exception("Invalid file");
                        }
                        if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                        {
                            srtItem.Text.AppendLine();
                            break;
                        }
                        foundCaption = true;
                        srtItem.Text.AppendLine(line);
                    }

                    subtitle.Items.Add(srtItem);
                }
            }

            return subtitle;
        }

        static bool IsTimecode(string line)
        {
            return line.Contains("-->");
        }

        static string DeleteCueSettings(string line)
        {
            StringBuilder output = new StringBuilder();
            foreach (char ch in line)
            {
                char chLower = char.ToLower(ch);
                if (chLower >= 'a' && chLower <= 'z')
                {
                    break;
                }
                output.Append(ch);
            }
            return output.ToString();
        }
    }
}
