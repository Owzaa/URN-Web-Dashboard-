using System.Data;
using System.Data.Entity;

namespace UrnBMS.Models
{
    public class UrnDbContext : DbContext
    {
        public UrnDbContext() : base("BmsWorkOrderDB") { }

        public DbSet<UrnForm> UrnForms { get; set; }
    }
}
