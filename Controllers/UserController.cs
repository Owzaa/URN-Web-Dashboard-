using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using UrnBMS.Models;

namespace UrnBMS.Controllers
{
    [Authorize(Roles = "Manager, Super-Admin, Admin")]
    public class UserController : Controller
	{
		private ApplicationUserManager _userManager;

		public UserController() { }

		public UserController(ApplicationUserManager userManager)
		{
			UserManager = userManager;
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().Get<ApplicationUserManager>();
			}
			set
			{
				_userManager = value;
			}
		}
		//
		// GET: ~/User/
		public ActionResult Index()
		{
			return View(UserManager.Users);
		}
		//
		// GET: ~/User/New/
		public ActionResult New()
			=> View();
		//
		// POST: ~/User/New/
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> New(UserViewModel model)
		{
			if (ModelState.IsValid)
			{
				ApplicationUser user = new ApplicationUser
				{
					UserName = model.EmployeeNumber,
					Email = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName,
				};

				IdentityResult result = await UserManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					string callbackUrl = Url.Action("confirmemail", "account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
					await UserManager.SendEmailAsync(user.Id,
						"Confirm your account", "Please confirm your BMS account by clicking <a href=\""
						+ callbackUrl + "\">here</a>");

					TempData["messgae"] = "User added successfully.";
					return RedirectToAction("index");
				}
				else
					AddErrorsFromResult(result);
			}
			return View(model);
		}
		//
		// GET: ~/User/Update/{id}
		public async Task<ActionResult> Update(string id)
		{
			ApplicationUser user = await UserManager.FindByIdAsync(id);

			UpdateUserViewModel viewModel = new UpdateUserViewModel
			{
				Email = user.Email,
				LastName = user.LastName
			};

			if (user != null)
				return View(viewModel);
			else
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
		}
		//
		// POST: ~/User/Update/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Update(string id, UpdateUserViewModel model)
		{
			ApplicationUser user = await UserManager.FindByIdAsync(id);

			if (user != null)
			{
				user.Email = model.Email;
				user.LastName = model.LastName;

				var result = await UserManager.UpdateAsync(user);

				if (result.Succeeded)
				{
					TempData["message"] = $"User { user.UserName } updated successfully";
					return RedirectToAction("index");
				}

			}
			else
				ModelState.AddModelError("", "Specified user not found.");

			return View(model);
		}
		//
		// POST: ~/User/Delete/{id}
		public async Task<ActionResult> Delete(string id)
		{
			ApplicationUser user = await UserManager.FindByIdAsync(id);

			if (user != null)
			{
				var result = await UserManager.DeleteAsync(user);

				if (result.Succeeded)
				{
					TempData["message"] = $"User { user.UserName } deleted successfully";
					return RedirectToAction("index");
				}
				else
					return View("error", result.Errors);
			}
			else
				return View("error", new string[] { "The specified user does not exist." });
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
	}
}
