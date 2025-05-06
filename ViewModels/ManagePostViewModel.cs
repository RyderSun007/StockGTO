using X.PagedList;
namespace StockGTO.Models.ViewModels
{
    public class ManagePostViewModel
    {
        public IPagedList<Post> PostList { get; set; }
        public Post NewPost { get; set; } = new();
    }
}