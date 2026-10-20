using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Data
{
    public class AsmDatabaseContext : DbContext
    {
        public DbSet<PackagingUnit> PackagingUnits { get; set; }

       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=SRV-SQL;Database=SiplaceSetupCenter;Integrated Security=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


        }
    }
}
