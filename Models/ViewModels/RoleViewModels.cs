using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UrnBMS.Models
{
	public class RoleViewModels
	{
		[Required]
		public string Name { get; set; }
	}

	public class RoleUpdateViewModels
	{
		[Required]
		[Display(Name = "Employee Number")]
		public string EmployeeNumber { get; set; }
	}
}
