using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockGTO.Models;

namespace StockGTO.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ===== 既有表 =====
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

        // ===== O_Ticket =====
        public DbSet<O_Ticket_Booking> O_Ticket_Bookings { get; set; }
        public DbSet<O_Ticket_DiyBooking> O_Ticket_DiyBookings { get; set; }
        public DbSet<O_Ticket_TicketType> O_Ticket_TicketTypes { get; set; }
        public DbSet<O_Ticket_BookingTicket> O_Ticket_BookingTickets { get; set; }

        // ===== O_HR_Control =====
        public DbSet<O_HR_Control_Department> O_HR_Control_Departments => Set<O_HR_Control_Department>();
        public DbSet<O_HR_Control_Employee> O_HR_Control_Employees => Set<O_HR_Control_Employee>();
        public DbSet<O_HR_Control_Holiday> O_HR_Control_Holidays => Set<O_HR_Control_Holiday>();
        public DbSet<O_HR_Control_ImportBatch> O_HR_Control_ImportBatches => Set<O_HR_Control_ImportBatch>();
        public DbSet<O_HR_Control_RawTimePunch> O_HR_Control_RawTimePunches => Set<O_HR_Control_RawTimePunch>();
        public DbSet<O_HR_Control_WorkSession> O_HR_Control_WorkSessions => Set<O_HR_Control_WorkSession>();
        public DbSet<O_HR_Control_DailyAttendance> O_HR_Control_DailyAttendances => Set<O_HR_Control_DailyAttendance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== O_Ticket =====================
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

                e.HasIndex(x => new { x.Date, x.SerialNo })
                 .HasDatabaseName("IX_Bookings_Date_Serial")
                 .IsUnique();

                e.HasIndex(x => x.GroupCode)
                 .HasDatabaseName("UX_Bookings_GroupCode_Filtered")
                 .IsUnique()
                 .HasFilter("[GroupCode] IS NOT NULL");

                e.HasIndex(x => new { x.Date, x.TimeSlot })
                 .HasDatabaseName("IX_Bookings_Date_Time");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Bus_NonNegative", "[BusCount] >= 0");
                    t.HasCheckConstraint("CK_Status_Enum",
                        "[Status] IN ('Unverified','Confirmed','Cancelled')");
                });

                e.HasMany(b => b.DiyDetails)
                 .WithOne(d => d.Booking)
                 .HasForeignKey(d => d.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(b => b.TicketLines)
                 .WithOne(l => l.Booking)
                 .HasForeignKey(l => l.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<O_Ticket_DiyBooking>(e =>
            {
                e.ToTable("O_Ticket_DiyBookings");
                e.HasKey(x => x.Id);
                e.Property(x => x.TimeSlot).HasMaxLength(5);

                e.HasIndex(x => new { x.Date, x.TimeSlot })
                 .HasDatabaseName("IX_Diy_Date_Time");

                e.ToTable(t => t.HasCheckConstraint("CK_Diy_Count_NonNegative", "[Count] >= 0"));
            });

            modelBuilder.Entity<O_Ticket_TicketType>(e =>
            {
                e.ToTable("O_Ticket_TicketTypes");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();

                // 對應 UnitPrice -> Price 欄位一次就好（移除你檔案裡重複的設定）
                e.Property(x => x.UnitPrice)
                 .HasColumnName("Price")
                 .HasColumnType("decimal(18,2)")
                 .HasDefaultValue(0m);

                e.Property(x => x.IsEntrance).HasDefaultValue(true);
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.Sort).HasDefaultValue(0);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<O_Ticket_BookingTicket>(e =>
            {
                e.ToTable("O_Ticket_BookingTickets");
                e.HasKey(x => x.Id);

                e.Property(x => x.Count).HasDefaultValue(0);
                e.Property(x => x.UnitPrice).HasColumnType("decimal(10,0)").HasDefaultValue(0);

                e.HasIndex(x => new { x.BookingId, x.TicketTypeId })
                 .HasDatabaseName("UX_BookingTicket")
                 .IsUnique();

                e.HasOne(x => x.Booking)
                 .WithMany(b => b.TicketLines)
                 .HasForeignKey(x => x.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.TicketType)
                 .WithMany()
                 .HasForeignKey(x => x.TicketTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== O_HR_Control =====================
            modelBuilder.Entity<O_HR_Control_Department>()
                .ToTable("O_HR_Control_Department");

            modelBuilder.Entity<O_HR_Control_Employee>(e =>
            {
                e.ToTable("O_HR_Control_Employee");
                e.HasIndex(x => x.EmpNo).IsUnique();
            });

            modelBuilder.Entity<O_HR_Control_Holiday>(e =>
            {
                e.ToTable("O_HR_Control_Holiday");

                // 1) 指定主鍵＝Date（你已經拿掉 Id，就用 Date 做 PK）
                e.HasKey(h => h.Date);

                // 2) 明確欄位型別：DateOnly → SQL Server 的 date
                e.Property(h => h.Date)
                 .HasColumnType("date");

                // 3) 說明欄位（你在表單貼的文字），給長度上限避免溢出
                e.Property(h => h.Memo)
                 .HasMaxLength(200);

                // 4) Name 保留當「節日名稱」：例如「國慶日」，也給個長度
                e.Property(h => h.Name)
                 .HasMaxLength(50)
                 .HasDefaultValue(string.Empty);
            });


            modelBuilder.Entity<O_HR_Control_ImportBatch>()
                .ToTable("O_HR_Control_ImportBatch");

            modelBuilder.Entity<O_HR_Control_RawTimePunch>(e =>
            {
                e.ToTable("O_HR_Control_RawTimePunch");
                e.HasIndex(x => new { x.EmpNo, x.PunchDateTime });
            });

            modelBuilder.Entity<O_HR_Control_WorkSession>(e =>
            {
                e.ToTable("O_HR_Control_WorkSession");
                e.HasIndex(x => new { x.EmpId, x.StartDT });
            });

            modelBuilder.Entity<O_HR_Control_DailyAttendance>(e =>
            {
                e.ToTable("O_HR_Control_DailyAttendance");
                e.HasKey(x => new { x.EmpId, x.WorkDate });
            });
        }
    }
}
