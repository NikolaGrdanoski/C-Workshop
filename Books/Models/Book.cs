using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;

namespace Books.Models
{
    public class Book
    {
        public int Id { get; set; }

        [StringLength(100)]
        [Required]
        public string Title { get; set; }

        [Display(Name = "Year Published")]
        [Required]
        public int? YearPublished { get; set; }

        [Display(Name = "Number of pages")]
        [Required]
        public int? NumPages { get; set; }

        [StringLength(int.MaxValue)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Publisher { get; set; }

        [StringLength(int.MaxValue)]
        public string? FrontPage { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Download link")]
        public string? DownloadUrl { get; set; }

        public Author? Author { get; set; }

        [Display(Name = "Author")]
        public int AuthorId { get; set; }

        public ICollection<BookGenre>? Genres { get; set; }

        public ICollection<Review>? Reviews { get; set; }

        public ICollection<UserBooks>? Users { get; set; }

        [Display(Name = "Average Rating")]
        public double AverageRating()
        {
            if (Reviews != null && Reviews.Any() && Reviews.All(r => r.Rating != null))
            {
                int Rating = (int)Reviews.Sum(r => r.Rating);
                return (double)Rating / Reviews.Count;
            }

            return 0;
        }
    }
}
