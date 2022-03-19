using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class RSSMSContext : DbContext
    {
        public RSSMSContext()
        {
        }

        public RSSMSContext(DbContextOptions<RSSMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Floor> Floors { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderDetailServiceMap> OrderDetailServiceMaps { get; set; }
        public virtual DbSet<OrderHistoryExtension> OrderHistoryExtensions { get; set; }
        public virtual DbSet<OrderTimeline> OrderTimelines { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<RequestDetail> RequestDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Space> Spaces { get; set; }
        public virtual DbSet<StaffAssignStorage> StaffAssignStorages { get; set; }
        public virtual DbSet<Storage> Storages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Birthdate).HasColumnType("date");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeviceTokenId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FirebaseId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl).HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Phone)
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_Role");
            });

            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Area");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Length).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.StorageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Area_Storage");
            });

            modelBuilder.Entity<Floor>(entity =>
            {
                entity.ToTable("Floor");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Length).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Space)
                    .WithMany(p => p.Floors)
                    .HasForeignKey(d => d.SpaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Floor_Shelf");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("Image");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.OrderDetailid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Image_OrderDetail");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.HasOne(d => d.Receiver)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.ReceiverId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notification_Account");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.RequestId)
                    .HasConstraintName("FK_Notification_Request");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryAddress).HasMaxLength(255);

                entity.Property(e => e.DeliveryDate).HasColumnType("date");

                entity.Property(e => e.DeliveryTime)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RejectedReason).HasMaxLength(255);

                entity.Property(e => e.ReturnAddress).HasMaxLength(255);

                entity.Property(e => e.ReturnDate).HasColumnType("date");

                entity.Property(e => e.ReturnTime)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_Account");

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StorageId)
                    .HasConstraintName("FK_Order_Storage");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Length).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Floor)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.FloorId)
                    .HasConstraintName("FK_OrderDetail_Floor");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<OrderDetailServiceMap>(entity =>
            {
                entity.ToTable("OrderDetailServiceMap");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.OrderDetailServiceMaps)
                    .HasForeignKey(d => d.OrderDetailId)
                    .HasConstraintName("FK_OrderDetailServiceMap_OrderDetail");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.OrderDetailServiceMaps)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("FK_OrderDetailServiceMap_Service");
            });

            modelBuilder.Entity<OrderHistoryExtension>(entity =>
            {
                entity.ToTable("OrderHistoryExtension");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.OldReturnDate).HasColumnType("date");

                entity.Property(e => e.PaidDate).HasColumnType("date");

                entity.Property(e => e.ReturnDate).HasColumnType("date");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderHistoryExtensions)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderHistoryExtension_Order");
            });

            modelBuilder.Entity<OrderTimeline>(entity =>
            {
                entity.ToTable("OrderTimeline");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Description).HasMaxLength(25);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderTimelines)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderTimeline_Order");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Request");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CancelReason).HasMaxLength(255);

                entity.Property(e => e.CreatedDate).HasColumnType("date");

                entity.Property(e => e.DeliveryAddress).HasMaxLength(255);

                entity.Property(e => e.DeliveryDate).HasColumnType("date");

                entity.Property(e => e.DeliveryTime)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.ReturnAddress).HasMaxLength(255);

                entity.Property(e => e.ReturnDate).HasColumnType("date");

                entity.Property(e => e.ReturnTime)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK_Request_Account");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Request_Order");
            });

            modelBuilder.Entity<RequestDetail>(entity =>
            {
                entity.ToTable("RequestDetail");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.RequestId)
                    .HasConstraintName("FK_RequestDetail_Request");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("FK_RequestDetail_Service1");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedule");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.ScheduleDay).HasColumnType("date");

                entity.Property(e => e.ScheduleTime)
                    .IsRequired()
                    .HasMaxLength(12);

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.RequestId)
                    .HasConstraintName("FK_Schedule_Request");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Schedule_Account");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ImageUrl).HasMaxLength(255);

                entity.Property(e => e.Length).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ToolTip).HasMaxLength(255);

                entity.Property(e => e.Unit).HasMaxLength(20);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");
            });

            modelBuilder.Entity<Space>(entity =>
            {
                entity.ToTable("Space");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.Spaces)
                    .HasForeignKey(d => d.AreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Shelf_Area");
            });

            modelBuilder.Entity<StaffAssignStorage>(entity =>
            {
                entity.ToTable("StaffAssignStorage");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate).HasColumnType("date");

                entity.Property(e => e.RoleName).HasMaxLength(255);

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.StaffAssignStorages)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StaffAssignStorage_Account");

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.StaffAssignStorages)
                    .HasForeignKey(d => d.StorageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StaffAssignStorage_Storage");
            });

            modelBuilder.Entity<Storage>(entity =>
            {
                entity.ToTable("Storage");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Height).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ImageUrl).HasMaxLength(255);

                entity.Property(e => e.Length).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 3)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
