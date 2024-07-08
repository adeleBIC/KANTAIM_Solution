using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ColorService
    {
        private List<ProdColor> cache;
        Repository<ProdColor> _repo;

        public IEnumerable<ProdColor> Cache
        {
            get
            {
                if(cache == null)
                {
                    cache = _repo.GetAll().ToList();
                }
                    
                return cache;
            }
        }

        public ColorService(Repository<ProdColor> repo)
        {
            this._repo = repo;
        }

        public IEnumerable<ProdColor> GetAll() => Cache;
        public ProdColor? GetById(int? id) => id == null ? null : Cache.SingleOrDefault(x => x.Id == id);
        public void ResetCache() => cache = null;
        public void Upsert(ProdColor item) 
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
