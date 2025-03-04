using Azure;
using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<Log> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.Logs.Include(c => c.Product).Include(c => c.Press).Include(c => c.Shape).Include(c => c.Cell).Include(c => c.ProdColor).Include(c => c.Machine).Include(c => c.Container).ThenInclude(x=>x.ContainerType).ToList();
        }
        public void UpSert(Log item) => _repo.UpSert(item);

        public void Delete(int id) => _repo.Delete(id);
        public IEnumerable<Container> GetAllContainer() => _repoContainer.GetAll();
        public IEnumerable<Product> GetAllProduct() => _repoProduct.GetAll();
        public IEnumerable<Cell> GetAllCell() => _repoCell.GetAll();
        public IEnumerable<Press> GetAllPress() => _repoPress.GetAll();
        public IEnumerable<Shape> GetAllShape() => _repoShape.GetAll();
        public IEnumerable<ProdColor> GetAllColor() => _repoColor.GetAll();
        public IEnumerable<Machine> GetAllMachine() => _repoMachine.GetAll();

        public Log? GetByContenaireId(int id) => GetAll().OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == id);
        public Log? GetByContenaireIdAction(int id, int operation) => GetAll().OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == id && c.Operation == operation);
        public Log? GetByContenaireNumber(int number) => GetAll().OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.Container.Number == number);
        public Log? GetByContenaireByOperationStatus(int contId, int opeStatus) => GetAll().Where(u=>u.Operation == opeStatus).OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == contId);
        public Log? GetByCell(int cellId) => GetAll().Where(u => u.CellID == cellId && u.Container.CellId == cellId).OrderByDescending(u => u.EventTime).FirstOrDefault();
    }
}
