using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using KANTAIM.DAL.Model;

namespace KANTAIM.DAL
{
    public partial class DataKANTAIMContext : DbContext
    {
        static string? cs;

        public virtual DbSet<Shift> Shifts { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAccessLvl> UserAccessLvls { get; set; }
        public virtual DbSet<Workshop> Workshops { get; set; }
        public virtual DbSet<ProductFamily> ProductFamilies { get; set; }
        public virtual DbSet<ContainerAction> ContainerActions { get; set; }
        public virtual DbSet<Cell> Cells { get; set; }
        public virtual DbSet<CellProduct> CellProducts { get; set; }
        public virtual DbSet<ColorProduct> ColorProducts { get; set; }
        public virtual DbSet<ProdColor> Colors { get; set; }
        public virtual DbSet<Container> Containers { get; set; }
        public virtual DbSet<ContainerType> ContainerTypes { get; set; }
        public virtual DbSet<DataProd> DataProds { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Machine> Machines { get; set; }
        public virtual DbSet<Press> Press { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Shape> Shapes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (cs == null)
                {
                    IConfigurationRoot config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();

                    cs = config["connectionStrings:DataKANTAIMContext"];
                }

                optionsBuilder.UseSqlServer(cs);
                #if DEBUG
                //optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=DATASCADAMOULAGE;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
                #endif

            }
        }
    }
}
