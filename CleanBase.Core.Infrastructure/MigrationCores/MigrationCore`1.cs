using CleanBase.Core.Settings;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.MigrationCores
{
    public class MigrationCore<T> : Migration where T : AppSettings, new()
    {
        protected readonly T Settings;

        public MigrationCore()
        {
            Settings = AppSettings<T>.Instance ?? new T { Schema = "CleanBase" };
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!string.IsNullOrEmpty(Settings.Schema))
            {
                migrationBuilder.EnsureSchema(Settings.Schema);
            }
        }
    }
}
