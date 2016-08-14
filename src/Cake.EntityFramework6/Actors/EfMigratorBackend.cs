﻿namespace Cake.EntityFramework6.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Cake.EntityFramework6.Interfaces;

    public class EfMigratorBackend : MarshalByRefObject, IEfMigratorBackend
    {
        private string _parrentPath;
        private DbMigrator _dbMigrator;

        public bool Ready { get; private set; }

        public IEnumerable<string> GetLocalMigrations()
        {
            AssertForReady();
            return _dbMigrator.GetLocalMigrations();
        }

        public IEnumerable<string> GetRemoteMigrations()
        {
            AssertForReady();
            return _dbMigrator.GetDatabaseMigrations();
        }

        public IEnumerable<string> GetPendingMigrations()
        {
            AssertForReady();
            return _dbMigrator.GetPendingMigrations();
        }

        public string GetCurrentMigration()
        {
            AssertForReady();
            return _dbMigrator.GetDatabaseMigrations().FirstOrDefault();
        }

        public string GetLatestMigration()
        {
            return _dbMigrator.GetPendingMigrations().LastOrDefault();
        }

        public bool HasPendingMigrations()
        {
            AssertForReady();
            return GetPendingMigrations().Any();
        }

        public bool MigrateTo(string version)
        {
            AssertForReady();
            try
            {
                _dbMigrator.Update(version);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool MigrateToLatest()
        {
            AssertForReady();
            var last = GetLatestMigration();
            if (last != null)
            {
                return MigrateTo(last);
            }

            return true;
        }

        public void Initialize(string assemblyPath, string qualifiedDbConfigName, string connectionString, string connectionProvider)
        {
            if (Ready)
            {
                throw new Exception("EfMigrator is already initialize and cannot be re-initialize.");
            }

            var assemblyLocation = Path.GetFullPath(assemblyPath);
            if (!File.Exists(assemblyLocation))
            {
                throw new Exception($"The assemblyPath '{assemblyPath}' must exist, it currently doesn't.");
            }

            _parrentPath = Path.GetDirectoryName(assemblyLocation);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            var dbMigrationsConfiguration = LoadConfiguration(assemblyLocation, qualifiedDbConfigName);
            if (dbMigrationsConfiguration == null)
            {
                throw new Exception(
                    $"The qualifiedDbConfigName {qualifiedDbConfigName} must exist within {assemblyPath} and implement type {nameof(DbMigrationsConfiguration)}. Make sure this class exists or that this class is a Migration Configuration.");
            }
            dbMigrationsConfiguration.TargetDatabase = new DbConnectionInfo(connectionString, connectionProvider);
            _dbMigrator = new DbMigrator(dbMigrationsConfiguration);

            Ready = true;
        }

        private DbMigrationsConfiguration LoadConfiguration(string assemblyLocation, string qualifiedDbConfigName)
        {
            var dll = Assembly.LoadFrom(assemblyLocation);
            return dll.DefinedTypes
                      .Where(x => typeof(DbMigrationsConfiguration).IsAssignableFrom(x))
                      .Where(x => !x.IsAbstract)
                      .Select(x => Activator.CreateInstance(x) as DbMigrationsConfiguration)
                      .Where(x => x != null)
                      .SingleOrDefault(x => x.GetType().FullName == qualifiedDbConfigName);
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyPath =
                new DirectoryInfo(_parrentPath)
                    .GetFiles("*.dll")
                    .FirstOrDefault(x => x.Name == $"{args.Name}.dll");

            return assemblyPath != null ? Assembly.LoadFrom(assemblyPath.FullName) : null;
        }

        private void AssertForReady()
        {
            if (!Ready)
            {
                throw new Exception("Safety assert failed, improbable code reached. EfMigrator is not ready.");
            }
        }
    }
}