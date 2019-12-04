using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBR.Model.Entity
{
    public class Bookmark
    {
        [Key]
        public string BookmarkId { get; set; }

        [ForeignKey(nameof(Book.Bookmarks))]
        public string BookId { get; set; }

        public string Description { get; set; }

        public int ChapterIndex { get; set; }

        public int? SubChapterIndex { get; set; }

        public int Position { get; set; }
    }
}