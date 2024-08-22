using CleanBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace CleanBase.Core.Infrastructure.EF.Context
{
    public class EmailDbContext : DbContextBase
    {
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        public EmailDbContext(DbContextOptions<EmailDbContext> options, string schema, string tablePrefix)
            : base(options, schema, tablePrefix)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureEmailTemplateEntity(modelBuilder);
        }

        private void ConfigureEmailTemplateEntity(ModelBuilder modelBuilder)
        {
            var tableName = TablePrefix + "EmailTemplate";

            var entity = modelBuilder.Entity<EmailTemplate>();

            if (!string.IsNullOrEmpty(Schema))
            {
                entity.ToTable(tableName, Schema);
            }
            else
            {
                entity.ToTable(tableName);
            }

            entity.HasKey(e => e.Id);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.CreatedDate)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );

            entity.Property(e => e.UpdatedDate)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );
        }
    }
}
