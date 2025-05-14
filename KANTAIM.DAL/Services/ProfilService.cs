using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ProfilService
    {
        RackService _rackService;
        private List<Profil> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;

        public IEnumerable<Profil> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    //cache = _repo.GetAll().ToList();
                    using DataKANTAIMContext ctx = new(_devModeService.DevMode);
                    cache = ctx.Profils.Include(p=>p.RackProfils).ThenInclude(r=>r.Rack).ToList();
                    oldDevMode = _devModeService.DevMode;
                }
                return cache;
            }
        }

        Repository<Profil> _repo;
        public ProfilService(Repository<Profil> repo, RackService rackService, DevModeService devModeService)
        {
            _repo = repo;
            _rackService = rackService;
            _devModeService = devModeService;
        }

        public IEnumerable<Profil> GetAll() => Cache;
        public Profil? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public int UpSert(Profil item)
        {
            ResetCache();
            return _repo.UpSert(item);
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
        public IEnumerable<RackProfil> GetAllRackProfilByProfil(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.RackProfils.Where(m => m.ProfilId == id).ToList();
        }

        public IEnumerable<Rack> GetAllRack() => _rackService.GetAll();

    }
}
