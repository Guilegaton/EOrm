using EOrm.DBModels;
using EOrm.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EOrm.Repositories.EF
{
    public class EFRepository<TModel> : IRepository<TModel>, IDisposable where TModel : class, IEntity, new()
    {
        #region Private Fields

        private ShipmentEntities _context;
        private DbSet<TModel> _dbSet;
        private DbContextTransaction _transaction;

        #endregion Private Fields

        #region Public Constructors

        public EFRepository()
        {
            _context = new ShipmentEntities();
            _dbSet = _context.Set<TModel>();
        }

        #endregion Public Constructors

        #region Public Methods

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

            _dbSet.Add(model);
            _context.SaveChanges();
        }

        public void Delete(TModel model)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }

            _dbSet.Remove(model);
            _context.SaveChanges();
        }

        public void DeleteById(int id)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }

            var model = _dbSet.Find(id);
            _dbSet.Remove(model);
            _context.SaveChanges();
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
            _context.Dispose();
        }

        public IEnumerable<TModel> GetAll()
        {
            return _dbSet.ToArray();
        }

        public TModel GetByField(Dictionary<Expression<Func<TModel, object>>, object> dict)
        {
            var result = _dbSet.SqlQuery(CreateGetCommand(dict)).First();

            return result;
        }

        public void Update(TModel model)
        {
            if (_transaction == null)
            {
                CreateTransaction();
            }

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();
        }

        #endregion Public Methods

        #region Private Methods

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

        private void CreateTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
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
                    return $"'{pair.Value}'";

                default:
                    return pair.Value.ToString();
            }
        }

        #endregion Private Methods
    }
}