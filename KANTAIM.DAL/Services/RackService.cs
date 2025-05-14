using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class RackService
    {
        WorkshopService _workshopService;
        private List<Rack> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<Rack> Cache
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

        Repository<Rack> _repo;
        public RackService(Repository<Rack> repo, WorkshopService workshopService, DevModeService devModeService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _devModeService = devModeService;
        }

        public IEnumerable<Rack> GetAll() => Cache;
        public Rack? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(Rack item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }

        public int InsertRackProfil(RackProfil model)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(model).State = EntityState.Added;
            ctx.SaveChanges();
            return model.RackId;
        }

        public void UpdateRackProfil(RackProfil model)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(model).State = EntityState.Modified;
            ctx.SaveChanges();
        }

        public void DeleteRackProfil(int rId, int pId)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(new RackProfil() { RackId = rId, ProfilId = pId }).State = EntityState.Deleted;
            ctx.SaveChanges();
        }
        public IEnumerable<RackProfil> GetAllRackProfilByRack(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.RackProfils.Where(m => m.RackId == id).ToList();
        }

        public IEnumerable<Workshop> GetAllWorkshops()
        {
            return _workshopService.GetAll().ToList();
        }
    }
}
