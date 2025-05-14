using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ProductFamilyService
    {
        private List<ProductFamily> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<ProductFamily> Cache
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

        Repository<ProductFamily> _repo;
        public ProductFamilyService(Repository<ProductFamily> _repo, DevModeService devModeService)
        {
            this._repo = _repo;
            _devModeService = devModeService;
        }

        public IEnumerable<ProductFamily> GetAll() => Cache;
        public ProductFamily? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(ProductFamily item)
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
