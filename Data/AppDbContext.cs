using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockGTO.Models;

namespace StockGTO.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 既有表…
        public DbSet<Post> Posts { get; set; }
        public DbSet<ArticlePost> ArticlePosts { get; set; }
        public DbSet<IndexNews> IndexNews { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ArticleFavorite> ArticleFavorites { get; set; }
        public DbSet<DiyTicketType> DiyTicketTypes { get; set; }
        public DbSet<DiyBooking> DiyBookings { get; set; }

        // 新 O_Ticket 主檔 / DIY 明細
        public DbSet<O_Ticket_Booking> O_Ticket_Bookings { get; set; }
        public DbSet<O_Ticket_DiyBooking> O_Ticket_DiyBookings { get; set; }

        // 票種主檔 / 訂單票種明細（正規化）
        public DbSet<O_Ticket_TicketType> O_Ticket_TicketTypes { get; set; }
        public DbSet<O_Ticket_BookingTicket> O_Ticket_BookingTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── O_Ticket_Bookings ──────────────────────────────
            modelBuilder.Entity<O_Ticket_Booking>(e =>
            {
                e.ToTable("O_Ticket_Bookings");
                e.HasKey(x => x.Id);

                e.Property(x => x.Area).HasMaxLength(50);
                e.Property(x => x.TimeSlot).HasMaxLength(5);
                e.Property(x => x.GroupCode).HasMaxLength(20);
                e.Property(x => x.Company).HasMaxLength(100);
                e.Property(x => x.GroupName).HasMaxLength(100);
                e.Property(x => x.UserName).HasMaxLength(50);
                e.Property(x => x.UserPhone).HasMaxLength(50);
                e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Unverified");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

                // 當日序號唯一
                e.HasIndex(x => new { x.Date, x.SerialNo })
                 .HasDatabaseName("IX_Bookings_Date_Serial")
                 .IsUnique();

                // GroupCode 唯一 (允許 NULL)
                e.HasIndex(x => x.GroupCode)
                 .HasDatabaseName("UX_Bookings_GroupCode_Filtered")
                 .IsUnique()
                 .HasFilter("[GroupCode] IS NOT NULL");

                // 常用查詢
                e.HasIndex(x => new { x.Date, x.TimeSlot })
                 .HasDatabaseName("IX_Bookings_Date_Time");

                // 必要檢查
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Bus_NonNegative", "[BusCount] >= 0");
                    t.HasCheckConstraint("CK_Status_Enum",
                        "[Status] IN ('Unverified','Confirmed','Cancelled')");
                });

                // 關聯：主檔 → DIY 明細
                e.HasMany(b => b.DiyDetails)
                 .WithOne(d => d.Booking)
                 .HasForeignKey(d => d.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                // 關聯：主檔 → 票種明細
                e.HasMany(b => b.TicketLines)
                 .WithOne(l => l.Booking)
                 .HasForeignKey(l => l.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── O_Ticket_DiyBookings ───────────────────────────
            modelBuilder.Entity<O_Ticket_DiyBooking>(e =>
            {
                e.ToTable("O_Ticket_DiyBookings");
                e.HasKey(x => x.Id);
                e.Property(x => x.TimeSlot).HasMaxLength(5);

                e.HasIndex(x => new { x.Date, x.TimeSlot })
                 .HasDatabaseName("IX_Diy_Date_Time");

                e.ToTable(t => t.HasCheckConstraint("CK_Diy_Count_NonNegative", "[Count] >= 0"));
            });

            // ── O_Ticket_TicketTypes（票種主檔） ────────────────
            modelBuilder.Entity<O_Ticket_TicketType>(e =>
            {
                e.ToTable("O_Ticket_TicketTypes");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.UnitPrice)
                    .HasColumnName("Price")
                    .HasColumnType("decimal(18,2)")
                     .HasDefaultValue(0m);


                //  修正：把程式的 UnitPrice 對應到資料庫的 Price 欄位
                e.Property(x => x.UnitPrice).HasColumnName("Price").HasDefaultValue(0m);


                e.Property(x => x.IsEntrance).HasDefaultValue(true);
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.Sort).HasDefaultValue(0);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // ── O_Ticket_BookingTickets（訂單票種明細） ─────────
            modelBuilder.Entity<O_Ticket_BookingTicket>(e =>
            {
                e.ToTable("O_Ticket_BookingTickets");
                e.HasKey(x => x.Id);

                e.Property(x => x.Count).HasDefaultValue(0);
                e.Property(x => x.UnitPrice).HasColumnType("decimal(10,0)").HasDefaultValue(0);

                // 同一訂單 × 同一票種 => 唯一
                e.HasIndex(x => new { x.BookingId, x.TicketTypeId })
                 .HasDatabaseName("UX_BookingTicket")
                 .IsUnique();

                e.HasOne(x => x.Booking)
                 .WithMany(b => b.TicketLines)
                 .HasForeignKey(x => x.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.TicketType)
                 .WithMany() // 票種不需要反向集合
                 .HasForeignKey(x => x.TicketTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
