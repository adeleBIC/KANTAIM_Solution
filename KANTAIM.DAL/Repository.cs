using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL
{
    public class Repository<T> where T: class, IObject , new()
    {
        DevModeService _devModeService;
        private bool oldDevMode = false;

        public Repository(DevModeService devModeService)
        {
            _devModeService = devModeService;
        }
        public IEnumerable<T> Where(Func<T, bool> where)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Set<T>().Where(where).ToList();
        }

        public IEnumerable<T> GetAll()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
             return ctx.Set<T>().ToList();
        }

        public T GetById(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Set<T>().SingleOrDefault(t => t.Id == id);
        }

        public int Insert(T model)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(model).State = EntityState.Added;
            ctx.SaveChanges();
            return model.Id;
        }

        public void Update(T model)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(model).State = EntityState.Modified;
            ctx.SaveChanges();
        }

        public void Delete(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(new T() { Id = id }).State = EntityState.Deleted;
            ctx.SaveChanges();
        }

        public int UpSert(T model)
        {
            if (model.Id == 0) Insert(model);
            else Update(model);
            return model.Id;
        }
    }
}
