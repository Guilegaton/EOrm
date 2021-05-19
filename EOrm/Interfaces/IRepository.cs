using System.Collections.Generic;

namespace EOrm.Interfaces
{
    public interface IRepository<TModel> where TModel : class, new()
    {

        void Create(TModel model);
        void DeleteById(int id);
        IEnumerable<TModel> GetAll();
        TModel GetById(int id);
        void UpdateById(TModel model);

    }
}