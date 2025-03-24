using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Interface
{
    public interface IShiftService
    {
        public IEnumerable<Shift> GetAll();
        public Shift? GetById(int id);
        public void ResetCache();
        public void UpSert(Shift item);
        public void Delete(int id);
        public int CalculSecond(DateTime date);
        public int GetWeekShift(DateTime dateProd, int NumDayShift, bool cache);
        public int GetDayShift(DateTime dateProd, bool cache);

    }
}
