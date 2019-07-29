using ClosedXML.Excel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using UrnBMS.Models;

namespace UrnBMS.Controllers
{
	public class IncompleteController : Controller
	{
		// GET: ~/Incomplete/?q={string}
		public async Task<ActionResult> Index(string q, int? page)
		{
			List<UrnForm> incomplete;

			using (UrnDbContext db = new UrnDbContext())
			{
				if (!string.IsNullOrEmpty(q))
				{
					incomplete = await db.UrnForms.Where(f => f.Status == "Incomplete" && f.To.Contains(q) || f.From.Contains(q)
					|| f.RrNumber.ToString().Contains(q) || f.DateEntered.ToString().Contains(q) || f.AssyName.Contains(q) || f.Quantity.ToString().Contains(q)
					|| f.Priority.Contains(q) || f.Urgency.Contains(q) || f.AssyNumber.ToString().Contains(q)).OrderByDescending(f => f.Id).ToListAsync();
				}
				else
					incomplete = await db.UrnForms.Where(f => f.Status == "Incomplete").OrderByDescending(wo => wo.TimeCompleted).ToListAsync();
			}

			int pageNumber = (page ?? 1);
			int pageSize = 10;

			return View(incomplete.ToPagedList(pageNumber, pageSize));
		}

		// ~/Incomplete/Export/
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Export()
		{
			List<UrnForm> urnForms;

			using (UrnDbContext db = new UrnDbContext())
				urnForms = await db.UrnForms.Where(urn => urn.Status == "Incomplete")
					.OrderByDescending(urn => urn.DateEntered).AsNoTracking().ToListAsync();

			DataTable table = new DataTable("Incomplete Work Orders");

			table.Columns.AddRange(new DataColumn[27]
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
				new DataColumn("Qty In RWC"),
				new DataColumn("Comments"),
				new DataColumn("Status"),
				new DataColumn("Submitted By"),
				new DataColumn("Reason For Incompletion"),
				new DataColumn("RA Number"),
				new DataColumn("Serial Number")
			});

			foreach (UrnForm urnForm in urnForms)
			{
				table.Rows.Add(
					urnForm.To,
					urnForm.From,
					urnForm.Branch,
					urnForm.Customer,
					urnForm.CsrName,
					urnForm.AssyName,
					urnForm.AssyNumber,
					urnForm.DateEntered,
					urnForm.AuthorityName,
					urnForm.ModelName,
					urnForm.Description,
					urnForm.Priority,
					urnForm.Quantity,
					urnForm.RrNumber,
					urnForm.SystemDowntime,
					urnForm.CriticalTime,
					urnForm.Urgency,
					urnForm.OvertimeRequired,
					urnForm.Stock,
					urnForm.Replacement,
					urnForm.QtyInRwc,
					urnForm.Comments,
					urnForm.Status,
					urnForm.SubmittedBy,
					urnForm.ReasonForIncompletion,
					urnForm.RaNumber,
					urnForm.SerialNumber
				);
			}

			using (XLWorkbook workBook = new XLWorkbook())
			{
				workBook.Worksheets.Add(table);
				workBook.Author = HttpContext.User.Identity.Name;

				using (MemoryStream stream = new MemoryStream())
				{
					workBook.SaveAs(stream);
					return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Incomplete { DateTime.Today }.xlsx");
				}
			}
		}
	}
}
