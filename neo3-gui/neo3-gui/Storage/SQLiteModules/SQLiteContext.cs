﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Neo.Storage.SQLiteModules
{
    public class SQLiteContext : DbContext
    {
        public DbSet<SyncIndex> SyncIndices { get; set; }
        public DbSet<Nep5TransactionEntity> Nep5Transactions { get; set; }

        private readonly string _filename;
        public SQLiteContext(string filename)
        {
            this._filename = filename;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            SqliteConnectionStringBuilder sb = new SqliteConnectionStringBuilder
            {
                DataSource = _filename
            };
            optionsBuilder.UseSqlite(sb.ToString());
        }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Nep5TransactionEntity>().ToTable("Nep5Transaction");
            modelBuilder.Entity<Nep5TransactionEntity>().HasIndex(p => p.From);
            modelBuilder.Entity<Nep5TransactionEntity>().HasIndex(p => p.To);
            modelBuilder.Entity<Nep5TransactionEntity>().HasIndex(p => p.Time);
            modelBuilder.Entity<Nep5TransactionEntity>().HasIndex(p => p.TxId);
        }
    }
}
