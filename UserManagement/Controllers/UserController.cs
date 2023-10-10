using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UserManagement.Model;
using UserManagement.Service;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        public readonly ApplicationDdContext dbContext;
        private readonly IUserRepository userRepository;
        private readonly UserValidator userValidator;
        private readonly ILogger<UserController> logger;

        public UserController(ILogger<UserController> logger, ApplicationDdContext dbContext, IUserRepository repository, UserValidator userValidator)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.userRepository = repository;
            this.userValidator = userValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get(string? term, string? sort, int page = 1, int limit = 5)
        {
            if (dbContext.user == null)
            {
                return NotFound();
            }

            //var users = await dbContext.user.Join(dbContext.role, 
            //    u => u.roleId,
            //    c => c.id,
            //    (u, c) => new 
            //      {
            //       Name = u.Name,
            //       User = c.user,
            //        Admin = c.admin,
            //        Support = c.support,
            //        Superadmin = c.superadmin,
            //        Age = u.Age
            //    }).ToListAsync();

            //var users = from u in dbContext.user
            //            join c in dbContext.role on u.roleId equals c.id
            //            select new { ID = u.Id, Name = u.Name, roleId = c.id, Age = u.Age, Email = u.Email, User = c.user, Admin = c.admin, Support = c.support, Superadmin = c.superadmin };

            var users = await userRepository.GetUsers(term, sort, page, limit);
            Response.Headers.Add("X-Total-Count", users.TotalCount.ToString());
            Response.Headers.Add("X-Total-Pages", users.TotalPages.ToString());

            return Ok(users.user);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<User>>> Get(int? id)
        {
            if (dbContext.user == null)
            {
                return NotFound();
            }

            var _user = from u in dbContext.user
                        join c in dbContext.role on u.roleId equals c.id
                        select new { ID = u.Id, Name = u.Name, roleId = c.id, Age = u.Age, Email = u.Email, User = c.user, Admin = c.admin, Support = c.support, Superadmin = c.superadmin };
            var _userFirst = await _user.FirstOrDefaultAsync(x => x.ID == id);

            return Ok(_userFirst);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Post(string _name, int _age, string _email, bool user_role = true, bool admin_role = false, bool suppor_role = false, bool superadmin_role = false)
        {
            int newId = 1;
            bool idExists = true;

            while (idExists)
            {
                idExists = dbContext.role.Any(i => i.id == newId);

                if (idExists)
                {
                    newId++;
                }
            }
            Role role = new Role() { id = newId, user = user_role, admin = admin_role, support = suppor_role, superadmin = superadmin_role};
            if (role == null)
            {
                return BadRequest();
            }
            dbContext.role.Add(role);
            await dbContext.SaveChangesAsync();

            int newId2 = 1;
            idExists = true;
            while (idExists)
            {
                idExists = dbContext.role.Any(i => i.id == newId2);

                if (idExists)
                {
                    newId2++;
                }
            }

            User user = new User() { Id = newId2, Name = _name, Age = _age, Email = _email, roleId = newId };
            if (user == null)
            {
                return BadRequest();
            }

            var validationResult = userValidator.Validate(user);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

            if (user.Age <= 0)
                ModelState.AddModelError("Age", "Возраст не должен быть отрицательным");

            if (user.Name == "admin")
            {
                ModelState.AddModelError("Name", "Недопустимое имя пользователя - admin");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dbContext.user.Add(user);
            await dbContext.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> Put(int? id, string _name, int _age, string _email, bool user_role = true, bool admin_role = false, bool suppor_role = false, bool superadmin_role = false)
        {
            var _userFirst = await dbContext.user.FirstOrDefaultAsync(x => x.Id == id);
            _userFirst.Name = _name;
            _userFirst.Age = _age;
            _userFirst.Email = _email;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_userFirst.Age <= 0)
                ModelState.AddModelError("Age", "Возраст не должен быть отрицательным");

            if (_userFirst.Name == "admin")
            {
                ModelState.AddModelError("Name", "Недопустимое имя пользователя - admin");
            }

            var validationResult = userValidator.Validate(_userFirst);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

            if (_userFirst == null)
            {
                return BadRequest();
            }
            if (!dbContext.user.Any(x => x.Id == id))
            {
                return NotFound();
            }

            var _roleFirst = await dbContext.role.FirstOrDefaultAsync(x => x.id == _userFirst.roleId);
            _roleFirst.user = user_role;
            _roleFirst.admin = admin_role;
            _roleFirst.support = suppor_role;
            _roleFirst.superadmin = superadmin_role;
            dbContext.Update(_userFirst);
            dbContext.Update(_roleFirst);
            await dbContext.SaveChangesAsync();
            return Ok(_userFirst);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<User>>> DeleteUser(int? id)
        {
            if (dbContext.user == null)
            {
                return NotFound();
            }

            var _user = await dbContext.user.FindAsync(id);
            if (_user == null)
            {
                return NotFound();
            }

            dbContext.role.Remove(dbContext.role.Find(_user.roleId));
            dbContext.user.Remove(_user);
            return await dbContext.user.ToListAsync();
        }
    }
}
