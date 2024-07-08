using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class WorkshopService
    {
        private List<Workshop> cache;

        public IEnumerable<Workshop> Cache
        {
            get
            {
                if (cache == null) cache = _repo.GetAll().ToList();
                return cache;
            }
        }

        Repository<Workshop> _repo;
        public WorkshopService(Repository<Workshop> repo)
        {
            _repo = repo;
        }

        public IEnumerable<Workshop> GetAll() => Cache;
        public Workshop? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(Workshop item)
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
