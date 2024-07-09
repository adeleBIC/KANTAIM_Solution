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
        private List<Cell> cache;
        Repository<Cell> _repo;
        Repository<Container> _repoContainer;

        public IEnumerable<Cell> Cache
        {
            get
            {
                if (cache == null)
                {
                    //cache = _repo.GetAll().ToList();
                    using DataKANTAIMContext ctx = new();
                    cache = ctx.Cells.Include(c => c.Containers).ToList();
                }

                return cache;
            }
        }

        public CellService(Repository<Cell> repo, Repository<Container> repoContainer)
        {
            this._repo = repo;
            _repoContainer = repoContainer;
        }

        public IEnumerable<Cell> GetAll() => Cache;
        public Cell? GetById(int? id) => id == null ? null : Cache.SingleOrDefault(x => x.Id == id);
        public Cell? GetByNumber(string n) => Cache.SingleOrDefault(x => x.Name == n);
        public Cell? GetByXY(int X, int Y) => Cache.SingleOrDefault(x => x.X == X && x.Y == Y);


        public int GetContainerCount(int cellId)
        {
            return _repoContainer.GetAll().Count(c => c.CellId == cellId && (c.ContainerTypeID == 1 || c.ContainerTypeID == 3));// conpte juste le contenaire normal et palette
        }
        public void ResetCache() => cache = null;
        public void Upsert(Cell item)
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
