using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.DBIntegration.Database
{
    internal static class Factory
    {
        public static IDatabase Create(DatabaseTypes type)
        {
            if (type == DatabaseTypes.SqlServer)
            {
                return new SqlServerDatabase();
            }

            throw new NotImplementedException();
        }
    }
}
