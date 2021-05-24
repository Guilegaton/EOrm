using EOrm.Attributes;
using EOrm.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EOrm.Repositories.ADO
{
    public class AdoRepository<TModel> : IRepository<TModel>, IDisposable where TModel : class, IEntity, new()
    {
        #region Public Constructors

        public AdoRepository(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            try
            {
                _connection.Open();
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection.Close();
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public SqlConnection _connection { get; private set; }
        public SqlTransaction _transaction { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void CommitChanges()
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public void Create(TModel model)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.Connection = _connection;
            command.CommandText = model.GetCreateCommand();

            command.ExecuteScalar();
        }

        public void Delete(TModel model)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.Connection = _connection;
            command.CommandText = model.GetDeleteCommand();

            command.ExecuteScalar();
        }

        public void DeleteById(int id)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.Connection = _connection;
            command.CommandText = CreateDeleteCommand(id);

            command.ExecuteScalar();
        }

        public void DiscardChandes()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction.Rollback();
            _connection.Close();
        }

        public IEnumerable<TModel> GetAll()
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.Connection = _connection;
            command.CommandText = CreateGetAllCommand();

            var reader = command.ExecuteReader();
            var result = ParseReader(reader);
            reader.Close();

            return result;
        }

        public TModel GetByField(Dictionary<Expression<Func<TModel, object>>, object> dict)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.Connection = _connection;
            command.CommandText = CreateGetCommand(dict);

            var reader = command.ExecuteReader();
            var result = ParseReader(reader);
            reader.Close();

            return result.FirstOrDefault();
        }

        public void Update(TModel model)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }

            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.Connection = _connection;
            command.CommandText = model.GetUpdateCommand();

            command.ExecuteScalar();
        }

        #endregion Public Methods

        #region Private Methods

        private string CreateDeleteCommand(int id)
        {
            var type = typeof(TModel);
            var result = new StringBuilder();
            try
            {
                result.Append("DELETE FROM ");
                result.AppendLine(type.Name);
                result.Append("WHERE [");
                var key = type.GetProperties().First(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>() != null);
                result.Append(key.Name);
                result.Append("] = ");
                result.Append(id);
                result.Append(";");
            }
            catch (Exception)
            {
                throw new Exception("Model class doesn't have Primary Key properties;");
            }
            return result.ToString();
        }

        private string CreateGetAllCommand()
        {
            var type = typeof(TModel);
            var result = new StringBuilder();
            result.Append("SELECT * FROM ");
            result.AppendLine(type.Name);
            result.Append(";");
            return result.ToString();
        }

        private string CreateGetCommand(int id)
        {
            var type = typeof(TModel);
            var result = new StringBuilder();
            try
            {
                result.Append("SELECT * FROM ");
                result.AppendLine(type.Name);
                result.Append("WHERE [");
                var key = type.GetProperties().First(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>() != null);
                result.Append(key.Name);
                result.Append("] = ");
                result.Append(id);
                result.Append(";");
            }
            catch (Exception)
            {
                throw new Exception("Model class doesn't have Primary Key properties;");
            }
            return result.ToString();
        }

        private string CreateGetCommand(Dictionary<Expression<Func<TModel, object>>, object> dict)
        {
            var type = typeof(TModel);
            var result = new StringBuilder();
            try
            {
                result.Append("SELECT * FROM ");
                result.AppendLine(type.Name);
                result.Append(" WHERE ");
                var arr = dict.ToArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    Expression expression = arr[i].Key.Body as UnaryExpression;
                    expression = expression ?? arr[i].Key.Body as MemberExpression;
                    var propName = (expression as MemberExpression).Member.Name;
                    result.Append(propName);
                    result.Append("=");
                    result.Append(GetContentForPropValue(arr[i]));
                    if (i + 1 < arr.Length)
                    {
                        result.Append(" AND ");
                    }
                    else
                    {
                        result.AppendLine(";");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Something wrong with id dictionary");
            }
            return result.ToString();
        }

        private string GetContentForPropValue(KeyValuePair<Expression<Func<TModel, object>>, object> pair)
        {
            Expression expression = pair.Key.Body as UnaryExpression;
            expression = expression ?? pair.Key.Body as MemberExpression;
            var propType = ((PropertyInfo)(expression as MemberExpression).Member).PropertyType;
            var typeCode = Type.GetTypeCode(propType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return (bool)pair.Value ? "1" : "0";
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.SByte:
                case TypeCode.UInt64:
                case TypeCode.Single:
                    return pair.Value.ToString();
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return $"'{pair.Value}'" ;
                default:
                    return pair.Value.ToString();
            }
        }

        private SqlTransaction CreateTransaction()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        private IEnumerable<TModel> ParseReader(SqlDataReader reader)
        {
            var result = new List<TModel>();

            while (reader.Read())
            {
                var obj = new TModel();
                var props = typeof(TModel).GetProperties().Where(prop => prop.GetCustomAttribute<ColumnPropertyAttribute>() != null);
                foreach (var prop in props)
                {
                    prop.SetValue(obj, reader[prop.Name]);
                }

                result.Add(obj);
            }

            return result;
        }

        #endregion Private Methods
    }
}