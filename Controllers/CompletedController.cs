using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using UrnBMS.Models;
using PagedList;

namespace UrnBMS.Controllers
{
	public class CompletedController : Controller
	{
		// GET: ~/Completed/?q={string}
		public async Task<ActionResult> Index(string q, int? page)
		{
			List<UrnForm> completed;

			using (UrnDbContext db = new UrnDbContext())
			{
				if (!string.IsNullOrEmpty(q))
				{
					completed = await db.UrnForms.Where(f => f.Status == "Completed" && f.To.Contains(q) || f.From.Contains(q)
					|| f.RrNumber.ToString().Contains(q) || f.DateEntered.ToString().Contains(q) || f.AssyName.Contains(q) || f.Quantity.ToString().Contains(q)
					|| f.Priority.Contains(q) || f.Urgency.Contains(q) || f.AssyNumber.ToString().Contains(q)).OrderByDescending(f => f.TimeCompleted).ToListAsync();
				}
				else
					completed = await db.UrnForms.Where(f => f.Status == "Completed").OrderByDescending(wo => wo.TimeCompleted).ToListAsync();
			}

			int pageNumber = (page ?? 1);
			int pageSize = 10;

			return View(completed.ToPagedList(pageNumber, pageSize));
		}

		// POST: ~/Completed/Undo/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Undo(int? id)
		{
			if (id == null)
				return View("error", new string[] { "HTTP Error 400. Bad Request, please check the url and try again." });

			UrnForm urnForm;

			using (UrnDbContext db = new UrnDbContext())
			{
				urnForm = await db.UrnForms.FirstOrDefaultAsync(urn => urn.Id == id);
				
				if (TryUpdateModel(urnForm))
				{
					urnForm.Status = "Incomplete";
					urnForm.ReasonForIncompletion = Request["ReasonForIncompletion"];
					db.Entry(urnForm).State = EntityState.Modified;
					await db.SaveChangesAsync();
				}
			}

			TempData["error"] = $"Work order { urnForm.RrNumber } has been marked as not complete.";

			return RedirectToAction("index", "home", null);
		}

		// POST: ~/Completed/Ship/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Ship(int? id)
		{
			if (id == null)
				return View("error", new string[] { "HTTP Error 400. Bad Request, please check the url and try again." });

			UrnForm urnForm;

			using (UrnDbContext db = new UrnDbContext())
			{
				urnForm = await db.UrnForms.FirstOrDefaultAsync(urn => urn.Id == id);

				if (TryUpdateModel(urnForm))
				{
					urnForm.Status = "Shipped";
					urnForm.TimeShipped = DateTime.Now;
					urnForm.ShippedBy = HttpContext.User.Identity.Name;
					urnForm.ReasonForIncompletion = "";

					if (Request["issueResolution"] != string.Empty)
					{
						urnForm.ResolutionStatus = Request["issueResolution"];
					}

					db.Entry(urnForm).State = EntityState.Modified;
				}

				await db.SaveChangesAsync();
			}

			TempData["message"] = $"Work order { urnForm.RrNumber } has successfully been shipped.";

			return RedirectToAction("index", "completed", null);
		}

		// POST: ~/Completed/Export/
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Export()
		{
			List<UrnForm> urnForms;

			using (UrnDbContext db = new UrnDbContext())
			{
				urnForms = await db.UrnForms.AsNoTracking().Where(urn => urn.Status == "Completed")
					.OrderByDescending(urn => urn.TimeCompleted).ToListAsync();
			}

			DataTable table = new DataTable("Completed Work Orders");

			table.Columns.AddRange(new DataColumn[28]
			{
				new DataColumn("To"),
				new DataColumn("From"),
				new DataColumn("Branch"),
				new DataColumn("Customer"),
				new DataColumn("CSR Name"),
				new DataColumn("Assy Name"),
				new DataColumn("Assy Number"),
				new DataColumn("Date Entered"),
				new DataColumn("Time Completed"),
				new DataColumn("Authority Name"),
				new DataColumn("Model Name"),
				new DataColumn("Description"),
				new DataColumn("Priority"),
				new DataColumn("Quantity"),
				new DataColumn("RR Number"),
				new DataColumn("System Downtime"),
				new DataColumn("Critical Time"),
				new DataColumn("Urgency"),
				new DataColumn("Overtime Required"),
				new DataColumn("Stock"),
				new DataColumn("Replacement"),
				new DataColumn("Qty In RWC"),
				new DataColumn("Comments"),
				new DataColumn("Status"),
				new DataColumn("Employee Number"),
				new DataColumn("Submitted By"),
				new DataColumn("RA Number"),
				new DataColumn("Serial Number")
			});

			foreach (UrnForm workOrder in urnForms)
			{
				table.Rows.Add(
					workOrder.To,
					workOrder.From,
					workOrder.Branch,
					workOrder.Customer,
					workOrder.CsrName,
					workOrder.AssyName,
					workOrder.AssyNumber,
					workOrder.DateEntered,
					workOrder.TimeCompleted,
					workOrder.AuthorityName,
					workOrder.ModelName,
					workOrder.Description,
					workOrder.Priority,
					workOrder.Quantity,
					workOrder.RrNumber,
					workOrder.SystemDowntime,
					workOrder.CriticalTime,
					workOrder.Urgency,
					workOrder.OvertimeRequired,
					workOrder.Stock,
					workOrder.Replacement,
					workOrder.QtyInRwc,
					workOrder.Comments,
					workOrder.Status,
					workOrder.CompletedBy,
					workOrder.SubmittedBy,
					workOrder.RaNumber,
					workOrder.SerialNumber
				);
			}

			using (XLWorkbook workBook = new XLWorkbook())
			{
				workBook.Worksheets.Add(table);
				workBook.Author = HttpContext.User.Identity.Name;

				using (MemoryStream stream = new MemoryStream())
				{
					workBook.SaveAs(stream);
					return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Completed { DateTime.Today }.xlsx");
				}
			}
		}
	}
}
