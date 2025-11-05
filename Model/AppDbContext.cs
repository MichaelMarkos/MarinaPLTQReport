using Microsoft.EntityFrameworkCore;

namespace maria.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportImage> ReportFiles { get; set; }
        public DbSet<CheckingItem> CheckingItems { get; set; }
        public DbSet<SiteReport> SiteReports { get; set; }
        public DbSet<CheckingItemReport> CheckingItemReports { get; set; }
        public DbSet<SiteReportImage> SiteReportImages { get; set; }
        public DbSet<DeliveryNote> DeliveryNotes { get; set; }
        public DbSet<DeliveryReport> DeliveryReport { get; set; }
        public DbSet<DeliveryNoteReport> DeliveryNoteReport { get; set; }


    }
}
