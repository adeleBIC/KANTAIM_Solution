using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ContenaireService
    {
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

        public IEnumerable<Container> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.Containers.Include(c => c.BigContainer).Include(c => c.ContainerType).Include(c => c.CellStock).Include(c => c.ContainerAction).ToList();
        }
        public Container GetContainerById(int ContainerId) => GetAll().SingleOrDefault(u => u.ContainerID == ContainerId);
        public Container GetContainerByNumber(int n) => GetAll().FirstOrDefault(u => u.Number == n);
        public IEnumerable<Container> GetByCellId(int CellId) => GetAll().Where(u => u.CellId == CellId);
        public IEnumerable<Container> GetAllBacs(int paletteId) => GetAll().Where(u => u.ContainerID == paletteId);
        public int CountCells(int cellId) => GetAll().Where(u => u.CellId == cellId && !u.ContainerType.IsContainable).Count();
        public int CountCellsInJail(int cellId) => GetAll().Where(u => u.CellId == cellId && !u.ContainerType.IsContainable && u.InJail).Count();
        public int CountCellsInMaintenance(int cellId) => GetAll().Where(u => u.CellId == cellId && !u.ContainerType.IsContainable && u.InMaintenance).Count();

        public int CountBac(int paletteId)  => GetAll().Where(u => u.ContainerID == paletteId).Count();

        public void UpSert(Container item) => _repo.UpSert(item);

        public void Delete(int id) => _repo.Delete(id);

        public IEnumerable<Container> GetAllContainer() => _repoContainer.GetAll();
        public IEnumerable<ContainerType> GetAllContainerType() => _repoContainerType.GetAll();
        public IEnumerable<Cell> GetAllCell() => _repoCell.GetAll();
        public IEnumerable<ContainerAction> GetAllAction() => _repoAction.GetAll();


    }
}
