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
        public virtual DbSet<CurrentPressCounter> CurrentPressCounters { get; set; }
        public virtual DbSet<Rack> Racks { get; set; }
        public virtual DbSet<Profil> Profils { get; set; }
        public virtual DbSet<RackProfil> RackProfils { get; set; }
        public virtual DbSet<RackCell> RackCells { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RackProfil>()
                .HasKey(mp => new { mp.RackId, mp.ProfilId });

            modelBuilder.Entity<RackProfil>()
                .HasOne(mp => mp.Rack)
                .WithMany(p => p.RackProfils)
                .HasForeignKey(mp => mp.RackId);

            modelBuilder.Entity<RackProfil>()
                .HasOne(mp => mp.Profil)
                .WithMany(m => m.RackProfils)
                .HasForeignKey(mp => mp.ProfilId);

            modelBuilder.Entity<RackCell>()
                .HasKey(mp => new { mp.RackId, mp.CellId });

            modelBuilder.Entity<RackCell>()
                .HasOne(mp => mp.Rack)
                .WithMany(p => p.RackCells)
                .HasForeignKey(mp => mp.RackId);

            modelBuilder.Entity<RackCell>()
                .HasOne(mp => mp.Cell)
                .WithMany(m => m.RackCells)
                .HasForeignKey(mp => mp.CellId);
        }

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
                else
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
