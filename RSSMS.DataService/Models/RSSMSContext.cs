﻿using System;
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

        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Box> Boxes { get; set; }
        public virtual DbSet<BoxOrderDetail> BoxOrderDetails { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotificationDetail> NotificationDetails { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderHistoryExtension> OrderHistoryExtensions { get; set; }
        public virtual DbSet<OrderStorageDetail> OrderStorageDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<RequestDetail> RequestDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Shelf> Shelves { get; set; }
        public virtual DbSet<StaffManageStorage> StaffManageStorages { get; set; }
        public virtual DbSet<Storage> Storages { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Area");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.StorageId)
                    .HasConstraintName("FK_Area_Storage");
            });

            modelBuilder.Entity<Box>(entity =>
            {
                entity.ToTable("Box");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Boxes)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Box_Product");

                entity.HasOne(d => d.Shelf)
                    .WithMany(p => p.Boxes)
                    .HasForeignKey(d => d.ShelfId)
                    .HasConstraintName("FK_Box_Shelf");
            });

            modelBuilder.Entity<BoxOrderDetail>(entity =>
            {
                entity.ToTable("BoxOrderDetail");

                entity.HasOne(d => d.Box)
                    .WithMany(p => p.BoxOrderDetails)
                    .HasForeignKey(d => d.BoxId)
                    .HasConstraintName("FK_BoxOrderDetail_Box");

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.BoxOrderDetails)
                    .HasForeignKey(d => d.OrderDetailId)
                    .HasConstraintName("FK_BoxOrderDetail_OrderDetail");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("Image");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(100);

                entity.Property(e => e.Type)
                    .HasMaxLength(10)
                    .IsFixedLength(true);

                entity.Property(e => e.Url).HasMaxLength(200);

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.OrderDetailId)
                    .HasConstraintName("FK_Image_OrderDetail");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Image_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Image_Product1");

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.StorageId)
                    .HasConstraintName("FK_Image_Storage");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Image_User1");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Note).HasMaxLength(255);
            });

            modelBuilder.Entity<NotificationDetail>(entity =>
            {
                entity.ToTable("NotificationDetail");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.HasOne(d => d.Notification)
                    .WithMany(p => p.NotificationDetails)
                    .HasForeignKey(d => d.NotificationId)
                    .HasConstraintName("FK_NotificationDetail_Notification");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.NotificationDetails)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_NotificationDetail_User");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.AddressReturn).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryAddress).HasMaxLength(100);

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryTime)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.RejectedReason).HasMaxLength(100);

                entity.Property(e => e.ReturnDate).HasColumnType("datetime");

                entity.Property(e => e.ReturnTime)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.OrderCustomers)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Order_User");

                entity.HasOne(d => d.Manager)
                    .WithMany(p => p.OrderManagers)
                    .HasForeignKey(d => d.ManagerId)
                    .HasConstraintName("FK_Order_User1");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_Product");
            });

            modelBuilder.Entity<OrderHistoryExtension>(entity =>
            {
                entity.ToTable("OrderHistoryExtension");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(50);

                entity.Property(e => e.OldReturnDate).HasColumnType("datetime");

                entity.Property(e => e.PaidDate).HasColumnType("datetime");

                entity.Property(e => e.ReturnDate).HasColumnType("datetime");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderHistoryExtensions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderHistoryExtension_Order");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.OrderHistoryExtensions)
                    .HasForeignKey(d => d.RequestId)
                    .HasConstraintName("FK_OrderHistoryExtension_Request");
            });

            modelBuilder.Entity<OrderStorageDetail>(entity =>
            {
                entity.ToTable("OrderStorageDetail");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderStorageDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderStorageDetail_Order");

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.OrderStorageDetails)
                    .HasForeignKey(d => d.StorageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderStorageDetail_Storage");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Size).HasMaxLength(100);

                entity.Property(e => e.ToolTip).HasColumnType("text");

                entity.Property(e => e.Unit).HasMaxLength(100);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ProductCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK_Product_User");

                entity.HasOne(d => d.ModifiedByNavigation)
                    .WithMany(p => p.ProductModifiedByNavigations)
                    .HasForeignKey(d => d.ModifiedBy)
                    .HasConstraintName("FK_Product_User1");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Request");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(100);

                entity.Property(e => e.ReturnAddress).HasMaxLength(255);

                entity.Property(e => e.ReturnDate).HasColumnType("datetime");

                entity.Property(e => e.ReturnTime).HasMaxLength(50);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Request_Order");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Request_User");
            });

            modelBuilder.Entity<RequestDetail>(entity =>
            {
                entity.HasKey(e => new { e.RequestId, e.BoxId });

                entity.ToTable("RequestDetail");

                entity.HasOne(d => d.Box)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.BoxId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestDetail_Box");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestDetail_Request");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedule");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryTime)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(100);

                entity.Property(e => e.SheduleDay).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Schedule_Order");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.RequestId)
                    .HasConstraintName("FK_Schedule_Request");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Schedule_User");
            });

            modelBuilder.Entity<Shelf>(entity =>
            {
                entity.ToTable("Shelf");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Note).HasMaxLength(100);

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.Shelves)
                    .HasForeignKey(d => d.AreaId)
                    .HasConstraintName("FK_Shelf_Area");
            });

            modelBuilder.Entity<StaffManageStorage>(entity =>
            {
                entity.ToTable("StaffManageStorage");

                entity.Property(e => e.RoleName).HasMaxLength(100);

                entity.Property(e => e.StorageName).HasMaxLength(100);

                entity.HasOne(d => d.Storage)
                    .WithMany(p => p.StaffManageStorages)
                    .HasForeignKey(d => d.StorageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StaffManageStorage_Storage");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.StaffManageStorages)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StaffManageStorage_User");
            });

            modelBuilder.Entity<Storage>(entity =>
            {
                entity.ToTable("Storage");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Size).HasMaxLength(100);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Storages)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Storage_Product");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.Birthdate).HasColumnType("date");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeviceTokenId).HasMaxLength(255);

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.FirebaseId).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.Phone)
                    .HasMaxLength(13)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_User_Role");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
