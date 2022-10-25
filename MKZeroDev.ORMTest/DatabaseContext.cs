using MKZeroDev.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKZeroDev.ORMTest
{
    public class DatabaseContext : ORMDatabase
    {
        public DatabaseContext(string connectionString) : base(connectionString)
        {
            base.DatabaseUpdate(this);
        }

        public ORMTable<Category> Categories { get; set; }
        public ORMTable<Product> Products { get; set; }
        public ORMTable<Result> Results { get; set; }
    }
}
