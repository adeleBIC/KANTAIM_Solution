using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class PressService
    {
        ShapeService _shapeService;
        WorkshopService _workshopService;
        private List<Press> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<Press> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    cache = _repo.GetAll().ToList();
                    foreach (Press item in cache)
                    {
                        item.Shape = _shapeService.GetById(item.ShapeID);
                        item.Workshop = _workshopService.GetById(item.WorkshopID);
                    }
                    oldDevMode = _devModeService.DevMode;
                }
                
                return cache;
            }
        }

        Repository<Press> _repo;
        public PressService(Repository<Press> _repo, ShapeService shapeService, WorkshopService workshopService, DevModeService devModeService)
        {
            this._repo = _repo;
            _shapeService = shapeService;
            _workshopService = workshopService;
            _devModeService = devModeService;
        }


        public IEnumerable<Press> GetAll() => Cache;
        public IEnumerable<Press> GetAllPerWorkshop(int id) => Cache.Where(p=>p.WorkshopID == id);
        public Press? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public Press? GetByNumber(int id) => Cache.SingleOrDefault(c => c.Number == id);
        public void ResetCache() => cache = null;

        public void UpSert(Press item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }
        public IEnumerable<Shape> GetAllShapes() => _shapeService.GetAll();
        public IEnumerable<Workshop> GetAllWorkshops() => _workshopService.GetAll();
    }
}
