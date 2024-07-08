using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ActionService
    {
        private List<ContainerAction> cache;
        Repository<ContainerAction> _repo;
        public IEnumerable<ContainerAction> Cache
        {
            get
            {
                if (cache == null)
                {

                    cache = _repo.GetAll().ToList();
                }
                return cache;
            }
        }


        public ActionService(Repository<ContainerAction> repo)
        {
            this._repo = repo;
        }

        public IEnumerable<ContainerAction> GetAll() => Cache;
        public ContainerAction? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(ContainerAction item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }
    }
}
