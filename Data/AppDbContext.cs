using Microsoft.EntityFrameworkCore;
using PayrollAPI.Models;

namespace PayrollAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Allowance> Allowances { get; set; }
    public DbSet<Bonus> Bonuses { get; set; }
    public DbSet<Deduction> Deductions { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<SalaryRule> SalaryRules { get; set; }
    public DbSet<Payslip> Payslips { get; set; }
    public DbSet<PayslipLineItem> PayslipLineItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        // ── Employee ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email).IsUnique();

        // ── Department ────────────────────────────────────────────────────────
        modelBuilder.Entity<Department>()
            .Property(d => d.BaseSalary).HasColumnType("decimal(18,2)");

        // ── Allowance ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Allowance>()
            .Property(a => a.Amount).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Allowance>()
            .HasOne(a => a.Department)
            .WithMany(d => d.Allowances)
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<Allowance>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Allowances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // ── Bonus ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Bonus>()
            .Property(b => b.Value).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Bonus>()
            .HasOne(b => b.Department)
            .WithMany(d => d.Bonuses)
            .HasForeignKey(b => b.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<Bonus>()
            .HasOne(b => b.Employee)
            .WithMany(e => e.Bonuses)
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // ── Deduction ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Deduction>()
            .Property(d => d.Amount).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Deduction>()
            .HasOne(d => d.Department)
            .WithMany(dep => dep.Deductions)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<Deduction>()
            .HasOne(d => d.Employee)
            .WithMany(e => e.Deductions)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // ── Attendance ────────────────────────────────────────────────────────
        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: one attendance record per employee per day
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();

        // ── SalaryRule ────────────────────────────────────────────────────────
        modelBuilder.Entity<SalaryRule>()
            .Property(r => r.PenaltyDays).HasColumnType("decimal(5,2)");

        // ── Payslip ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Payslip>()
            .Property(p => p.BaseSalary).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payslip>()
            .Property(p => p.TotalAllowances).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payslip>()
            .Property(p => p.TotalBonuses).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payslip>()
            .Property(p => p.TotalDeductions).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payslip>()
            .Property(p => p.AttendancePenalty).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Payslip>()
            .Property(p => p.NetPayable).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Payslip>()
            .HasOne(p => p.Employee)
            .WithMany(e => e.Payslips)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: one payslip per employee per month/year
        modelBuilder.Entity<Payslip>()
            .HasIndex(p => new { p.EmployeeId, p.Month, p.Year }).IsUnique();

        // ── PayslipLineItem ───────────────────────────────────────────────────
        modelBuilder.Entity<PayslipLineItem>()
            .Property(l => l.Amount).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PayslipLineItem>()
            .HasOne(l => l.Payslip)
            .WithMany(p => p.LineItems)
            .HasForeignKey(l => l.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
