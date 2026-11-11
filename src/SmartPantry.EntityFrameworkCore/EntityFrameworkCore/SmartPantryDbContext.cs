using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using SmartPantry.Products;
using SmartPantry.Pantries;
using SmartPantry.Warnings;

namespace SmartPantry.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class SmartPantryDbContext :
    AbpDbContext<SmartPantryDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
        public DbSet<Product> Products { get; set; }
        public DbSet<Pantry> Pantries { get; set; }
        public DbSet<ExpirationWarning> ExpirationWarnings { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public SmartPantryDbContext(DbContextOptions<SmartPantryDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();

if (Database.ProviderName?.Contains("MySql") == true)
{
    builder.Entity<IdentityUser>(b =>
    {
        b.Ignore(x => x.Passkeys);
    });

    builder.Ignore<IdentityUserPasskey>();
}
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        builder.Entity<Product>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "Products", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            b.OwnsOne(x => x.Barcode, barcode =>
            {
                barcode.Property(x => x.Code)
                    .HasColumnName("Barcode")
                    .IsRequired()
                    .HasMaxLength(ProductConsts.MaxBarcodeLength);
            });

            b.Property(x => x.Name).IsRequired().HasMaxLength(ProductConsts.MaxNameLength);
            b.Property(x => x.Brand).HasMaxLength(ProductConsts.MaxBrandLength);
        });

        builder.Entity<Pantry>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "Pantries", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(PantryConsts.MaxNameLength);
            b.HasIndex(x => x.UserId);

            b.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey("PantryId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<PantryItem>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "PantryItems", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.ExpirationDate)
                .HasConversion<DateOnlyDateTimeConverter>()
                .HasColumnType("date");
        });

        builder.Entity<ExpirationWarning>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "ExpirationWarnings", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.PantryId).IsRequired();
            b.Property(x => x.PantryItemId).IsRequired();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.ExpirationDate).IsRequired();
            b.Property(x => x.IsActive).IsRequired();

            // Clave lógica: una sola advertencia por ítem y tipo
            b.HasIndex(x => new { x.PantryItemId, x.Type }).IsUnique();
            b.Property(x => x.ExpirationDate)
                .HasConversion<DateOnlyDateTimeConverter>()
                .HasColumnType("date");
        });
    }
}
