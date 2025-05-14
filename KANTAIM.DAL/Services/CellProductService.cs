using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class CellProductService
    {
        private List<CellProduct> cache;
        Repository<CellProduct> _repo;
        DevModeService _devModeService;
        private bool oldDevMode = false;
        public IEnumerable<CellProduct> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    using DataKANTAIMContext ctx = new(_devModeService.DevMode);
                    cache = ctx.CellProducts.Include(c=>c.Cell).Include(c => c.Product).ToList();
                    oldDevMode = _devModeService.DevMode;
                }
                return cache;
            }
        }

        public CellProductService(Repository<CellProduct> repo, DevModeService devModeService)
        {
            this._repo = repo;
            _devModeService = devModeService;
        }

        public void ResetCache() => cache = null;

        public IEnumerable<CellProduct> GetAll() => Cache;

        public IEnumerable<CellProduct> GetAllPerCell(int id) => Cache.Where(c => c.CellID == id);

        public IEnumerable<CellProduct> GetAllPerProduct(int id) => Cache.Where(c => c.ProductID == id);


        public CellProduct GetByCellId(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.CellProducts.SingleOrDefault(t => t.CellID == id);
        }
        public CellProduct GetByProdcutId(int id)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.CellProducts.SingleOrDefault(t => t.ProductID == id);
        }

        public bool FindLink(int cellId, int productId)
        {
            return Cache.FirstOrDefault(cp => cp.CellID == cellId && cp.ProductID == productId) == null ? false : true;
        }
        public void UpSert(CellProduct item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }
        public void DeleteLink(int cellId, int productId)
        {
            if (FindLink(cellId, productId))
            {
                Delete(Cache.FirstOrDefault(cp => cp.CellID == cellId && cp.ProductID == productId).Id);
            }
        }

        public void DeleteAllByCellId(int cellId)
        {
            using (DataKANTAIMContext ctx = new DataKANTAIMContext())
            {
                // Retrieve all entries linked to the specified cell
                var entries = ctx.CellProducts.Where(cp => cp.CellID == cellId).ToList();

                // If there are entries, proceed to delete
                if (entries.Any())
                {
                    foreach (var entry in entries)
                    {
                        ctx.CellProducts.Remove(entry);  // Remove each entry from the context
                    }
                    ctx.SaveChanges();  // Commit the changes in the database
                }
            }
            ResetCache();  // Reset the cache to reflect these changes
        }
    }
}
