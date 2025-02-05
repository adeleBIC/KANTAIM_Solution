using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using KANTAIM.DAL.Model;
using System.Reflection;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

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
                //Console.WriteLine("Testing...");
                //bool isConnected = TestConnectionAsync().GetAwaiter().GetResult(); // Blocking for testing purposes
                //Console.WriteLine($"Database connection: {isConnected}");
                if (OperatingSystem.IsAndroid())
                {
                    optionsBuilder.UseSqlServer("Server=MONSSQL03;Database=DATASCADAMOULAGE;User Id=UserMLV;Password=BicUserMLV20;MultipleActiveResultSets=True;Encrypt=false;TrustServerCertificate=False;MultiSubnetFailover=True");
                    
                }
                else // Supposons que c'est Web
                {
                    IConfigurationRoot config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();
                    cs = config["connectionStrings:DataKANTAIMContext"];
                }

                if (cs != null)
                {
                    optionsBuilder.UseSqlServer(cs);
                }
#if DEBUG
                //optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=DATASCADAMOULAGE;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
                //optionsBuilder.UseSqlServer("Server=MONSSQL03;Database=DATASCADAMOULAGE;User Id=UserMLV;Password=BicUserMLV20;MultipleActiveResultSets=True;Encrypt=false;TrustServerCertificate=False;MultiSubnetFailover=True");
#endif

            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                Console.WriteLine("Testing database connection...");
                return await Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection failed: {ex.Message}");
                return false;
            }
        }
    }
}
