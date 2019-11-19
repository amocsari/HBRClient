using System;
using System.Collections.Generic;

namespace HBR.Model
{
    public class Chapter
    {
        public int MenuItemId { get; set; }
        public string ChapterTitle { get; set; }
        public string Src { get; set; }
        public List<Chapter> SubChapters { get; set; }
        public Action<bool> OnClickCallback { get; set; }
    }
}