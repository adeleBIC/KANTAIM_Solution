using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ContenaireTypeService
    {
        private List<ContainerType> cache;
        Repository<ContainerType> _repo;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<ContainerType> Cache
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

        
        public ContenaireTypeService(Repository<ContainerType> repo, DevModeService devModeService)
        {
            this._repo = repo;
            _devModeService = devModeService;
        }

        public IEnumerable<ContainerType> GetAll() => Cache;
        public ContainerType? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(ContainerType item)
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
