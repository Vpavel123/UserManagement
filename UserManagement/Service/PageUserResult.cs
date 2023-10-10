using UserManagement.Model;

namespace UserManagement.Service
{
    public class PageUserResult
    {
        public IEnumerable<User> user { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
