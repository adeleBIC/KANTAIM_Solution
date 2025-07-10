using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class DevModeService
    {
        public event Action OnChange;

        private bool _devMode;
        public bool DevMode
        {
            get => _devMode;
            set
            {
                if (_devMode != value)
                {
                    _devMode = value;
                    NotifyStateChanged();
                }
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
