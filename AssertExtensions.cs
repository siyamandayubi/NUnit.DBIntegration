using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.DBIntegration
{
    public static class AssertExtensions
    {
        public static void SqlScalarCheck<T>(this Assert assert, string sql, T value)
            where T :IConvertible
        {
            Object resultObj = DatabaseWrapper.RunSqlScalar(sql);

            T resultInt = (T)resultObj;
            Assert.AreEqual(resultInt, value);
        }
    }
}
