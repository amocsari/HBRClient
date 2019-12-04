using System;

namespace HBR.Model.NavigationItem
{
    public class NavigationItem
    {
        public int MenuItemId { get; set; }
        public string Text { get; set; }
        public Action OnClickCallback { get; set; }
    }
}