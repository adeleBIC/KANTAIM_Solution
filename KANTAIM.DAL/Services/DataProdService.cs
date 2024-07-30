using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class DataProdService
    {
        ProductService _productService;
        PressService _pressService;

        Repository<DataProd> _repo;
        public DataProdService(Repository<DataProd> _repo, ProductService productService, PressService pressService)
        {
            this._repo = _repo;
            _productService = productService;
            _pressService = pressService;
        }

        public IEnumerable<DataProd> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.DataProds.Include(c => c.Product).Include(c => c.Press).ToList();
        }
        public DataProd? GetById(int id) => GetAll().SingleOrDefault(c => c.Id == id);

        public void UpSert(DataProd item) => _repo.UpSert(item);
        public void Delete(int id) => _repo.Delete(id);

        public IEnumerable<Product> GetAllProducts() => _productService.GetAll();
        public IEnumerable<Press> GetAllPresses() => _pressService.GetAll();
        public Press? GetPressById(int id) => _pressService.GetById(id);
    }
}
