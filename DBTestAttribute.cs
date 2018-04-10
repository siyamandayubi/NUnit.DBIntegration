using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.DBIntegration
{
    public class DBTestAttribute : Attribute, IApplyToContext
    {
        private string[] scriptFiles;
        private bool resetDatabase;
        public DBTestAttribute(bool resetDatabase = false, string[] scriptFiles = null)
        {
            this.scriptFiles = scriptFiles;
            this.resetDatabase = resetDatabase;
        }

        public void ApplyToContext(TestExecutionContext context)
        {
            if (resetDatabase)
            {
                if (!DatabaseWrapper.Initialized)
                {
                    throw new Exception("Database is not initialized");
                }

                DatabaseWrapper.RestoreLastbackup();
            }

            if (scriptFiles != null && scriptFiles.Length > 0)
            {
                foreach(var scriptFile in scriptFiles)
                {
                    if (!string.IsNullOrWhiteSpace(scriptFile))
                    {
                        DatabaseWrapper.RunScript(scriptFile);
                    }
                }
            }
        }
    }
}
