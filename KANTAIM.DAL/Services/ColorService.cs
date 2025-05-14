using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ColorService
    {
        private List<ProdColor> cache;
        Repository<ProdColor> _repo;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<ProdColor> Cache
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

        public ColorService(Repository<ProdColor> repo, DevModeService devModeService)
        {
            this._repo = repo;
            _devModeService = devModeService;
        }

        public IEnumerable<ProdColor> GetAll() => Cache;
        public ProdColor? GetById(int? id) => id == null ? null : Cache.SingleOrDefault(x => x.Id == id);
        public void ResetCache() => cache = null;
        public void Upsert(ProdColor item) 
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
