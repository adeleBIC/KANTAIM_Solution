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
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<ContainerAction> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    cache = _repo.GetAll().ToList();
                    oldDevMode = _devModeService.DevMode;
                }
                return cache;
            }
        }


        public ActionService(Repository<ContainerAction> repo, DevModeService devModeService)
        {
            this._repo = repo;
            _devModeService = devModeService;
        }

        public IEnumerable<ContainerAction> GetAll() => Cache;
        public ContainerAction? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public ContainerAction? GetByStatus(int status) => Cache.SingleOrDefault(c => c.Status == status);
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
