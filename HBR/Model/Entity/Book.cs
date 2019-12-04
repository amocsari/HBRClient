using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBR.Model.Entity
{
    public class Book
    {
        [Key]
        public string BookId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string CoverLocation { get; set; }

        [ForeignKey(nameof(LastPosition))]
        public string LastPositionId { get; set; }

        public virtual List<Bookmark> Bookmarks { get; set; }
        public virtual Bookmark LastPosition { get; set; }
    }
}