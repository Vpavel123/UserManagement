using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace UserManagement.Model
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public bool user { get; set; }
        public bool admin { get; set; }
        public bool support { get; set; }
        public bool superadmin { get; set; }

        public virtual ICollection<User> _user { get; set; }

        public Role()
        {
            this._user = new List<User>();
        }
    }
}
