using Microsoft.EntityFrameworkCore;
using ChineseSaleApi.Models;

namespace ChineseSaleApi.Data
{
    public class ChineseSaleContext : DbContext
    {
        public ChineseSaleContext(DbContextOptions<ChineseSaleContext> options) : base(options) { }

        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Package> Packages { get; set; } = null!;
        public DbSet<Gift> Gifts { get; set; } = null!;
        public DbSet<Card> Cards { get; set; } = null!;
        public DbSet<CardCart> CardCarts { get; set; } = null!;
        public DbSet<PackageCart> PackageCarts { get; set; } = null!;
        public DbSet<Lottery> Lotteries { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Donor> Donors { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - unique indices
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // User <-> Address (one-to-one)
            // NOTE: models currently contain FK properties on both sides (Address.UserId and User.AddressId).
            // We'll configure User as the dependent entity using User.AddressId as the FK.
            modelBuilder.Entity<User>()
                .HasOne(u => u.Address)
                .WithOne(a => a.User)
                .HasForeignKey<User>(u => u.AddressId)
                .OnDelete(DeleteBehavior.Cascade);

            // Donor <-> Address (one-to-one) via Donor.CompanyAddressId
            modelBuilder.Entity<Donor>()
                .HasOne(d => d.CompanyAddress)
                .WithOne(a => a.Donor)
                .HasForeignKey<Donor>(d => d.CompanyAddressId)
                .OnDelete(DeleteBehavior.Cascade);

            // Package -> Lottery (many-to-one)
            modelBuilder.Entity<Package>()
                .HasOne(p => p.Lottery)
                .WithMany(l => l.Packages)
                .HasForeignKey(p => p.LotteryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Gift relationships
            modelBuilder.Entity<Gift>()
                .HasOne(g => g.Donor)
                .WithMany(d => d.Gifts)
                .HasForeignKey(g => g.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Gift>()
                .HasOne(g => g.Category)
                .WithMany(c => c.Gifts)
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Gift>()
                .HasOne(g => g.Lottery)
                .WithMany(l => l.Gifts)
                .HasForeignKey(g => g.LotteryId)
                .OnDelete(DeleteBehavior.Cascade);

            // CardCart relationships
            modelBuilder.Entity<CardCart>()
                .HasOne(cc => cc.User)
                .WithMany(u => u.CardCart)
                .HasForeignKey(cc => cc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CardCart>()
                .HasOne(cc => cc.Gift)
                .WithMany(g => g.CartCarts)
                .HasForeignKey(cc => cc.GiftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Card relationships
            modelBuilder.Entity<Card>()
                .HasOne(c => c.User)
                .WithMany(u => u.Cards)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Card>()
                .HasOne(c => c.Gift)
                .WithMany(g => g.Cards)
                .HasForeignKey(c => c.GiftId)
                .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<Donor>()
        //    .HasMany(d => d.Lotteries)
        //    .WithMany(l => l.Donors)
        //    .UsingEntity<Dictionary<string, object>>(
        //    "DonorLottery", // join table name
        //    j => j
        //        .HasOne<Lottery>()
        //        .WithMany()
        //        .HasForeignKey("LotteryId")
        //        .HasConstraintName("FK_DonorLottery_Lotteries_LotteryId")
        //        .OnDelete(DeleteBehavior.Cascade),
        //    j => j
        //        .HasOne<Donor>()
        //        .WithMany()
        //        .HasForeignKey("DonorId")
        //        .HasConstraintName("FK_DonorLottery_Donors_DonorId")
        //        .OnDelete(DeleteBehavior.Cascade),
        //    j =>
        //    {
        //        j.HasKey("DonorId", "LotteryId");
        //        j.ToTable("DonorLottery");
        //    });
        }
    }
}