using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.DBIntegration
{
    public class DatabaseInitializerOptions
    {
        public  string MasterConnectionString { get; set; }
        public  string DatabaseConnectionString { get; set; }
        public  string BackupPath { get; set; }
        public  string DatabaseName { get; set; }
        public  string ToRunSqlsPath { get; set; }
        public  string SqlFilesFilter { get; set; }
        public  string AfterRestoreScriptFile { get; set; }
    }
}
