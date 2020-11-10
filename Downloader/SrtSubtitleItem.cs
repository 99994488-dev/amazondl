using System;
using System.Text;

namespace AmazonDL.Downloader
{
    public class SrtSubtitleItem
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public StringBuilder Text { get; set; } = new StringBuilder();

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{Start:HH:mm:ss,fff} --> {End:HH:mm:ss,fff}");
            stringBuilder.AppendLine(Text.ToString());

            return stringBuilder.ToString();
        }

        public void Concat(SrtSubtitleItem otherItem)
        {
            SrtSubtitleItem mainItem;
            SrtSubtitleItem discardItem;

            if (this.Start <= otherItem.Start)
            {
                mainItem = this;
                discardItem = otherItem;
            }
            else
            {
                mainItem = otherItem;
                discardItem = this;
            }

            if (discardItem.End > mainItem.End)
                mainItem.End = discardItem.End;

            mainItem.Text.Append(discardItem.Text.ToString());

            this.Text = mainItem.Text;
            this.Start = mainItem.Start;
        }
    }
}
