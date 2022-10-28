namespace MKZeroDev.ORM
{
    public abstract class ORMDatabase
    {
        private readonly string _connectionString = default!;
        private readonly ORMInitializer _ormInitializer = default!;

        protected ORMDatabase(string connectionString)
        {
            _connectionString = connectionString;
            _ormInitializer = new ORMInitializer(_connectionString);

            if (_ormInitializer.IsExistsDatabase())
                _ormInitializer.TableInstanceInitialize(this);
        }

        public virtual void DatabaseUpdate()
        {
            _ormInitializer.DatabaseInitialize(this);
            _ormInitializer.TableInstanceInitialize(this);
        }

        protected virtual void DatabaseUpdate<T>(T obj) where T : class
        {
            _ormInitializer.DatabaseInitialize(obj);
            _ormInitializer.TableInstanceInitialize(obj);
        }

        protected ORMTable<T> ORMTableSet<T>() where T : class
        {
            return new ORMTable<T>(_connectionString);
        }
    }

    public class ORMTable<T> where T : class
    {
        private readonly string _connectionString = default!;
        private readonly ORMExecutor _ormExecutor = default!;

        public ORMTable(string connectionString)
        {
            _connectionString = connectionString;
            _ormExecutor = new ORMExecutor(_connectionString);
        }

        public int Insert(T item)
        {
            return _ormExecutor.Insert<T>(item);
        }

        public int Update(T item)
        {
            return _ormExecutor.Update(item);
        }

        public int Delete(T item)
        {
            return _ormExecutor.Delete<T>(item);
        }

        public IList<T> SelectAll()
        {
            return _ormExecutor.SelectAll<T>();
        }

        public T? SelectFirstOrDefault()
        {
            return _ormExecutor.SelectFirstOrDefault<T>();
        }
    }
}
