﻿using System;
using System.Linq;

using Cake.EntityFramework6.Interfaces;
using Cake.EntityFramework6.Migrator;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Cake.EntityFramework6.Tests.Migrator.Postgres
{
    public class MigratorFacts
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ITestOutputHelper _logHelper;

        private readonly ILogger _mockLogger;

        private readonly string _instanceString = PostgresFactConstants.InstanceConnectionString;

        private IEfMigrator Migrator => new EfMigrator(PostgresFactConstants.DdlPath, PostgresFactConstants.ConfigName, PostgresFactConstants.AppConfig,
            _instanceString, PostgresFactConstants.ConnectionProvider, _mockLogger);

        public MigratorFacts(ITestOutputHelper logHelper)
        {
            _logHelper = logHelper;
            _mockLogger = new MockLogger(logHelper);

            _logHelper.WriteLine($"Using connectionString: {_instanceString}");
        }

        [Fact]
        public void When_no_error_on_construct_ready_should_be_true()
        {
            var migrator = Migrator;

            // Act
            var result = migrator.Ready;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void When_migrating_from_InitialDatabase_to_latest_rollback_should_rollback_to_InitialDatabase()
        {
            var migrator = Migrator;
            var lastGoodMigration = migrator.GetLocalMigrations().Skip(1).First();

            // Act
            migrator.MigrateTo(lastGoodMigration);
            migrator.Rollback();

            // Assert
            var assertMigrator = Migrator;
            assertMigrator.GetRemoteMigrations().Should().BeEmpty();
        }

        [Fact]
        public void When_migrating_fails_return_false()
        {
            var migrator = Migrator;

            // Act
            Action action = () => migrator.MigrateToLatest();

            // Assert
            action.ShouldThrow<Exception>();
            // TODO
            // Will throw EfMigrationException if Npgsql is installed in GAC, else it will throw SerializationException. 
            // How to handle for CI?
        }

        [Fact]
        public void When_migrating_succeeds_return_true()
        {
            var migrator = Migrator;
            var migrations = migrator.GetLocalMigrations().ToList();
            var firstMigration = migrations.First();

            // Act
            var success = migrator.MigrateTo(firstMigration);

            // Assert
            success.Should().BeTrue();
        }

        [Fact]
        public void When_committed_rollback_should_throw()
        {
            var migrator = Migrator;
            var lastGoodMigration = migrator.GetLocalMigrations().Skip(1).First();

            // Act
            migrator.MigrateTo(lastGoodMigration);
            migrator.Commit();

            Action action = () => migrator.Rollback();

            // Assert
            action.ShouldThrow<Exception>();
        }
    }
}