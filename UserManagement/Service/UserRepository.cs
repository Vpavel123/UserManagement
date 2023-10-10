using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using System.Linq.Dynamic.Core;
using UserManagement.Model;

namespace UserManagement.Service
{
    public interface IUserRepository 
    {
        Task<PageUserResult> GetUsers(string? term, string? sort, int page, int limit);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDdContext dbContext;

        public UserRepository(ApplicationDdContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PageUserResult> GetUsers(string? term, string? sort, int page, int limit)
        {
            IQueryable<User> users;
            if (string.IsNullOrWhiteSpace(term))
                users = dbContext.user;
            else
            {
                term = term.Trim().ToLower();
                users = dbContext.user
                    .Where(b => b.Name.ToLower().Contains(term)
                    || b.Email.ToLower().Contains(term) || b.Age.ToString().ToLower().Contains(term)
                    );
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                var sortFields = sort.Split(','); 
                StringBuilder orderQueryBuilder = new StringBuilder();

                PropertyInfo[] propertyInfo = typeof(User).GetProperties();


                foreach (var field in sortFields)
                {

                    string sortOrder = "ascending";

                    var sortField = field.Trim();
                    if (sortField.StartsWith("-"))
                    {
                        sortField = sortField.TrimStart('-');
                        sortOrder = "descending";
                    }

                    var property = propertyInfo.FirstOrDefault(a => a.Name.Equals(sortField, StringComparison.OrdinalIgnoreCase));
                    if (property == null)
                        continue;

                    orderQueryBuilder.Append($"{property.Name.ToString()} {sortOrder}, ");
                }

                string orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
                if (!string.IsNullOrWhiteSpace(orderQuery))
        
                    users = users.OrderBy(orderQuery);
                else
                    users = users.OrderBy(a => a.Id);
            }


            var totalCount = await dbContext.user.CountAsync();  
            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            var pagedUsers = await users.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var pagedUserData = new PageUserResult
            {
                user = pagedUsers,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
            return pagedUserData;
        }
    }
}
