using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        T Get(Expression<Func<T, bool>> filter);
        IEnumerable<T> GetAll();
        void Remove(T entity);
        void Add(T entity);
        void RemoveRange(IEnumerable<T> entity);
    }
}
