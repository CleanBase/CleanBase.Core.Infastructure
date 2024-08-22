using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.EF.Context
{
    public class DbContextBase : DbContext
    {
        protected string Schema { get; set; }

        protected string TablePrefix { get; set; }

        public DbContextBase(DbContextOptions options, string schema, string tablePrefix)
            : base(options)
        {
            this.Schema = schema;
            this.TablePrefix = tablePrefix ?? string.Empty;
        }
    }
}
