using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ShapeService
    {
        ProductService _productService;
        private List<Shape> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<Shape> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    cache = _repo.GetAll().ToList();
                    oldDevMode = _devModeService.DevMode;
                    foreach (Shape item in cache)
                    {
                        item.Product = _productService.GetById(item.ProductID);
                    }
                }

                return cache;
            }
        }

        Repository<Shape> _repo;
        public ShapeService(Repository<Shape> _repo, ProductService productService, DevModeService devModeService)
        {
            this._repo = _repo;
            _productService = productService;
            _devModeService = devModeService;
        }

        public IEnumerable<Shape> GetAll() => Cache;
        public Shape? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(Shape item)
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
    }
}
