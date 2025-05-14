using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ProductService
    {
        ProductFamilyService _productFamilyService;
        private List<Product> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<Product> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    cache = _repo.GetAll().ToList();
                    oldDevMode = _devModeService.DevMode;
                }
                return cache;
            }
        }

        Repository<Product> _repo;
        public ProductService(Repository<Product> _repo, ProductFamilyService productFamilyService, DevModeService devModeService)
        {
            this._repo = _repo;
            _productFamilyService = productFamilyService;
            _devModeService = devModeService;
        }

        public IEnumerable<Product> GetAll() => Cache;
        public Product? GetByNumber(int Number) => Cache.SingleOrDefault(c => c.Number == Number);
        public IEnumerable<Product> GetAllPerProductFamily(int id) => Cache.Where(p => p.ProductFamilyID == id);
        public Product? GetById(int? id) => id == null ? null : Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(Product item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }
        public IEnumerable<ProductFamily> GetAllProductFamily() => _productFamilyService.GetAll();

    }
}
