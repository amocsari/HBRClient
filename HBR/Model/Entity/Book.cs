using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HBR.Model.Entity
{
    public class Book
    {
        [Key]
        [JsonProperty(nameof(BookId))]
        public string BookId { get; set; }

        [JsonProperty(nameof(Title))]
        public string Title { get; set; }

        [JsonProperty(nameof(Author))]
        public string Author { get; set; }

        [JsonProperty(nameof(CoverLocation))]
        public string CoverLocation { get; set; }
    }
}