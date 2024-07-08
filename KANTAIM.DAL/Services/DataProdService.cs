using KANTAIM.DAL.Model;
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
        private List<DataProd> cache;

        public IEnumerable<DataProd> Cache
        {
            get
            {
                if (cache == null) cache = _repo.GetAll().ToList();
                foreach (DataProd item in cache)
                {
                    item.Product = _productService.GetById(item.ProductID);
                    item.Press = _pressService.GetById(item.PressID);
                }
                return cache;
            }
        }

        Repository<DataProd> _repo;
        public DataProdService(Repository<DataProd> _repo, ProductService productService, PressService pressService)
        {
            this._repo = _repo;
            _productService = productService;
            _pressService = pressService;
        }

        public IEnumerable<DataProd> GetAll() => Cache;
        public DataProd? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(DataProd item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }

        public IEnumerable<Product> GetAllProducts() => _productService.GetAll();
        public IEnumerable<Press> GetAllPresses() => _pressService.GetAll();
        public Press? GetPressById(int id) => _pressService.GetById(id);
    }
}
