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

        public IEnumerable<Profil> Cache
        {
            get
            {
                if (cache == null)
                {
                    //cache = _repo.GetAll().ToList();
                    using DataKANTAIMContext ctx = new();
                    cache = ctx.Profils.Include(p=>p.RackProfils).ThenInclude(r=>r.Rack).ToList();
                }
                return cache;
            }
        }

        Repository<Profil> _repo;
        public ProfilService(Repository<Profil> repo, RackService rackService)
        {
            _repo = repo;
            _rackService = rackService;
        }

        public IEnumerable<Profil> GetAll() => Cache;
        public Profil? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(Profil item)
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
            using DataKANTAIMContext ctx = new();
            ctx.Entry(model).State = EntityState.Added;
            ctx.SaveChanges();
            return model.RackId;
        }

        public void UpdateRackProfil(RackProfil model)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(model).State = EntityState.Modified;
            ctx.SaveChanges();
        }

        public void DeleteRackProfil(int rId, int pId)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(new RackProfil() { RackId = rId, ProfilId = pId }).State = EntityState.Deleted;
            ctx.SaveChanges();
        }
        public IEnumerable<RackProfil> GetAllRackProfilByProfil(int id)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackProfils.Where(m => m.ProfilId == id).ToList();
        }

        public IEnumerable<Rack> GetAllRack() => _rackService.GetAll();

    }
}
