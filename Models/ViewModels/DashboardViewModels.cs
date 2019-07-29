namespace UrnBMS.Models
{
	public class DashboardViewModels
	{
		public int CompleteWorkOrders { get; set; }
		public int ShippedWorkOrders { get; set; }
		public int TotalWorkOrders { get; set; }
		public int AllQueuedWorkOrders { get; set; }
		public int AllCompletedWorkOrders { get; set; }
		public int AllBackOrders { get; set; }
		public int AllIncompleteWorkOrders { get; set; }
		public int AllShippedWorkOrders { get; set; }
	}
}
