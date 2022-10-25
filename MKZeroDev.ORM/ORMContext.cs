using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKZeroDev.ORM
{
    public abstract class ORMDatabase
    {
        private readonly string _connectionString = default!;

        public ORMDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DatabaseUpdate<T>(T obj) where T : ORMDatabase
        {
            var ormInit = new ORMInitializer(_connectionString);
            ormInit.DatabaseInitialize(obj);
        }
    }

    public class ORMTable<T> where T : class
    {

    }
}
