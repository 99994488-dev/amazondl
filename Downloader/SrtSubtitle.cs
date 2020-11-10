using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmazonDL.Downloader
{
    public class SrtSubtitle
    {
        public List<SrtSubtitleItem> Items { get; set; } = new List<SrtSubtitleItem>();

        public override string ToString()
        {
            FilterItems();

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < Items.Count; i++)
            {
                stringBuilder.AppendLine((i + 1).ToString());
                stringBuilder.AppendLine(Items[i].ToString().Replace("\r\n\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n"));
            }

            return stringBuilder.ToString();
        }

        public void FilterItems()
        {
            var sharedTimecodeGroups = Items.GroupBy(x => x.Start.ToString()).Where(g => g.Count() > 1);

            foreach (var group in sharedTimecodeGroups)
            {
                var groupList = group.ToList();

                var mainItem = groupList[0];
                while (groupList.Count > 1)
                {
                    var otherItem = groupList[1];
                    mainItem.Concat(otherItem);

                    Items.Remove(otherItem);
                    groupList.Remove(otherItem);
                }
            }
        }
    }
}
