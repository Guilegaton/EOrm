using Dapper;
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
using System.Threading.Tasks;

namespace EOrm.Repositories.Dapper
{
    public class DapperRepository<TModel> : IRepository<TModel> where TModel : class, IEntity, new()
    {
        public IDbConnection _connection { get; set; }
        public IDbTransaction _transaction { get; private set; }

        public DapperRepository(string connectionString)
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

        public void CommitChanges()
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public void Create(TModel model)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }
            _connection.Execute(model.GetCreateCommand(), transaction: _transaction);
        }

        public void Delete(TModel model)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }
            _connection.Execute(model.GetDeleteCommand(), transaction: _transaction);
        }

        public void DeleteById(int id)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }
            _connection.Execute(CreateDeleteCommand(id), transaction: _transaction);
        }

        public void DiscardChandes()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public IEnumerable<TModel> GetAll()
        {
            var result = _connection.Query<TModel>(CreateGetAllCommand());

            return result;
        }

        public TModel GetByField(Dictionary<Expression<Func<TModel, object>>, object> dict)
        {
            var result = _connection.QueryFirst<TModel>(CreateGetCommand(dict));

            return result;
        }

        public void Update(TModel model)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }

            _connection.Execute(model.GetUpdateCommand(), transaction: _transaction);
        }

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
                    var propName = ((arr[i].Key.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
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
            var propType = ((pair.Key.Body as UnaryExpression).Operand as MemberExpression).Member.DeclaringType;
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
                    return $"'{pair}'";
                default:
                    return pair.Value.ToString();
            }
        }

        private void CreateTransaction()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
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
    }
}
