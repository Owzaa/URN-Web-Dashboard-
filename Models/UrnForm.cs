using System;
using System.ComponentModel.DataAnnotations;

namespace UrnBMS.Models
{
	public class UrnForm
	{
		//Initialize Pertinent Fields
		public UrnForm()
		{
			DateEntered = DateTime.Now;
			TimeCompleted = DateTime.Now;
			TimeShipped = DateTime.Now;
			BackorderDate = DateTime.Now;
			Status = "Queued";
			DocumentName = "Urgent Repair Notice";
			FormNumber = "FORM0011";
			Department = "Managed Services Operations";
			Revision = "06";
			Index = "IMC";
		}

		[ScaffoldColumn(false)]
		public long Id { get; set; }

		// Pertinent document info
		[ScaffoldColumn(false)]
		public string DocumentName { get; set; }

		[ScaffoldColumn(false)]
		public string FormNumber { get; set; }

		[ScaffoldColumn(false)]
		public string Department { get; set; }

		[ScaffoldColumn(false)]
		public string Revision { get; set; }

		[ScaffoldColumn(false)]
		public string Index { get; set; }

		// Form fields to be manually filled in.
		[Required(ErrorMessage = "Please fill in the To field.")]
		public string To { get; set; }

		[Required(ErrorMessage = "Please fill in the from field.")]
		[Display(Name = "From")]
		public string From { get; set; }

		[Required(ErrorMessage = "Please fill in the branch.")]
		[Display(Name = "Branch")]
		public string Branch { get; set; }

		[Required(ErrorMessage = "Please fill in the Customer.")]
		public string Customer { get; set; }

		[Required(ErrorMessage = "Please fill in the CSRName.")]
		public string CsrName { get; set; }

		[Required(ErrorMessage = "Please fill in the AssyName.")]
		public string AssyName { get; set; }

		[Required(ErrorMessage = "Please fill in the AssyNumber")]
		public string AssyNumber { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Date Completed")]
		public DateTime DateEntered { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Time Completed")]
		public DateTime TimeCompleted { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Time Shipped")]
		public DateTime TimeShipped { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Backorder Date")]
		public DateTime BackorderDate { get; set; }

		[Required]
		[Display(Name = "Authority Name")]
		public string AuthorityName { get; set; }

		[Required]
		[Display(Name = "Model Name")]
		public string ModelName { get; set; }

		[Required]
		[Display(Name = "Authority Name")]
		public string Description { get; set; }

		[Required]
		public string Priority { get; set; }

		[Required]
		public int Quantity { get; set; }

		[Required]
		[Display(Name = "RR Number")]
		public string RrNumber { get; set; }

		[Required]
		[Display(Name = "System Down Time ")]
		public string SystemDowntime { get; set; }

		[Required]
		[Display(Name = "Critical Time")]
		public string CriticalTime { get; set; }

		[Required]
		[Display(Name = "Urgency")]
		public string Urgency { get; set; }

		[Required]
		[Display(Name = "Overtime Required")]
		public bool OvertimeRequired { get; set; }

		[Required]
		public bool Stock { get; set; }

		[Required]
		public string Replacement { get; set; }

		[Required]
		[Display(Name = "Quantity in Rework Center")]
		public int QtyInRwc { get; set; }

		[Required]
		[DataType(DataType.MultilineText)]
		public string Comments { get; set; }

		public string Status { get; set; }

		[Display(Name = "Employee Number")]
		public string CompletedBy { get; set; }

		[Display(Name = "Submitted By")]
		public string SubmittedBy { get; set; }

		[Display(Name = "Shipped By")]
		public string ShippedBy { get; set; }

		[Display(Name = "Reason for Incompletion")]
		public string ReasonForIncompletion { get; set; }

		[Display(Name = "Resolution Status")]
		public string ResolutionStatus { get; set; }

		[Display(Name = "RA Number")]
		public string RaNumber { get; set; }

		[Display(Name = "Serial Number")]
		public string SerialNumber { get; set; }

		[Display(Name = "Reason For Backorder")]
		public string ReasonForBackorder { get; set; }

		[Required]
		[Display(Name = "All Equivalent Digits Checked ")]
		public bool AllRelevantNumbersChecked { get; set; }

		[Required]
		[Display(Name = "Store Rack checked")]
		public bool StoreRackChecked { get; set; }

		[Required]
		[Display(Name = "Store Trolley checked")]
		public bool StoreTrolleyChecked { get; set; }

		[Required]
		[Display(Name = "No unpacked stock")]
		public bool NoUnpackedStock { get; set; }

		[Required]
		[Display(Name = "RW trolley checked")]
		public bool ReworkTrolleyChecked { get; set; }
	}
}
