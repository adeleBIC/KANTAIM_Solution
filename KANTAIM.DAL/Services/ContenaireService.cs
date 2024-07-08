using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ContenaireService
    {
        private List<Container> cache;
        public IEnumerable<Container> Cache
        {
            get
            {
                if (cache == null)
                 {
                    cache = _repo.GetAll().ToList();
                    foreach (Container item in cache)
                    {
                        if (item.ContainerID.HasValue)
                            item.BigContainer = _repoContainer.GetById(item.ContainerID.Value);
                        else
                            item.ContainerID = null;
                        if (item.ContainerTypeID.HasValue)
                            item.ContainerType = _repoContainerType.GetById(item.ContainerTypeID.Value);
                        else
                            item.ContainerType = null;
                        if (item.CellId.HasValue)
                            item.CellStock = _repoCell.GetById(item.CellId.Value);
                        else
                            item.CellStock = null;
                        item.ContainerAction = _repoAction.GetById(item.ActionID);
                    }
                }
                return cache;
            }
        }

        Repository<Container> _repo;
        Repository<Container> _repoContainer;
        Repository<ContainerType> _repoContainerType;
        Repository<ContainerAction> _repoAction;
        Repository<Cell> _repoCell;
        public ContenaireService(Repository<Container> repo, Repository<Container> repoContainer, Repository<ContainerType> repoContainerType, Repository<Cell> repoCell, Repository<ContainerAction> repoAction)
        {
            _repo = repo;
            _repoContainer = repoContainer;
            _repoContainerType = repoContainerType;
            _repoAction = repoAction;
            _repoCell = repoCell;
        }

        public IEnumerable<Container> GetAll() => Cache;
        //public IEnumerable<Container> GetContainerById(int ContainerId) => Cache.Where(u => u.ContainerID == ContainerId);
        public IEnumerable<Container> GetContainerByNumber(int n) => Cache.Where(u => u.Number == n);
        public IEnumerable<Container> GetCellById(int CellId) => Cache.Where(u => u.CellId == CellId);
        public IEnumerable<Container> GetAllBacs(int paletteId) => Cache.Where(u => u.ContainerID == paletteId);
        public int CountCells(int cellId) => Cache.Where(u => u.CellId == cellId && u.ContainerType.Name != "Bac").Count();
        public int CountBac(int paletteId)  => Cache.Where(u => u.ContainerID == paletteId).Count();
        //public IEnumerable<Container> GetActionlById(int ActionId) => Cache.Where(u => u.ActionID == ActionId);
        //public Container? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);


        public void ResetCache() => cache = null;

        public void UpSert(Container item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }
        public IEnumerable<Container> GetAllContainer() => _repoContainer.GetAll();
        public IEnumerable<ContainerType> GetAllContainerType() => _repoContainerType.GetAll();
        public IEnumerable<Cell> GetAllCell() => _repoCell.GetAll();
        public IEnumerable<ContainerAction> GetAllAction() => _repoAction.GetAll();
        //public Container? GetFirstContainer() => _repoContainer.GetAll().OrderBy(l => l.Id).FirstOrDefault();
        //public ContainerType? GetFirstContainerType() => _repoContainerType.GetAll().OrderBy(l => l.Id).FirstOrDefault();
        //public Cell? GetFirstCell() => _repoCell.GetAll().OrderBy(l => l.Id).FirstOrDefault();

    }
}
