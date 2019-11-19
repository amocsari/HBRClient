using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HBR
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