using Azure;
using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class LogService
    {
        private List<Log> cache;
        Repository<Log> _repo;
        Repository<Product> _repoProduct;
        Repository<Container> _repoContainer;
        Repository<ContainerType> _repoContainerType;
        Repository<Press> _repoPress;
        Repository<Shape> _repoShape;
        Repository<Machine> _repoMachine;
        Repository<ProdColor> _repoColor;
        Repository<Cell> _repoCell;
       
        public LogService(Repository<Log> repo, Repository<Product> repoProduct, Repository<Container> repoContainer, Repository<Press> repoPress, Repository<Shape> repoShape, Repository<Machine> repoMachine, Repository<ProdColor> repoColor, Repository<Cell> repoCell)
        {
            _repo = repo;
            _repoProduct = repoProduct;
            _repoContainer = repoContainer;
            _repoPress = repoPress;
            _repoShape = repoShape;
            _repoMachine = repoMachine;
            _repoColor = repoColor;
            _repoCell = repoCell;
        }
        public IEnumerable<Log> Cache
        {
            get
            {
                if (cache == null)
                {
                    cache = _repo.GetAll().ToList();
                    foreach (Log item in cache)
                    {
                        if (item.ProductID.HasValue)
                            item.ProductID = _repoProduct.GetById(item.ProductID.Value).Id;
                        else
                            item.ProductID = null;
                        if (item.PressID.HasValue)
                            item.PressID = _repoPress.GetById(item.PressID.Value).Id;
                        else
                            item.PressID = null;
                        if (item.ShapeID.HasValue)
                            item.ShapeID = _repoShape.GetById(item.ShapeID.Value).Id;
                        else
                            item.ShapeID = null;
                        if (item.CellID.HasValue)
                            item.CellID = _repoCell.GetById(item.CellID.Value).Id;
                        else
                            item.CellID = null;
                        if (item.ProdColorID.HasValue)
                            item.ProdColorID = _repoColor.GetById(item.ProdColorID.Value).Id;
                        else
                            item.ProdColorID = null;
                        if (item.MachineID.HasValue)
                            item.MachineID = _repoMachine.GetById(item.MachineID.Value).Id;
                        else
                            item.MachineID = null;
                        item.Container = _repoContainer.GetById(item.ContainerID);
                        item.Container.ContainerType = _repoContainerType.GetById(item.Container.ContainerTypeID);
                    }
                }
                return cache;
            }
        }

        public IEnumerable<Log> GetAll() => Cache;
        public void ResetCache() => cache = null;
        public void UpSert(Log item)
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
        public IEnumerable<Product> GetAllProduct() => _repoProduct.GetAll();
        public IEnumerable<Cell> GetAllCell() => _repoCell.GetAll();
        public IEnumerable<Press> GetAllPress() => _repoPress.GetAll();
        public IEnumerable<Shape> GetAllShape() => _repoShape.GetAll();
        public IEnumerable<ProdColor> GetAllColor() => _repoColor.GetAll();
        public IEnumerable<Machine> GetAllMachine() => _repoMachine.GetAll();

        public Log? GetByContenaireId(int id) => Cache.OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == id);
        public Log? GetByContenaireByActionId(int contId, int opeId) => Cache.Where(u=>u.Operation == opeId).OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == contId);
    }
}
