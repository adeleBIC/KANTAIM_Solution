using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class UserService
    {
        private List<User> cache;
        DevModeService _devModeService;
        private bool oldDevMode = false;

        public int CurrentUserLvl { get; set; }
        public IEnumerable<User> Cache
        {
            get
            {
                if (cache == null || oldDevMode != _devModeService.DevMode)
                {
                    cache = _repo.GetAll().ToList();
                    foreach (User item in cache)
                    {
                        item.UserAccessLvl = _repoUserAccessLvl.GetById(item.UserAccessLvlId);
                    }
                    oldDevMode = _devModeService.DevMode;
                }
                return cache;
            }
        }

        Repository<User> _repo;
        Repository<UserAccessLvl> _repoUserAccessLvl;
        public UserService(Repository<User> repo, Repository<UserAccessLvl> repoUserAccessLvl, DevModeService devModeService)
        {
            _repo = repo;
            _repoUserAccessLvl = repoUserAccessLvl;
            _devModeService = devModeService;
        }

        public IEnumerable<User> GetAll() => Cache;
        public IEnumerable<User> GetByLvl(int lvlId) => Cache.Where(u=>u.UserAccessLvlId == lvlId);
        public User? GetByName(string name) => Cache.SingleOrDefault(u => u.LoginADUser.ToLower() == name.ToLower());
        public void SetCurrentUserLvl(string name) => CurrentUserLvl = GetByName(name)?.UserAccessLvl.AccesLvL ?? 0;
        public User? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);

        public void ResetCache() => cache = null;

        public void UpSert(User item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }

        public IEnumerable<UserAccessLvl> GetAllLvl() => _repoUserAccessLvl.GetAll();
        public UserAccessLvl? GetFirstLvl() => _repoUserAccessLvl.GetAll().OrderBy(l => l.Id).FirstOrDefault();

    }
}
