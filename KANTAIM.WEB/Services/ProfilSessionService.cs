using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.WEB.Services
{
    public class ProfilSessionService
    {
        public Profil? CurrentProfil { get; private set; }

        public void SetProfil(Profil profil)
        {
            CurrentProfil = profil;
        }

        public void ClearProfil()
        {
            CurrentProfil = null;
        }
    }
}
