using System.Collections.Generic;

namespace EOrm.Interfaces
{
    public interface IRepository<TModel> where TModel : class, new()
    {
        void CommitChanges();

        void Create(TModel model);

        void DeleteById(int id);

        void DiscardChandes();

        IEnumerable<TModel> GetAll();

        TModel GetById(int id);

        void UpdateById(TModel model);
    }
}