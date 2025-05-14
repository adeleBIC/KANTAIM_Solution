using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class CellService
    {
        Repository<Cell> _repo;
        //Repository<Workshop> _repoWorkshop;
        RackService _rackService;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public CellService(Repository<Cell> repo, RackService rackService, DevModeService devModeService)
        {
            this._repo = repo;
            //_repoWorkshop = repoWorkshop;
            _rackService = rackService;
            _devModeService = devModeService;
        }

        public IEnumerable<Cell> GetAll()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Cells.Include(c => c.Containers).ThenInclude(c => c.ContainerType)
                            .Include(c=>c.RackCells).ThenInclude(r=>r.Rack)
                            .ToList();
        }

        //public IEnumerable<Workshop> GetAllWorkshop() => _repoWorkshop.GetAll();
        public Cell? GetById(int? id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return id == null ? null : ctx.Cells.Include(c => c.Containers).ThenInclude(c => c.ContainerType)
                                                .Include(c => c.RackCells).ThenInclude(r => r.Rack)
                                                .SingleOrDefault(x => x.Id == id); ;
        }
        public Cell? GetByNumber(string n)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Cells.Include(c => c.Containers).ThenInclude(c => c.ContainerType)
                            .Include(c => c.RackCells).ThenInclude(r => r.Rack)
                            .SingleOrDefault(x => x.Name == n);
        }

        
        public Cell? GetByXY(int X, int Y)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Cells.Include(c => c.Containers).ThenInclude(c => c.ContainerType)
                            .Include(c => c.RackCells).ThenInclude(r => r.Rack)
                            .SingleOrDefault(x => x.X == X && x.Y == Y);
        }

        //public Workshop? GetFirstWorkshop() => _repoWorkshop.GetAll().OrderBy(l => l.Id).FirstOrDefault();

        public int GetContainerCount(int cellId)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Containers.Include(c=>c.ContainerType).Count(c => c.CellID == cellId && !c.ContainerType.IsContainable);// conpte juste le contenaire normal et palette
        }

        public List<Cell> GetByWorkshop(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Cells
                      .Include(c => c.Containers).ThenInclude(c => c.ContainerType)
                      .Include(c => c.RackCells)
                          .ThenInclude(rc => rc.Rack)
                      .Where(c => c.RackCells.Any(rc => rc.Rack.WorkshopID == id))
                      .ToList();
        }

        public int Upsert(Cell item) => _repo.UpSert(item);
        public void Delete(int id) => _repo.Delete(id);

        public int InsertRackCell(RackCell model)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(model).State = EntityState.Added;
            ctx.SaveChanges();
            return model.RackId;
        }

        public void UpdateRackCell(RackCell model)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(model).State = EntityState.Modified;
            ctx.SaveChanges();
        }

        public void DeleteRackCell(int rId, int cId)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            ctx.Entry(new RackCell() { RackId = rId, CellId = cId }).State = EntityState.Deleted;
            ctx.SaveChanges();
        }
        public IEnumerable<RackCell> GetAllRackCellByCell(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.RackCells.Where(m => m.CellId == id).ToList();
        }

        public IEnumerable<Rack> GetAllRack() => _rackService.GetAll();

    }
}
