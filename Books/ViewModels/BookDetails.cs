using Books.Models;

namespace Books.ViewModels
{
    public class BookDetails
    {
        public Book Book;
        public string Purchased;
        public IList<Review> Reviews;
    }
}
