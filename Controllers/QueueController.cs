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
	public class QueueController : Controller
	{
		// GET: ~/Queue/?{q}
		public async Task<ActionResult> Index(string q, int? page)
		{
			List<UrnForm> queue;

			using (UrnDbContext db = new UrnDbContext())
			{
				if (!string.IsNullOrEmpty(q))
				{
					queue = await db.UrnForms.Where(f => f.Status == "Queued" && f.To.Contains(q) || f.From.Contains(q)
					|| f.RrNumber.ToString().Contains(q) || f.DateEntered.ToString().Contains(q) || f.AssyName.Contains(q) || f.Quantity.ToString().Contains(q)
					|| f.Priority.Contains(q) || f.Urgency.Contains(q) || f.AssyNumber.ToString().Contains(q)).OrderByDescending(f => f.Id).ToListAsync();
				}
				else
					queue = await db.UrnForms.Where(f => f.Status == "Queued").OrderByDescending(wo => wo.TimeShipped).ToListAsync();
			}

			int pageSize = 10;
			int pageNumber = (page ?? 1);

			return View(queue.ToPagedList(pageNumber, pageSize));
		}

		// POST: ~/Queue/Complete/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Complete(int? id)
		{
			if (id == null)
				return View("error", new string[] { "HTTP Error 400. Bad Request, please check the url and try again." });

			UrnForm urnForm;

			using (UrnDbContext db = new UrnDbContext())
			{
				urnForm = await db.UrnForms.FirstOrDefaultAsync(urn => urn.Id == id);

				if (TryUpdateModel(urnForm))
				{
					urnForm.Status = "Completed";
					urnForm.TimeCompleted = DateTime.Now;

					if (Request["raNumber"] != string.Empty || Request["serial"] != string.Empty)
					{
						urnForm.RaNumber = Request["raNumber"];
						urnForm.SerialNumber = Request["serial"];
					}

					urnForm.CompletedBy = HttpContext.User.Identity.Name;
					db.Entry(urnForm).State = EntityState.Modified;
				}

				TempData["message"] = $"Work order { urnForm.RrNumber } has been successfully completed.";
				await db.SaveChangesAsync();
			}

			return RedirectToAction("index", "home", null);
		}

		// POST: ~/Queue/Backorder/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Backorder(int? id)
		{
			if (id == null)
				return View("error", new string[] { "HTTP Error 400. Bad Request, please check the url and try again." });

			UrnForm urn;

			using (UrnDbContext db = new UrnDbContext())
			{
				urn = await db.UrnForms.FirstOrDefaultAsync(u => u.Id == id);

				if (TryUpdateModel(urn))
				{
					urn.Status = "Backorder";
					urn.ReasonForBackorder = Request["ReasonForBackorder"];
					urn.BackorderDate = DateTime.Now;
					db.Entry(urn).State = EntityState.Modified;
				}
				TempData["message"] = $"Work order { urn.RrNumber } successfully sent to backorder";
				await db.SaveChangesAsync();
			}

			return RedirectToAction("index", "home", null);
		}

		// POST: ~/Queue/Export/
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Export()
		{
			List<UrnForm> workOrders;

			using (UrnDbContext db = new UrnDbContext())
			{
				workOrders = await db.UrnForms.AsNoTracking().Where(urn => urn.Status == "Queued")
					.OrderByDescending(urn => urn.DateEntered).ToListAsync();
			}

			DataTable table = new DataTable($"Queue { DateTime.Today.ToShortDateString() }");

			table.Columns.AddRange(new DataColumn[24]
			{
				new DataColumn("To"),
				new DataColumn("From"),
				new DataColumn("Branch"),
				new DataColumn("Customer"),
				new DataColumn("CSR Name"),
				new DataColumn("Assy Name"),
				new DataColumn("Assy Number"),
				new DataColumn("Date Entered"),
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
				new DataColumn("Qty in RWC"),
				new DataColumn("Comments"),
				new DataColumn("Status"),
				new DataColumn("Submitted By")
			});

			foreach (UrnForm workOrder in workOrders)
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
					workOrder.SubmittedBy
				);
			}

			using (XLWorkbook workbook = new XLWorkbook())
			{
				workbook.Worksheets.Add(table);
				workbook.Author = HttpContext.User.Identity.Name;

				using (MemoryStream stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Queue { DateTime.Today }.xlsx");
				}
			}
		}
	}
}
