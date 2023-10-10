using UserManagement.Model;

namespace UserManagement.Service
{
    public class SortState
    {
        public ApplicationDdContext dbContext { get; set; }
        public enum SortStates
        {
            NameAsc,   
            NameDesc,   
            AgeAsc, 
            AgeDesc,   
            EmailAsc, 
            EmailDesc 
        }

        public SortStates start() 
        {
            return SortStates.NameAsc; 
        }
    }
}
