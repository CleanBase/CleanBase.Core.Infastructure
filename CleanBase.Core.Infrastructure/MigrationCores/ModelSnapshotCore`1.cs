using CleanBase.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.MigrationCores
{
    public class ModelSnapshotCore<T> : ModelSnapshot where T : AppSettings, new()
    {
        protected T Settings { get; private set; }

        public ModelSnapshotCore()
        {
            Settings = AppSettings<T>.Instance ?? new T { Schema = "CleanBase" };
        }

        protected override void BuildModel(ModelBuilder modelBuilder)
        {
        }
    }
}
