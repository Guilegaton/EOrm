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

namespace EOrm.Repositories.ADO
{
    public class AdoRepositoryDisconnectedAproach<TModel> : IRepository<TModel>, IDisposable where TModel : class, IEntity, new()
    {
        #region Public Constructors

        public AdoRepositoryDisconnectedAproach(string connectionString)
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

            _dataSet = new DataSet();
        }

        #endregion Public Constructors

        #region Public Properties

        public SqlConnection _connection { get; private set; }
        public SqlTransaction _transaction { get; private set; }
        public DataSet _dataSet { get; private set; }

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
            var type = typeof(TModel);

            if (_dataSet.Tables.Contains(type.Name))
            {
                _dataSet.Tables[type.Name].Clear();
            }
            else
            {
                _dataSet.Tables.Add(type.Name);
            }

            var selectAdapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            selectAdapter.SelectCommand.Transaction = transaction;

            selectAdapter.Fill(_dataSet.Tables[type.Name]);

            var dataSet = AddRow(model);

            var adapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            adapter.SelectCommand.Transaction = _transaction;
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.InsertCommand.Transaction = _transaction;
            adapter.Update(dataSet, type.Name);
        }

        public void Delete(TModel model)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }
            var type = typeof(TModel);

            var selectAdapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            selectAdapter.SelectCommand.Transaction = transaction;

            selectAdapter.Fill(_dataSet.Tables[type.Name]);

            var dataSet = RemoveRow(model.Id);

            var adapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            adapter.SelectCommand.Transaction = _transaction;
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.UpdateCommand.Transaction = _transaction;
            adapter.Update(dataSet);
        }

        public void DeleteById(int id)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }
            var type = typeof(TModel);

            var selectAdapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            selectAdapter.SelectCommand.Transaction = transaction;

            selectAdapter.Fill(_dataSet.Tables[type.Name]);

            var dataSet = RemoveRow(id);

            var adapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            adapter.SelectCommand.Transaction = _transaction;
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.UpdateCommand.Transaction = _transaction;
            adapter.Update(dataSet);
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
            var type = typeof(TModel);

            if (_dataSet.Tables.Contains(type.Name))
            {
                _dataSet.Tables[type.Name].Clear();
            }
            else
            {
                _dataSet.Tables.Add(type.Name);
            }

            var adapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            adapter.SelectCommand.Transaction = transaction;

            adapter.Fill(_dataSet.Tables[type.Name]);

            var datarows = _dataSet.Tables[type.Name];

            return ParseReader(datarows.Rows);
        }

        public TModel GetByField(Dictionary<Expression<Func<TModel, object>>, object> dict)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }
            var type = typeof(TModel);

            if (_dataSet.Tables.Contains(type.Name))
            {
                _dataSet.Tables[type.Name].Clear();
            }
            else
            {
                _dataSet.Tables.Add(type.Name);
            }

            var adapter = new SqlDataAdapter(CreateGetCommand(dict), _connection);
            adapter.SelectCommand.Transaction = transaction;

            adapter.Fill(_dataSet.Tables[type.Name]);

            var datarows = _dataSet.Tables[type.Name];

            return ParseReader(datarows.Rows).FirstOrDefault();
        }

        public void Update(TModel model)
        {
            var transaction = _transaction;
            if (transaction == null)
            {
                transaction = CreateTransaction();
            }
            var type = typeof(TModel);

            var selectAdapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            selectAdapter.SelectCommand.Transaction = transaction;

            selectAdapter.Fill(_dataSet.Tables[type.Name]);

            var row = UpdateRow(model);

            var adapter = new SqlDataAdapter(CreateGetAllCommand(), _connection);
            adapter.SelectCommand.Transaction = _transaction;
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.UpdateCommand.Transaction = _transaction;
            adapter.Update(new DataRow[] { row });
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

        private SqlTransaction CreateTransaction()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        private IEnumerable<TModel> ParseReader(DataRowCollection rows)
        {
            var result = new List<TModel>();
            for (int i = 0; i < rows.Count; i++)
            {
                var obj = new TModel();
                var props = typeof(TModel).GetProperties().Where(prop => prop.GetCustomAttribute<ColumnPropertyAttribute>() != null);
                foreach (var prop in props)
                {
                    prop.SetValue(obj, rows[i][prop.Name]);
                }

                result.Add(obj);
            }

            return result;
        }

        private DataRow UpdateRow(TModel model)
        {
            var primaryKeyProp = typeof(TModel).GetProperties().First(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            var row = _dataSet.Tables[typeof(TModel).Name].Select($"{primaryKeyProp.Name}={primaryKeyProp.GetValue(model)}").First();
            var props = typeof(TModel).GetProperties().Where(prop => prop.GetCustomAttribute<ColumnPropertyAttribute>() != null);
            foreach (var prop in props)
            {
                row[prop.Name] = prop.GetValue(model);
            }

            return row;
        }

        private DataSet RemoveRow(int id)
        {
            var primaryKeyProp = typeof(TModel).GetProperties().First(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            var row = _dataSet.Tables[typeof(TModel).Name].Select($"{primaryKeyProp.Name}={id}").First();
            _dataSet.Tables[typeof(TModel).Name].Rows.Remove(row);
            return _dataSet;
        }


        private DataSet AddRow(TModel model)
        {
            var primaryKeyProp = typeof(TModel).GetProperties().First(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            var row = _dataSet.Tables[typeof(TModel).Name].NewRow();
            var props = typeof(TModel).GetProperties().Where(prop => prop.GetCustomAttribute<ColumnPropertyAttribute>() != null);
            foreach (var prop in props)
            {
                row[prop.Name] = prop.GetValue(model);
            }
            _dataSet.Tables[typeof(TModel).Name].Rows.Add(row);

            return _dataSet;
        }

        #endregion Private Methods
    }
}
