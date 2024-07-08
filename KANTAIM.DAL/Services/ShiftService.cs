using KANTAIM.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class ShiftService
    {
        private List<Shift> cache;

        public IEnumerable<Shift> Cache
        {
            get
            {
                if (cache == null) cache = _repo.GetAll().ToList();
                return cache;
            }
        }

        Repository<Shift> _repo;
        public ShiftService(Repository<Shift> _repo)
        {
            this._repo = _repo;
        }

        public IEnumerable<Shift> GetAll() => Cache;
        public Shift? GetById(int id) => Cache.SingleOrDefault(c => c.Id == id);
        public void ResetCache() => cache = null;

        public void UpSert(Shift item)
        {
            ResetCache();
            _repo.UpSert(item);
        }

        public void Delete(int id)
        {
            ResetCache();
            _repo.Delete(id);
        }

      
        public int CalculSecond(DateTime date)
        {
            return date.Hour*3600 + date.Minute*60 + date.Second;
            // Use shifttotal as needed here
        }
        public int GetWeekShift(DataProd data)
        {
            int weekShift = 0;
            string[] Week = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            foreach (var shift in Cache)
            {
                foreach (var dayOfWeek in Week)
                {
                    var propertyInfo = shift.GetType().GetProperty(dayOfWeek);
                    if (propertyInfo != null)
                    {
                        if(Enum.TryParse(dayOfWeek, out DayOfWeek shiftDayOfWeek))
                        {
                            if (shiftDayOfWeek > data.DateProd.DayOfWeek)
                            {
                                var value = propertyInfo.GetValue(shift);
                                if (value != null)
                                {
                                    weekShift++;
                                }
                            }
                            else
                                break;
                        }
                            
                    }
                }
            }
            weekShift += data.NumDayShift;
            return weekShift;
        }
    }
}
