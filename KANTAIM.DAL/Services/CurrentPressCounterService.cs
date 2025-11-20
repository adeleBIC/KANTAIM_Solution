using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class CurrentPressCounterService
    {
        Repository<CurrentPressCounter> _repo;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public CurrentPressCounterService(Repository<CurrentPressCounter> _repo, DevModeService devModeService)
        {
            this._repo = _repo;
            _devModeService = devModeService;
        }

        public IEnumerable<CurrentPressCounter> GetAll()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.CurrentPressCounters.Include(c => c.Press).ToList();
        }
        public CurrentPressCounter? GetById(int id) => GetAll().SingleOrDefault(c => c.Id == id);
        public CurrentPressCounter? GetByPressId(int id) => GetAll().SingleOrDefault(c => c.PressID == id);

        public void UpSert(CurrentPressCounter item) => _repo.UpSert(item);
        public void Delete(int id) => _repo.Delete(id);

        public IEnumerable<Press> GetAllPressInclude()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Press.Include(c => c.Shape).ThenInclude(C=>C.Product).ToList();
        }

        public IEnumerable<Press> GetAllPressPerWorkshopInclude(int wsId)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Press.Where(w=>w.WorkshopID == wsId).Include(c => c.Shape).ThenInclude(C => C.Product).ToList();
        }

        public Press GetPressByIdInclude(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Press.Include(c => c.Shape).ThenInclude(C => C.Product).SingleOrDefault(p => p.Id == id);
        }

        public IEnumerable<Workshop> GetAllWorkshop()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Workshops.ToList();
        }
    }
}
