using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Models;

public partial class CoffeeShopContext : DbContext
{
    public CoffeeShopContext()
    {
    }

    public CoffeeShopContext(DbContextOptions<CoffeeShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActionType> ActionTypes { get; set; }

    public virtual DbSet<CafeTable> CafeTables { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryHistory> InventoryHistories { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemPrice> ItemPrices { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Otprequest> Otprequests { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StaffAttendance> StaffAttendances { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-UEB0IP6O;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionType>(entity =>
        {
            entity.HasKey(e => e.ActionTypeId).HasName("PK__ActionTy__62FE4C04152CCDC1");

            entity.ToTable("ActionType");

            entity.Property(e => e.ActionTypeId).HasColumnName("ActionTypeID");
            entity.Property(e => e.ActionName).HasMaxLength(20);
        });

        modelBuilder.Entity<CafeTable>(entity =>
        {
            entity.HasKey(e => e.TableId).HasName("PK__CafeTabl__7D5F018EA874D4D1");

            entity.ToTable("CafeTable");

            entity.Property(e => e.TableId).HasColumnName("TableID");
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.TableName).HasMaxLength(40);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2B1B5BD146");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(40);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8AC0621E2");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Customer__85FB4E384C91FEF9").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Tier)
                .HasMaxLength(50)
                .HasDefaultValue("Silver");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__Inventor__C506131767B5081F");

            entity.ToTable("Inventory");

            entity.Property(e => e.MaterialId).HasColumnName("MaterialID");
            entity.Property(e => e.MaterialName).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Threshold).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<InventoryHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__Inventor__4D7B4ADD099CBAF3");

            entity.ToTable("InventoryHistory");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.ActionTypeId).HasColumnName("ActionTypeID");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InputPrice).HasColumnType("money");
            entity.Property(e => e.MaterialId).HasColumnName("MaterialID");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.ActionType).WithMany(p => p.InventoryHistories)
                .HasForeignKey(d => d.ActionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvHistory_Action");

            entity.HasOne(d => d.Material).WithMany(p => p.InventoryHistories)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvHistory_Inventoy");

            entity.HasOne(d => d.Staff).WithMany(p => p.InventoryHistories)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvHistory_Staff");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Item__727E83EB23FFE524");

            entity.ToTable("Item");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ItemName).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Items)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_Category");
        });

        modelBuilder.Entity<ItemPrice>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PK__ItemPric__4957584FB61E4252");

            entity.ToTable("ItemPrice");

            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.SizeId).HasColumnName("SizeID");

            entity.HasOne(d => d.Item).WithMany(p => p.ItemPrices)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemPrice_Item");

            entity.HasOne(d => d.Size).WithMany(p => p.ItemPrices)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("FK_ItemPrice_Size");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAFB8952F3A");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(40)
                .HasDefaultValue("Chờ thanh toán");
            entity.Property(e => e.PaymentMethod).HasMaxLength(40);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.TableId).HasColumnName("TableID");
            entity.Property(e => e.TotalAmount).HasColumnType("money");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Order_Customer");

            entity.HasOne(d => d.Table).WithMany(p => p.Orders)
                .HasForeignKey(d => d.TableId)
                .HasConstraintName("FK_Order_Table");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C8515BF05");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.Discount).HasColumnType("money");
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.TotalPrice).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_Order");

            entity.HasOne(d => d.Price).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.PriceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_ItemPrice");
        });

        modelBuilder.Entity<Otprequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__OTPReque__33A8519A4DB70511");

            entity.ToTable("OTPRequest");

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.Code)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__Shift__C0A838E1D90B5382");

            entity.ToTable("Shift");

            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.ShiftName).HasMaxLength(10);
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK__Size__83BD095A3C6447B7");

            entity.ToTable("Size");

            entity.Property(e => e.SizeId).HasColumnName("SizeID");
            entity.Property(e => e.SizeName).HasMaxLength(5);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__96D4AAF722A4BEA2");

            entity.HasIndex(e => e.Username, "UQ__Staff__536C85E41645C452").IsUnique();

            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.BaseSalary).HasColumnType("money");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(150);
            entity.Property(e => e.Phonenumber).HasMaxLength(20);
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.StaffName).HasMaxLength(100);
            entity.Property(e => e.StaffRole).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Shift).WithMany(p => p.Staff)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_Staff_Shift");
        });

        modelBuilder.Entity<StaffAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__StaffAtt__8B69263C488288B9");

            entity.ToTable("StaffAttendance");

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.CheckIn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CheckOut).HasColumnType("datetime");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffAttendances)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Staff");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
