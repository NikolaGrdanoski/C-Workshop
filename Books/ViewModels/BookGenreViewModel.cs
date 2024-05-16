using Books.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Books.ViewModels
{
    public class BookGenreViewModel
    {
        public IList<Book> Books { get; set; }
        public SelectList Genre { get; set; }
        public IList<Review> Reviews { get; set; }
        public string BookGenre { get; set; }
        public string SearchString { get; set; }
    }
}
