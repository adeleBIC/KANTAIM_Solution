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
        Repository<ProdColor> _repoColor;
        Repository<Product> _repoProduct;
        Repository<Press> _repoPress;
        Repository<Machine> _repoMachine;
        public ContenaireService(Repository<Container> repo, Repository<Container> repoContainer, Repository<ContainerType> repoContainerType, Repository<Cell> repoCell, Repository<ContainerAction> repoAction, Repository<ProdColor> repoColor, Repository<Product> repoProduct, Repository<Press> repoPress, Repository<Machine> repoMachine)
        {
            _repo = repo;
            _repoContainer = repoContainer;
            _repoContainerType = repoContainerType;
            _repoAction = repoAction;
            _repoCell = repoCell;
            _repoColor = repoColor;
            _repoProduct = repoProduct;
            _repoPress = repoPress;
            _repoMachine = repoMachine;
        }

        public IEnumerable<Container> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.Containers.Include(c => c.BigContainer)
                                    .Include(c => c.ContainerType)
                                    .Include(c => c.CellStock).ThenInclude(rc=>rc.RackCells).ThenInclude(r => r.Rack)
                                    .Include(c => c.ContainerAction)
                                    .Include(c => c.Product)
                                    .Include(c => c.ProdColor)
                                    .Include(c => c.Press).ThenInclude(p=>p.Shape)
                                    .Include(c => c.Machine)
                                    .ToList();
        }
        public IEnumerable<Container> GetAllByOperationStatus(int status1) => GetAll().Where(u => u.CellStock != null && u.ContainerAction.Status == status1).ToList();
        public IEnumerable<Container> GetAllByOperationStatus(int status1, int status2) => GetAll().Where(u => u.CellStock != null && (u.ContainerAction.Status == status1 || u.ContainerAction.Status == status2)).ToList();
        public Container GetContainerById(int ContainerId) => GetAll().SingleOrDefault(u => u.ContainerID == ContainerId);
        public Container GetContainerByNumber(int n) => GetAll().FirstOrDefault(u => u.Number == n);
        public Container? GetByIdByOperationStatus(int contId, int status1) => GetAll().Where(u => u.ContainerID == contId && u.CellStock != null && u.ContainerAction.Status == status1).FirstOrDefault();
        public Container? GetByIdByOperationStatus(int contId, int status1, int status2) => GetAll().Where(u => u.ContainerID == contId && u.CellStock != null && (u.ContainerAction.Status == status1 || u.ContainerAction.Status == status2)).FirstOrDefault();
        public IEnumerable<Container> GetByCellId(int CellId) => GetAll().Where(u => u.CellID == CellId );
        public IEnumerable<Container> GetAllBacs(int paletteId) => GetAll().Where(u => u.ContainerID == paletteId);
        public int CountCells(int cellId) => GetAll().Where(u => u.CellID == cellId && !u.ContainerType.IsContainable).Count();
        public int CountCellsXY(Cell cell) => GetAll().Where(u => u.CellStock.X == cell.X && u.CellStock.Y == cell.Y).Count();
        public int CountCellsInJail(int cellId) => GetAll().Where(u => u.CellID == cellId && !u.ContainerType.IsContainable && u.InJail).Count();
        public int CountCellsInMaintenance(int cellId) => GetAll().Where(u => u.CellID == cellId && !u.ContainerType.IsContainable && u.InMaintenance).Count();

        public int CountBac(int paletteId)  => GetAll().Where(u => u.ContainerID == paletteId).Count();

        public void UpSert(Container item) => _repo.UpSert(item);

        public void Delete(int id) => _repo.Delete(id);

        //public IEnumerable<Container> GetAllContainer() => _repoContainer.GetAll();
        public IEnumerable<ContainerType> GetAllContainerType() => _repoContainerType.GetAll();
        public IEnumerable<Cell> GetAllCell() => _repoCell.GetAll().OrderBy(c=>c.Name);
        public IEnumerable<ContainerAction> GetAllAction() => _repoAction.GetAll().OrderBy(c => c.Status);
        public IEnumerable<ProdColor> GetAllColor() => _repoColor.GetAll().OrderBy(c => c.Name);
        public IEnumerable<Product> GetAllProd() => _repoProduct.GetAll().OrderBy(c => c.Name);
        public IEnumerable<Press> GetAllPress() => _repoPress.GetAll().OrderBy(c => c.Number);
        public IEnumerable<Machine> GetAllMachine() => _repoMachine.GetAll().OrderBy(c => c.Name);
    }
}
