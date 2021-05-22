using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EOrm.Interfaces
{
    public interface IRepository<TModel> where TModel : class, IEntity, new()
    {
        void CommitChanges();

        void Create(TModel model);

        void DeleteById(int id);

        void Delete(TModel model);

        void DiscardChandes();

        IEnumerable<TModel> GetAll();

        TModel GetByField(Dictionary<Expression<Func<TModel, object>>, object> dict);

        void Update(TModel model);
    }
}