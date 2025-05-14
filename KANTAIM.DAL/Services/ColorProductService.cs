using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{

        public class ColorProductService
        {
            private List<ColorProduct> cache;
            Repository<ColorProduct> _repo;
            DevModeService _devModeService;
            private bool oldDevMode = false;
            public IEnumerable<ColorProduct> Cache
            {
                get
                {
                    if (cache == null || oldDevMode != _devModeService.DevMode)
                    {
                        using DataKANTAIMContext ctx = new(_devModeService.DevMode);
                        cache = ctx.ColorProducts.Include(c => c.Color).Include(c => c.Product).ToList();
                        oldDevMode = _devModeService.DevMode;
                    }
                    return cache;
                }
            }

            public ColorProductService(Repository<ColorProduct> repo, DevModeService devModeService)
            {
                this._repo = repo;
                _devModeService = devModeService;
            }

            public void ResetCache() => cache = null;

            public IEnumerable<ColorProduct> GetAll() => Cache;

            public IEnumerable<ColorProduct> GetAllPerColor(int id) => Cache.Where(c => c.ColorID == id);

            public IEnumerable<ColorProduct> GetAllPerProduct(int? id) => id == null ? null : Cache.Where(c => c.ProductID == id);


            public ColorProduct? GetByColorId(int id)
            {
                using DataKANTAIMContext ctx = new(_devModeService.DevMode);
                return ctx.ColorProducts.SingleOrDefault(t => t.ColorID == id);
            }

            public Boolean hasLien(int colorId)
            {
                using (DataKANTAIMContext ctx = new())
                {
                    return ctx.ColorProducts.Any(t => t.ColorID == colorId);
                }
        }

            public ColorProduct? GetByProdcutId(int id)
            {
                using DataKANTAIMContext ctx = new(_devModeService.DevMode);
                return ctx.ColorProducts.SingleOrDefault(t => t.ProductID == id);
            }

            public bool FindLink(int colorId, int productId)
            {
                return Cache.FirstOrDefault(cp => cp.ColorID == colorId && cp.ProductID == productId) == null ? false : true;
            }
            public void UpSert(ColorProduct item)
            {
                ResetCache();
                _repo.UpSert(item);
            }

            public void Delete(int id)
            {
                ResetCache();
                _repo.Delete(id);
            }

            public void DeleteLink(int colorId, int productId)
            {
                if(FindLink(colorId, productId))
                {
                    Delete(Cache.FirstOrDefault(cp => cp.ColorID == colorId && cp.ProductID == productId).Id);
                }
            }


            public void DeleteAllByColorId(int colorId)
            {
                using (DataKANTAIMContext ctx = new DataKANTAIMContext())
                {
                    // Retrieve all entries linked to the specified color
                    var entries = ctx.ColorProducts.Where(cp => cp.ColorID == colorId).ToList();

                    // If there are entries, proceed to delete
                    if (entries.Any())
                    {
                        foreach (var entry in entries)
                        {
                            ctx.ColorProducts.Remove(entry);  // Remove each entry from the context
                        }
                        ctx.SaveChanges();  // Commit the changes in the database
                    }
                }
                ResetCache();  // Reset the cache to reflect these changes
            }
        }
    
}
