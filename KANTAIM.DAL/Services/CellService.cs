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
        Repository<Container> _repoContainer;

        public CellService(Repository<Cell> repo, Repository<Container> repoContainer)
        {
            this._repo = repo;
            _repoContainer = repoContainer;
        }

        public IEnumerable<Cell> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.Cells.Include(c => c.Containers).ToList();
        }
        public Cell? GetById(int? id)
        {
            using DataKANTAIMContext ctx = new();
            return id == null ? null : ctx.Cells.Include(c => c.Containers).SingleOrDefault(x => x.Id == id); ;
        }
        public Cell? GetByNumber(string n)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.Cells.Include(c => c.Containers).SingleOrDefault(x => x.Name == n);
        }
        public Cell? GetByXY(int X, int Y)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.Cells.Include(c => c.Containers).SingleOrDefault(x => x.X == X && x.Y == Y);
        }

        public int GetContainerCount(int cellId)
        {
            return _repoContainer.GetAll().Count(c => c.CellId == cellId && (c.ContainerTypeID == 1 || c.ContainerTypeID == 3));// conpte juste le contenaire normal et palette
        }

        public void Upsert(Cell item) => _repo.UpSert(item);
        public void Delete(int id) => _repo.Delete(id);

    }
}
