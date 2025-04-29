using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class RackProfilService
    {
        public RackProfilService()
        {

        }
        public int Insert(RackProfil model)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(model).State = EntityState.Added;
            ctx.SaveChanges();
            return model.RackId;
        }

        public void Update(RackProfil model)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(model).State = EntityState.Modified;
            ctx.SaveChanges();
        }

        public void Delete(int rId, int pId)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(new RackProfil() { RackId = rId, ProfilId = pId }).State = EntityState.Deleted;
            ctx.SaveChanges();
        }
        public IEnumerable<RackProfil> GetAllRackProfilByRack(int id)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackProfils.Where(m => m.RackId == id).ToList();
        }

        public IEnumerable<RackProfil> GetAllRackProfilByProfil(int id)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackProfils.Where(m => m.ProfilId == id).ToList();
        }

        public IEnumerable<RackProfil> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackProfils.ToList();
        }

        public RackProfil? GetById(int rId, int pId)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackProfils.SingleOrDefault(t => t.RackId == rId && t.ProfilId == pId); ;
        }
    }
}
