using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class MachineService
    {
        private List<Machine> cache;
        Repository<Machine> _repo;
        Repository<Product> _products;
        ProductService _productService;
        public IEnumerable<Machine> Cache
        {
            get
            {
                if (cache == null)
                {
                    cache = _repo.GetAll().ToList();
  
                    foreach (Machine item in cache)
                    {
                        if(item.ProductID != null)
                        {
                            item.Product = _productService.GetById(item.ProductID);
                        }
                        
                    }
     
                }
                return cache;
            }
        }

        
        public MachineService(Repository<Machine> repo, Repository<Product> repoProductId, ProductService productService)
        {
            _repo = repo;
            _productService = productService;
            _products = repoProductId;
        }

        public IEnumerable<Machine> GetAll() => Cache;
        public IEnumerable<Machine> GetByProdId(int prodId) => Cache.Where(u => u.ProductID == prodId);
        public IEnumerable<Product> GetAllProducts() => _productService.GetAll();
        public Machine? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public Machine? GetByNumber(int n) => Cache.SingleOrDefault(c => c.Number == n);

        public void ResetCache() => cache = null;

        public void UpSert(Machine item)
        {
            ResetCache();
            _repo.UpSert(item);
        }
        
        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }

        public IEnumerable<Product> GetAllProd() => _products.GetAll();
        public Product? GetFirstProdId() => _products.GetAll().OrderBy(l => l.Id).FirstOrDefault();
    }
}
