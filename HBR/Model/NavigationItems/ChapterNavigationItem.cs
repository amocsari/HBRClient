using System;
using System.Collections.Generic;

namespace HBR.Model.NavigationItem
{
    public class ChapterNavigationItem : NavigationItem
    {
        public string Src { get; set; }
        public List<ChapterNavigationItem> SubChapters { get; set; }
    }
}