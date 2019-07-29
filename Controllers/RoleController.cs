using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using UrnBMS.Models;

namespace UrnBMS.Controllers
{
    [Authorize(Roles = "Super-Admin")]
    public class RoleController : Controller
    {
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        public RoleController() { }

        public RoleController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            RoleManager = roleManager;
            UserManager = userManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().Get<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: ~/Role/
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }

        //
        // GET: ~/Role/New/
        public ActionResult New()
            => View();
		//
		// POST: ~/Role/New
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> New(RoleViewModels model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await RoleManager.CreateAsync(new ApplicationRole(model.Name));

				if (result.Succeeded)
				{
					TempData["message"] = $"The role: { model.Name } has successfully been added.";
					return RedirectToAction("index");
				}
				else
					AddErrorsFromResult(result);
            }
            return View(model);
        }

        // GET: ~/Role/Update/{id}
        public async Task<ActionResult> Update(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);

			IEnumerable<ApplicationUser> users;

			ApplicationDbContext db = ApplicationDbContext.Create();
			users = from u in db.Users where u.Roles.Any(r => r.RoleId == id) select u;

			ViewBag.roleId = role.Id;

			return View(users);
        }

        //POST: ~/roleAdmin/Update/{Id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRole(string id)
        {
            IdentityResult result;

            if (ModelState.IsValid)
            {
                ApplicationRole role = await RoleManager.FindByIdAsync(id);

                result = await RoleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
					TempData["message"] = $"The role: { role.Name } successfully updated.";
                    return RedirectToAction("index");
                }
                else
                    AddErrorsFromResult(result);
            }
            return View("error", new string[] { "Specified role not found." });
        }

		// POST: ~/Role/Delete/{id}
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);

            if (role != null)
            {
                IdentityResult result = await RoleManager.DeleteAsync(role);

				if (result.Succeeded)
				{
					TempData["message"] = $"The role: { role.Name } successfully removed.";
					return RedirectToAction("index");
				}
				else
					return View("error", result.Errors);
            }
            else
                return View("error", new string[] { "Specified role not found." });
        }

		// GET: ~/Role/AddUser/{id}
		public async Task<ActionResult> AddUser(string id)
		{
			ApplicationRole role = await RoleManager.FindByNameAsync(id);

			return View(role);
		}

		// POST: ~/Role/AddUser/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddUser(string id, RoleUpdateViewModels model)
		{
			ApplicationRole role = await RoleManager.FindByIdAsync(id);
			ApplicationUser user = await UserManager.FindByNameAsync(model.EmployeeNumber);
			
			IdentityResult result = await UserManager.AddToRoleAsync(user.Id, role.Name);

			if (result.Succeeded)
			{
				TempData["message"] = $"User { user.UserName } successfully added to the { role.Name } role.";
				return RedirectToAction("update", "role", new { id = role.Id });
			}
			else
				AddErrorsFromResult(result);

			return View(model);
		}

		// POST: ~/Role/Remove/{id}
		[HttpPost]
		public async Task<ActionResult> Remove(string id, string roleId)
		{
			ApplicationRole role = await RoleManager.FindByIdAsync(roleId);

			IdentityResult result = await UserManager.RemoveFromRoleAsync(id, role.Name);

			if (result.Succeeded)
			{
				TempData["message"] = $"User successfully removed from role: { role.Name }";
				return RedirectToAction("update", "role", new { id = role.Id });
			}
			else AddErrorsFromResult(result);

			return RedirectToAction("update", new { id = role.Id });
		}

        /// <summary>
        /// Lists all errors from the IdentityErrors.Errors property and list them
        /// on the ModelState.
        /// </summary>
        /// <param name="result"></param>
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
                ModelState.AddModelError("", error);
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				using (ApplicationDbContext _db = ApplicationDbContext.Create())
					_db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
