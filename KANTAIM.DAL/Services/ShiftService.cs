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

        //public int GetWeekShift(DateTime dateProd, int NumDayShift, bool cache)
        //{
        //    if(!cache) ResetCache();

        //    int weekShift = 0;
        //    string[] Week = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        //    foreach (var shift in Cache)
        //    {
        //        foreach (var dayOfWeek in Week)
        //        {
        //            var propertyInfo = shift.GetType().GetProperty(dayOfWeek);
        //            if (propertyInfo != null)
        //            {
        //                if (Enum.TryParse(dayOfWeek, out DayOfWeek shiftDayOfWeek))
        //                {
        //                    if (shiftDayOfWeek > dateProd.DayOfWeek)
        //                    {
        //                        var value = propertyInfo.GetValue(shift);
        //                        if (value != null)
        //                        {
        //                            weekShift++;
        //                        }
        //                    }
        //                    else
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    weekShift += NumDayShift;
        //    return weekShift;
        //}

        public int GetWeekShift(DateTime dateProd, int numDayShift, bool cache)
        {
            if (!cache) ResetCache();

            // Jour de la semaine (Lundi = 0, Dimanche = 6)
            int dayOfWeek = (int)dateProd.DayOfWeek;

            // Décalage pour que Lundi = 1, Dimanche = 7
            int correctedDayOfWeek = (dayOfWeek == 0) ? 7 : dayOfWeek;

            int weekShift = 0;

            // Parcourir les jours précédents pour cumuler les shifts
            for (int i = 1; i < correctedDayOfWeek; i++)
            {
                // Récupérer le nombre de shifts pour chaque jour précédent
                var shifts = GetShiftsForDay((DayOfWeek)(i % 7), cache);
                weekShift += shifts.Count(s => s.HasValue); // Compter les shifts valides
            }

            // Ajouter le numéro de shift du jour
            weekShift += numDayShift;

            return weekShift;
        }

        public int GetDayShift(DateTime dateProd, bool cache)
        {
            if (!cache) ResetCache();

            var currentTime = dateProd.TimeOfDay;
            var dayOfWeek = dateProd.DayOfWeek;

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

        public TimeSpan?[] GetShiftsForDay(DayOfWeek dayOfWeek, bool cache)
        {
            if (!cache) ResetCache();

            // Extraire les heures de début des shifts pour le jour donné
            var shifts = Cache
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
                .Where(x => x.HasValue) // Garder uniquement les valeurs non nulles
                .OrderBy(x => x) // Trier par ordre chronologique
                .Take(3) // Prendre seulement les 3 premiers shifts du jour
                .ToArray();

            return shifts;
        }

        public DateTime? GetCurrentShiftDate(DateTime dateProd, bool cache)
        {
            if (!cache) ResetCache();

            var currentTime = dateProd.TimeOfDay;
            var dayOfWeek = dateProd.DayOfWeek;

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
                    NextStartTime = GetNextShiftStartTime(shift, dayOfWeek)
                })
                .Where(x => x.StartTime.HasValue)
                .OrderBy(x => x.StartTime)
                .ToList();

            foreach (var shift in shifts)
            {
                var startTime = shift.StartTime.Value;
                var endTime = shift.NextStartTime ?? TimeSpan.FromHours(24); // Fin à 23:59 si aucun shift suivant

                // ✅ Gestion du chevauchement jour suivant
                if (endTime < startTime)
                {
                    // Si le shift chevauche le lendemain
                    if (currentTime >= startTime || currentTime < endTime)
                    {
                        var shiftDate = (currentTime >= startTime)
                            ? dateProd.Date
                            : dateProd.AddDays(-1).Date; // Si on est après minuit, le shift a commencé la veille

                        Console.WriteLine($"Shift actif : {shift.ShiftNumber} - Date réelle : {shiftDate:dd/MM/yyyy}");
                        return shiftDate;
                    }
                }
                else
                {
                    // ✅ Cas classique dans la journée
                    if (currentTime >= startTime && currentTime < endTime)
                    {
                        Console.WriteLine($"Shift actif : {shift.ShiftNumber} - Date réelle : {DateTime.Now:dd/MM/yyyy}");
                        return dateProd.Date;
                    }
                }
            }

            return null;
        }

        public int GetPreviousDayShift(DateTime dateProd, bool cache)
        {
            if (!cache) ResetCache();

            var currentTime = dateProd.TimeOfDay;
            var dayOfWeek = dateProd.DayOfWeek;

            Console.WriteLine($"➡️ DateProd : {dateProd} | Heure actuelle : {currentTime}");

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
                    NextStartTime = GetNextShiftStartTime(shift, dayOfWeek)
                })
                .Where(x => x.StartTime.HasValue)
                .OrderBy(x => x.StartTime)
                .ToList();

            Console.WriteLine($"🔎 Shifts disponibles aujourd'hui :");
            foreach (var shift in shifts)
            {
                Console.WriteLine($"  - Shift {shift.ShiftNumber} : {shift.StartTime} ➡️ {shift.NextStartTime}");
            }

            if (!shifts.Any())
                return -1;

            var firstShift = shifts.First();
            var lastShift = shifts.Last();

            // ✅ Si l'heure est avant le premier shift → Prendre le dernier shift (même chevauché)
            if (currentTime < firstShift.StartTime)
            {
                Console.WriteLine($"🚨 Heure avant le premier shift → Retour au dernier shift de la veille");

                var lastShiftOfYesterday = GetLastShiftOfPreviousDay(dateProd);

                // 🔥 Si le dernier shift chevauche la journée suivante, on veut remonter au précédent
                if (lastShiftOfYesterday == lastShift.ShiftNumber)
                {
                    var previousShift = (shifts.Count > 1) ? shifts[^2].ShiftNumber : lastShift.ShiftNumber;
                    Console.WriteLine($"✅ Shift précédent (après chevauchement) : {previousShift}");
                    return previousShift;
                }
                else
                {
                    Console.WriteLine($"✅ Shift précédent (veille complète) : {lastShiftOfYesterday}");
                    return lastShiftOfYesterday;
                }
            }


            // ✅ Si l'heure est après le dernier shift → Retour au dernier shift du jour
            if (currentTime >= lastShift.StartTime)
            {
                Console.WriteLine($"🚨 Heure après le dernier shift → Retour au dernier shift du jour");
                var previousShift = (shifts.Count > 1) ? shifts[shifts.Count - 2] : lastShift;
                Console.WriteLine($"✅ Shift précédent (fin de journée) : {previousShift.ShiftNumber}");
                return previousShift.ShiftNumber;
            }

            // ✅ Si l'heure est pendant un shift → Retourner le shift précédent
            for (int i = 0; i < shifts.Count; i++)
            {
                var shift = shifts[i];
                var startTime = shift.StartTime.Value;
                var endTime = shift.NextStartTime ?? TimeSpan.FromHours(24);

                if (endTime < startTime) // Cas de chevauchement
                {
                    if (currentTime >= startTime || currentTime < endTime)
                    {
                        var previousShift = (i == 0) ? GetLastShiftOfPreviousDay(dateProd) : shifts[i - 1].ShiftNumber;
                        Console.WriteLine($"✅ Cas de chevauchement : Shift précédent = {previousShift}");
                        return previousShift;
                    }
                }
                else
                {
                    if (currentTime >= startTime && currentTime < endTime)
                    {
                        var previousShift = (i == 0) ? GetLastShiftOfPreviousDay(dateProd) : shifts[i - 1].ShiftNumber;
                        Console.WriteLine($"✅ Shift trouvé : Shift précédent = {previousShift}");
                        return previousShift;
                    }
                }
            }

            Console.WriteLine($"🚨 Aucun shift trouvé → Dernier shift par défaut : {lastShift.ShiftNumber}");
            return lastShift.ShiftNumber;
        }

        private int GetLastShiftOfPreviousDay(DateTime dateProd)
        {
            var previousDay = dateProd.AddDays(-1).DayOfWeek;

            var lastShiftYesterday = Cache
                .Select(shift => new
                {
                    ShiftNumber = shift.ShiftNumber,
                    StartTime = previousDay switch
                    {
                        DayOfWeek.Monday => shift.Monday?.TimeOfDay,
                        DayOfWeek.Tuesday => shift.Tuesday?.TimeOfDay,
                        DayOfWeek.Wednesday => shift.Wednesday?.TimeOfDay,
                        DayOfWeek.Thursday => shift.Thursday?.TimeOfDay,
                        DayOfWeek.Friday => shift.Friday?.TimeOfDay,
                        DayOfWeek.Saturday => shift.Saturday?.TimeOfDay,
                        DayOfWeek.Sunday => shift.Sunday?.TimeOfDay,
                        _ => null
                    }
                })
                .Where(x => x.StartTime.HasValue)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefault();

            if (lastShiftYesterday != null)
            {
                Console.WriteLine($"✅ Dernier shift de la veille : {lastShiftYesterday.ShiftNumber}");
                return lastShiftYesterday.ShiftNumber;
            }

            Console.WriteLine($"❌ Aucun shift trouvé pour la veille !");
            return -1;
        }



        public DateTime? GetPreviousShiftDate(DateTime dateProd, bool cache)
        {
            if (!cache) ResetCache();

            var currentTime = dateProd.TimeOfDay;
            var dayOfWeek = dateProd.DayOfWeek;

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
                    NextStartTime = GetNextShiftStartTime(shift, dayOfWeek)
                })
                .Where(x => x.StartTime.HasValue)
                .OrderBy(x => x.StartTime)
                .ToList();

            if (!shifts.Any())
                return null;

            // ✅ Si l'heure est avant le premier shift → Retourner le dernier shift de la veille
            var firstShift = shifts.First();
            if (currentTime < firstShift.StartTime)
            {
                var previousDay = dateProd.AddDays(-1).DayOfWeek;

                var lastShiftYesterday = Cache
                    .Select(shift => new
                    {
                        ShiftNumber = shift.ShiftNumber,
                        StartTime = previousDay switch
                        {
                            DayOfWeek.Monday => shift.Monday?.TimeOfDay,
                            DayOfWeek.Tuesday => shift.Tuesday?.TimeOfDay,
                            DayOfWeek.Wednesday => shift.Wednesday?.TimeOfDay,
                            DayOfWeek.Thursday => shift.Thursday?.TimeOfDay,
                            DayOfWeek.Friday => shift.Friday?.TimeOfDay,
                            DayOfWeek.Saturday => shift.Saturday?.TimeOfDay,
                            DayOfWeek.Sunday => shift.Sunday?.TimeOfDay,
                            _ => null
                        }
                    })
                    .Where(x => x.StartTime.HasValue)
                    .OrderByDescending(x => x.StartTime)
                    .FirstOrDefault();

                if (lastShiftYesterday != null)
                {
                    var shiftDate = dateProd.AddDays(-1).Date;
                    Console.WriteLine($"Shift précédent (veille) - Date réelle : {shiftDate:dd/MM/yyyy}");
                    return shiftDate;
                }
            }

            // ✅ Si l'heure est dans un shift → Retourner la date du shift précédent
            for (int i = 0; i < shifts.Count; i++)
            {
                var shift = shifts[i];
                var startTime = shift.StartTime.Value;
                var endTime = shift.NextStartTime ?? TimeSpan.FromHours(24);

                if (endTime < startTime) // Gestion du chevauchement de jour
                {
                    if (currentTime >= startTime || currentTime < endTime)
                    {
                        var previousShift = (i == 0) ? shifts.Last() : shifts[i - 1];
                        var shiftDate = (previousShift.StartTime.Value > startTime)
                            ? dateProd.AddDays(-1).Date
                            : dateProd.Date;

                        Console.WriteLine($"Shift précédent (actif) - Date réelle : {shiftDate:dd/MM/yyyy}");
                        return shiftDate;
                    }
                }
                else
                {
                    if (currentTime >= startTime && currentTime < endTime)
                    {
                        var previousShift = (i == 0) ? shifts.Last() : shifts[i - 1];
                        var shiftDate = dateProd.Date;

                        // ✅ Si le shift précédent chevauche la veille, on corrige la date
                        if (previousShift.StartTime.Value > startTime)
                        {
                            shiftDate = dateProd.AddDays(-1).Date;
                        }

                        Console.WriteLine($"Shift précédent (actif) - Date réelle : {shiftDate:dd/MM/yyyy}");
                        return shiftDate;
                    }
                }
            }

            // ✅ Si l'heure est après le dernier shift → Retourner le dernier shift du jour
            var lastShift = shifts.Last();
            var lastShiftDate = dateProd.Date;
            Console.WriteLine($"Shift précédent (dernier shift du jour) - Date réelle : {lastShiftDate:dd/MM/yyyy}");
            return lastShiftDate;
        }

    }
}
