using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using UrnBMS.Models;

namespace UrnBMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
	{
		// GET: ~/
		public async Task<ActionResult> Index()
		{
			DashboardViewModels dashboard = new DashboardViewModels();

			using (UrnDbContext db = new UrnDbContext())
			{
				dashboard.CompleteWorkOrders =
					await db.UrnForms.Where(urn => urn.Status == "Completed" && urn.CompletedBy == HttpContext.User.Identity.Name).
					AsNoTracking().CountAsync();

				dashboard.ShippedWorkOrders =
					await db.UrnForms.Where(urn => urn.Status == "Shipped" && urn.CompletedBy == HttpContext.User.Identity.Name)
					.AsNoTracking().CountAsync();

				dashboard.TotalWorkOrders = await db.UrnForms.AsNoTracking().CountAsync();

				dashboard.AllQueuedWorkOrders = await db.UrnForms.Where(urn => urn.Status == "Queued")
					.AsNoTracking().CountAsync();

				dashboard.AllCompletedWorkOrders = await db.UrnForms.Where(urn => urn.Status == "Completed")
					.AsNoTracking().CountAsync();

				dashboard.AllIncompleteWorkOrders = await db.UrnForms.Where(urn => urn.Status == "Incomplete")
					.AsNoTracking().CountAsync();

				dashboard.AllShippedWorkOrders = await db.UrnForms.Where(urn => urn.Status == "Shipped")
					.AsNoTracking().CountAsync();

				dashboard.AllBackOrders = await db.UrnForms.Where(urn => urn.Status == "Backorder")
					.AsNoTracking().CountAsync();
			}

			return View(dashboard);
		}

		// GET: ~/home/urnform/
		[Authorize(Roles = "Super-Admin, Super-User, Admin, Manager")]
		public ActionResult UrnForm()
			=> View();

		// POST: ~/home/urnform/
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Super-Admin, Super-User, Admin, Manager")]
		public async Task<ActionResult> UrnForm(UrnForm form)
		{
			using (UrnDbContext db = new UrnDbContext())
			{
				// TODO: Enforce form data validity.
				if (ModelState.IsValid)
				{
					form.SubmittedBy = HttpContext.User.Identity.Name;

					if (form.AllRelevantNumbersChecked == true && form.StoreRackChecked == true && form.StoreTrolleyChecked == true
						&& form.NoUnpackedStock == true && form.ReworkTrolleyChecked == true)
					{
						db.UrnForms.Add(form);
						await db.SaveChangesAsync();

						TempData["message"] = "Work order successfully placed in queue.";
						return RedirectToAction("index", "home");
					}
					else
						ModelState.AddModelError
							("", "Please ensure that all numbers, racks & trolleys are in check and that there's no unpacked stock.");
				}

				return View(form);
			}
		}

		// GET: ~/Home/Info/{id}
		public async Task<ActionResult> Info(int? id)
		{
			if (id == null)
				return View("error",
					new string[] { "HTTP Error 400. Bad Request, please check the url and try again." });

			UrnForm urn;

			using (UrnDbContext db = new UrnDbContext())
				urn = await db.UrnForms.FirstOrDefaultAsync(u => u.Id == id);

			if (urn == null)
				return View("error",
					new string[] { "404: Not Found. The entity requested is not found. Please check the url and try again." });

			return View(urn);
		}

		public async Task<ActionResult> UpdateWorkOrder(int? id)
		{
			if (id == null)
				return HttpNotFound();

			UrnForm workOrder;

			using (UrnDbContext _db = new UrnDbContext())
			{
				workOrder = await _db.UrnForms.FirstOrDefaultAsync(wo => wo.Id == id);
			}

			return View(workOrder);
		}

		[HttpPost]
		[ActionName("UpdateWorkOrder")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> UpdateWo(int? id)
		{
			UrnForm workOrder;

			using (UrnDbContext _db = new UrnDbContext())
			{
				workOrder = await _db.UrnForms.FirstOrDefaultAsync(wo => wo.Id == id);

				if (ModelState.IsValid)
				{
					try
					{
						if (TryUpdateModel(workOrder))
						{
							_db.Entry(workOrder).State = EntityState.Modified;
							await _db.SaveChangesAsync();
							TempData["message"] = $"Work order { workOrder.RrNumber } successfully updated.";
							return RedirectToAction("index", "queue", null);
						}
					}
					catch (DataException ex)
					{
						ModelState.AddModelError("", ex.Message);
					}
				}
			}
			return View(workOrder);
		}
	}
}
