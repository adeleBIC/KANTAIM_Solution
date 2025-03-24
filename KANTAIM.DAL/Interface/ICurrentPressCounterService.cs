using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Interface
{
    public interface ICurrentPressCounterService
    {
        public IEnumerable<CurrentPressCounter> GetAll();
        public CurrentPressCounter? GetById(int id);
        public void UpSert(CurrentPressCounter item);
        public void Delete(int id);

        public IEnumerable<Press> GetAllPressInclude();

        public Press GetPressByIdInclude(int id);

    }
}
