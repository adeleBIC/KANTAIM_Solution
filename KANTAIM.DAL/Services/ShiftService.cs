using KANTAIM.DAL.Interface;
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
            return date.Hour * 3600 + date.Minute * 60 + date.Second;
            // Use shifttotal as needed here
        }

        public int GetWeekShift(DateTime dateProd, int NumDayShift, bool cache)
        {
            if(!cache) ResetCache();

            int weekShift = 0;
            string[] Week = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            foreach (var shift in Cache)
            {
                foreach (var dayOfWeek in Week)
                {
                    var propertyInfo = shift.GetType().GetProperty(dayOfWeek);
                    if (propertyInfo != null)
                    {
                        if (Enum.TryParse(dayOfWeek, out DayOfWeek shiftDayOfWeek))
                        {
                            if (shiftDayOfWeek > dateProd.DayOfWeek)
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
            weekShift += NumDayShift;
            return weekShift;
        }

        public int GetDayShift(DateTime dateProd, bool cache)
        {
            if (!cache) ResetCache();

            var currentTime = DateTime.Now.TimeOfDay;
            var dayOfWeek = DateTime.Now.DayOfWeek;

            // Récupération des heures de début de chaque shift selon le jour actuel
            var shifts = Cache
                .Select(shift => new
                {
                    ShiftNumber = shift.ShiftNumber,
                    StartTime = dayOfWeek switch
                    {
                        DayOfWeek.Monday => shift.Monday?.TimeOfDay,
                        DayOfWeek.Tuesday => shift.Tuesday?.TimeOfDay,
                        DayOfWeek.Wednesday => shift.Wednesday?.TimeOfDay,
                        DayOfWeek.Thursday => shift.Thursday?.TimeOfDay,
                        DayOfWeek.Friday => shift.Friday?.TimeOfDay,
                        DayOfWeek.Saturday => shift.Saturday?.TimeOfDay,
                        DayOfWeek.Sunday => shift.Sunday?.TimeOfDay,
                        _ => null
                    },
                    NextStartTime = GetNextShiftStartTime(shift, dayOfWeek) // Gestion du chevauchement jour suivant
                })
                .Where(x => x.StartTime.HasValue)
                .OrderBy(x => x.StartTime)
                .ToList();

            if (!shifts.Any())
                return -1;

            foreach (var shift in shifts)
            {
                var startTime = shift.StartTime.Value;
                var endTime = shift.NextStartTime ?? TimeSpan.FromHours(24); // Fin = prochain shift ou 23:59

                // Gestion des chevauchements au lendemain
                if (endTime < startTime)
                {
                    // Si le shift chevauche le jour suivant
                    if (currentTime >= startTime || currentTime < endTime)
                    {
                        Console.WriteLine($"Shift actif : {shift.ShiftNumber}");
                        return shift.ShiftNumber;
                    }
                }
                else
                {
                    // Cas classique dans la même journée
                    if (currentTime >= startTime && currentTime < endTime)
                    {
                        Console.WriteLine($"Shift actif : {shift.ShiftNumber}");
                        return shift.ShiftNumber;
                    }
                }
            }

            // Si aucun shift trouvé
            return -1;
        }

        private TimeSpan? GetNextShiftStartTime(Shift currentShift, DayOfWeek dayOfWeek)
        {
            // Liste triée par heure de début dans la journée actuelle
            var shiftsToday = Cache
                .Select(shift => dayOfWeek switch
                {
                    DayOfWeek.Monday => shift.Monday?.TimeOfDay,
                    DayOfWeek.Tuesday => shift.Tuesday?.TimeOfDay,
                    DayOfWeek.Wednesday => shift.Wednesday?.TimeOfDay,
                    DayOfWeek.Thursday => shift.Thursday?.TimeOfDay,
                    DayOfWeek.Friday => shift.Friday?.TimeOfDay,
                    DayOfWeek.Saturday => shift.Saturday?.TimeOfDay,
                    DayOfWeek.Sunday => shift.Sunday?.TimeOfDay,
                    _ => null
                })
                .Where(x => x.HasValue)
                .OrderBy(x => x)
                .ToList();

            // On cherche le shift suivant dans la même journée
            var currentShiftTime = dayOfWeek switch
            {
                DayOfWeek.Monday => currentShift.Monday?.TimeOfDay,
                DayOfWeek.Tuesday => currentShift.Tuesday?.TimeOfDay,
                DayOfWeek.Wednesday => currentShift.Wednesday?.TimeOfDay,
                DayOfWeek.Thursday => currentShift.Thursday?.TimeOfDay,
                DayOfWeek.Friday => currentShift.Friday?.TimeOfDay,
                DayOfWeek.Saturday => currentShift.Saturday?.TimeOfDay,
                DayOfWeek.Sunday => currentShift.Sunday?.TimeOfDay,
                _ => null
            };

            // Chercher le prochain shift dans la journée actuelle
            var nextShiftToday = shiftsToday
                .FirstOrDefault(x => x > currentShiftTime);

            if (nextShiftToday.HasValue)
                return nextShiftToday;

            // Si aucun shift suivant dans la journée, on prend le premier shift du lendemain
            var nextDayOfWeek = (DayOfWeek)(((int)dayOfWeek + 1) % 7);

            var nextShiftTomorrow = Cache
                .Select(shift => nextDayOfWeek switch
                {
                    DayOfWeek.Monday => shift.Monday?.TimeOfDay,
                    DayOfWeek.Tuesday => shift.Tuesday?.TimeOfDay,
                    DayOfWeek.Wednesday => shift.Wednesday?.TimeOfDay,
                    DayOfWeek.Thursday => shift.Thursday?.TimeOfDay,
                    DayOfWeek.Friday => shift.Friday?.TimeOfDay,
                    DayOfWeek.Saturday => shift.Saturday?.TimeOfDay,
                    DayOfWeek.Sunday => shift.Sunday?.TimeOfDay,
                    _ => null
                })
                .Where(x => x.HasValue)
                .OrderBy(x => x)
                .FirstOrDefault();

            return nextShiftTomorrow;
        }


    }
}
