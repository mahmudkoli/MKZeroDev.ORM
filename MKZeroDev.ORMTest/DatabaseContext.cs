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

        }

        public ORMTable<Category> Categories { get; set; } = default!;
        public ORMTable<Product> Products { get; set; } = default!;
        public ORMTable<Result> Results { get; set; } = default!;

        public override void DatabaseUpdate()
        {
            base.DatabaseUpdate(this);
        }
    }
}
