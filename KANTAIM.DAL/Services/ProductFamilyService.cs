using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ProductFamilyService
    {
        private List<ProductFamily> cache;

        public IEnumerable<ProductFamily> Cache
        {
            get
            {
                if (cache == null) cache = _repo.GetAll().ToList();
                return cache;
            }
        }

        Repository<ProductFamily> _repo;
        public ProductFamilyService(Repository<ProductFamily> _repo)
        {
            this._repo = _repo;
        }

        public IEnumerable<ProductFamily> GetAll() => Cache;
        public ProductFamily? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(ProductFamily item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }
    }
}
