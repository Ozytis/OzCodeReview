using DataAccess;

using Ozytis.Common.Core.Managers;

namespace Business
{
    public class BaseEntityManager<T> : BaseEntityManager<DataContext, T> where T : class
    {
        public BaseEntityManager(DataContext dataContext):base(new ManagerOptions { UseContextScopes=false}, dataContext)
        {
            this.DataContext.ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}