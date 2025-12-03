using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Entities;

namespace OneCardExpenseValidator.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BusinessPolicy> BusinessPolicies { get; set; }

    public virtual DbSet<CategorizationLog> CategorizationLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryKeyword> CategoryKeywords { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ExpenseItem> ExpenseItems { get; set; }

    public virtual DbSet<ExpenseTicket> ExpenseTickets { get; set; }

    public virtual DbSet<MonthlyExpenseReport> MonthlyExpenseReports { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAlias> ProductAliases { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwExpenseDashboard> VwExpenseDashboards { get; set; }

    public virtual DbSet<VwPendingExpense> VwPendingExpenses { get; set; }

    public virtual DbSet<VwTopCategory> VwTopCategories { get; set; }

    public virtual DbSet<VwUserActivity> VwUserActivities { get; set; }

    public virtual DbSet<VwUsersWithRole> VwUsersWithRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MAMALONA;Database=OneCardExpenseValidator;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__A17F2398A0E1196E");

            entity.ToTable("AuditLog");

            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TableName).HasMaxLength(50);
        });

        modelBuilder.Entity<BusinessPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Business__2E1339A4C2ECC955");

            entity.HasIndex(e => e.PolicyCode, "UQ__Business__936831859BDD66BC").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDailyAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaxMonthlyAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MinApprovalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PolicyCode).HasMaxLength(50);
            entity.Property(e => e.PolicyName).HasMaxLength(200);
            entity.Property(e => e.RequiresManagerApproval).HasDefaultValue(false);
            entity.Property(e => e.RequiresReceipt).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.BusinessPolicies)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__BusinessP__Categ__3F466844");
        });

        modelBuilder.Entity<CategorizationLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Categori__5E5486488E6B1C7D");

            entity.ToTable("CategorizationLog");

            entity.HasIndex(e => e.ItemDescription, "IX_CategorizationLog_ItemDescription");

            entity.Property(e => e.AlgorithmUsed).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemDescription).HasMaxLength(500);

            entity.HasOne(d => d.CorrectCategory).WithMany(p => p.CategorizationLogCorrectCategories)
                .HasForeignKey(d => d.CorrectCategoryId)
                .HasConstraintName("FK__Categoriz__Corre__68487DD7");

            entity.HasOne(d => d.PredictedCategory).WithMany(p => p.CategorizationLogPredictedCategories)
                .HasForeignKey(d => d.PredictedCategoryId)
                .HasConstraintName("FK__Categoriz__Predi__6754599E");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B1C2C18F6");

            entity.HasIndex(e => e.CategoryCode, "UQ__Categori__371BA955C192CDBF").IsUnique();

            entity.Property(e => e.CategoryCode).HasMaxLength(50);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsDeductible).HasDefaultValue(true);
            entity.Property(e => e.MaxAmountAllowed).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RequiresApproval).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<CategoryKeyword>(entity =>
        {
            entity.HasKey(e => e.KeywordId).HasName("PK__Category__37C13521F4696867");

            entity.HasIndex(e => e.Keyword, "IX_CategoryKeywords_Keyword");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Keyword).HasMaxLength(100);
            entity.Property(e => e.Weight).HasDefaultValue(1.0);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryKeywords)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__CategoryK__Categ__45F365D3");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BEDB7F7F313");

            entity.HasIndex(e => e.DepartmentCode, "UQ__Departme__6EA8896DE7F1EDEC").IsUnique();

            entity.Property(e => e.BudgetLimit).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F1125A12C16");

            entity.HasIndex(e => e.EmployeeCode, "UQ__Employee__1F6425481CE744BF").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Employee__A9D1053487607ED1").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DailyExpenseLimit)
                .HasDefaultValue(1000.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MonthlyExpenseLimit)
                .HasDefaultValue(15000.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Employees__Depar__5165187F");
        });

        modelBuilder.Entity<ExpenseItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__ExpenseI__727E838B87F82E2F");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateTicketTotals"));

            entity.HasIndex(e => e.CategoryId, "IX_ExpenseItems_CategoryId");

            entity.HasIndex(e => e.ProductId, "IX_ExpenseItems_ProductId");

            entity.HasIndex(e => e.TicketId, "IX_ExpenseItems_TicketId");

            entity.Property(e => e.ItemDescription).HasMaxLength(500);
            entity.Property(e => e.OriginalDescription).HasMaxLength(500);
            entity.Property(e => e.PolicyValidation).HasMaxLength(50);
            entity.Property(e => e.ProcessedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ValidationNotes).HasMaxLength(500);

            entity.HasOne(d => d.Category).WithMany(p => p.ExpenseItems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__ExpenseIt__Categ__6383C8BA");

            entity.HasOne(d => d.Product).WithMany(p => p.ExpenseItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ExpenseItems_Products");

            entity.HasOne(d => d.Ticket).WithMany(p => p.ExpenseItems)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ExpenseIt__Ticke__619B8048");
        });

        modelBuilder.Entity<ExpenseTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__ExpenseT__712CC60790885BB3");

            entity.HasIndex(e => e.CreatedByUserId, "IX_ExpenseTickets_CreatedByUserId");

            entity.HasIndex(e => e.EmployeeId, "IX_ExpenseTickets_EmployeeId");

            entity.HasIndex(e => e.TicketDate, "IX_ExpenseTickets_TicketDate");

            entity.HasIndex(e => e.ValidationStatus, "IX_ExpenseTickets_ValidationStatus");

            entity.Property(e => e.ApprovalDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeductibleAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NonDeductibleAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.OcrextractedText).HasColumnName("OCRExtractedText");
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.SubmissionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TicketImagePath).HasMaxLength(500);
            entity.Property(e => e.TicketNumber)
                .HasMaxLength(9)
                .HasComputedColumnSql("('TK-'+right('00000'+CONVERT([nvarchar](10),[TicketId]),(6)))", true);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ValidationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("PENDIENTE");
            entity.Property(e => e.Vendor).HasMaxLength(200);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ExpenseTicketApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__ExpenseTi__Appro__5CD6CB2B");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ExpenseTickets)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_ExpenseTickets_Users");

            entity.HasOne(d => d.Employee).WithMany(p => p.ExpenseTicketEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__ExpenseTi__Emplo__5812160E");
        });

        modelBuilder.Entity<MonthlyExpenseReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__MonthlyE__D5BD4805EF16F65E");

            entity.HasIndex(e => new { e.EmployeeId, e.ReportMonth, e.ReportYear }, "UQ_EmployeeMonthYear").IsUnique();

            entity.Property(e => e.GeneratedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalApproved).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalDeductible).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalExpenses).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalNonDeductible).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalRejected).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.MonthlyExpenseReports)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__MonthlyEx__Emplo__6FE99F9F");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD56EDD577");

            entity.HasIndex(e => e.DefaultCategoryId, "IX_Products_DefaultCategoryId");

            entity.HasIndex(e => e.ProductName, "IX_Products_ProductName");

            entity.HasIndex(e => e.Gtin, "UQ__Products__147E53F2CA61C340").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__Products__CA1ECF0D444A4658").IsUnique();

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gtin)
                .HasMaxLength(14)
                .HasColumnName("GTIN");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DefaultCategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.DefaultCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Defaul__7D439ABD");
        });

        modelBuilder.Entity<ProductAlias>(entity =>
        {
            entity.HasKey(e => e.AliasId).HasName("PK__ProductA__7DBDAF6C64612655");

            entity.HasIndex(e => e.Alias, "IX_ProductAliases_Alias");

            entity.HasIndex(e => new { e.ProductId, e.Alias }, "UX_ProductAliases_Product_Alias").IsUnique();

            entity.Property(e => e.Alias).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Source).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAliases)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductAl__Produ__02FC7413");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__RefreshT__658FEEEAE066D3E1");

            entity.HasIndex(e => e.Token, "IX_RefreshTokens_Token");

            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F817A941D6B3").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.IsRevoked).HasComputedColumnSql("(case when [RevokedAt] IS NOT NULL then (1) else (0) end)", true);
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__RefreshTo__UserI__1CBC4616");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1ADAA89BBA");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61603457C24B").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C96DCD31F");

            entity.ToTable(tb => tb.HasTrigger("trg_AuditUserChanges"));

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.EmployeeId, "IX_Users_EmployeeId");

            entity.HasIndex(e => e.Username, "IX_Users_Username");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4C6600295").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341AF52563").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Users__EmployeeI__0F624AF8");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A3558A3458C");

            entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleId");

            entity.HasIndex(e => e.UserId, "IX_UserRoles_UserId");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRole").IsUnique();

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.UserRoleAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__UserRoles__Assig__18EBB532");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__17036CC0");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoleUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__160F4887");
        });

        modelBuilder.Entity<VwExpenseDashboard>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ExpenseDashboard");

            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.TotalDeductible).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalExpenses).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VwPendingExpense>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PendingExpenses");

            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmployeeName).HasMaxLength(201);
            entity.Property(e => e.TicketNumber).HasMaxLength(9);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ValidationStatus).HasMaxLength(20);
        });

        modelBuilder.Entity<VwTopCategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_TopCategories");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.TotalSpent).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VwUserActivity>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserActivity");

            entity.Property(e => e.FullName).HasMaxLength(201);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastTicketSubmitted).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<VwUsersWithRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UsersWithRoles");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Roles).HasMaxLength(4000);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
