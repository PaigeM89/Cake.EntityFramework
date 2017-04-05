using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.EntityFramework.TestProject.SqlServer.Models;

namespace Cake.EntityFramework.TestProject.SqlServer
{
    public class SchoolContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
    }
    public class AtomicMigrationScriptBuilder : SqlServerMigrationSqlGenerator
    {
        public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            yield return new MigrationStatement { Sql = "BEGIN TRANSACTION" };

            foreach (var migrationStatement in base.Generate(migrationOperations, providerManifestToken))
            {
                yield return migrationStatement;
            }

            yield return new MigrationStatement { Sql = "COMMIT TRANSACTION" };
        }
    }

}
