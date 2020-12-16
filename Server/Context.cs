using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server
{
    public class Context : DbContext
    {
        public static bool TestServer = true;

        public Context()
        {
#if DEBUG
            TestServer = true;
#endif
        }

        public DbSet<Models.Account> Account { get; set; }
        public DbSet<Models.Character> Character { get; set; }

        public DbSet<BankAccount> BankAccount { get; set; }

        public DbSet<Dealership> Dealership { get; set; }
        public DbSet<Models.Vehicle> Vehicle { get; set; }

        public DbSet<InventoryData> Inventory { get; set; }
        public DbSet<Models.Property> Property { get; set; }

        public DbSet<Models.Teleport> Teleport { get; set; }

        public DbSet<Faction> Faction { get; set; }

        public DbSet<BusRoute> BusRoutes { get; set; }

        public DbSet<ApartmentComplexes> ApartmentComplexes { get; set; }
        public DbSet<Phones> Phones { get; set; }
        public DbSet<AdminRecord> AdminRecords { get; set; }

        public DbSet<Bans> Bans { get; set; }

        public DbSet<DeliveryPoint> DeliveryPoint { get; set; }

        public DbSet<Warehouse> Warehouse { get; set; }

        public DbSet<Models.Graffiti> Graffiti { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<FishingPoint> FishingPoints { get; set; }

        public DbSet<Models.Motel> Motels { get; set; }

        public DbSet<Models.Garage> Garages { get; set; }

        public DbSet<Models.Clerk> Clerks { get; set; }

        public DbSet<Models.Backpack> Backpacks { get; set; }

        public DbSet<Scene> Scenes { get; set; }

        public DbSet<Door> Doors { get; set; }

        public DbSet<Storage> Storages { get; set; }

        public DbSet<Marijuana> Marijuana { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG

            optionsBuilder.UseMySql($"server=localhost;database={Release.Default.MySqlDebug};user={Release.Default.MySqlUser};password={Release.Default.MySqlPass};SslMode=none;Convert Zero Datetime=true;");

#endif

#if RELEASE

            optionsBuilder.UseMySql($"server=localhost;database={Release.Default.MySqlDb};user={Release.Default.MySqlUser};password={Release.Default.MySqlPass};SslMode=none;Convert Zero Datetime=true;");
#endif
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }
    }
}