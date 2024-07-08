using MudBlazor;
using KANTAIM.DAL.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ShiftVM : IValidatableObject
    {
        public static explicit operator Shift(ShiftVM vm) => vm.model;

        Shift model;

        public ShiftVM() => model = new Shift();
        public ShiftVM(Shift model)
        {
            this.model = model;
            shiftNumber = model.ShiftNumber;
            if (model.Monday.HasValue) monday = model.Monday.Value.TimeOfDay;
            else monday = null;
            if (model.Tuesday.HasValue) tuesday = model.Tuesday.Value.TimeOfDay;
            else tuesday = null;
            if (model.Wednesday.HasValue) wednesday = model.Wednesday.Value.TimeOfDay;
            else wednesday = null;
            if (model.Thursday.HasValue) thursday = model.Thursday.Value.TimeOfDay;
            else thursday = null;
            if (model.Friday.HasValue) friday = model.Friday.Value.TimeOfDay;
            else friday = null;
            if (model.Saturday.HasValue) saturday = model.Saturday.Value.TimeOfDay;
            else saturday = null;
            if (model.Sunday.HasValue) sunday = model.Sunday.Value.TimeOfDay;
            else sunday = null;
        }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;


        [Required]
        [Label("Numéro de shift")]
        private int shiftNumber;
        public int ShiftNumber
        {
            get { return shiftNumber; }
            set { shiftNumber = value; IsEditing = true; }
        }


        [Label("Lundi")]
        private TimeSpan? monday;
        public TimeSpan? Monday
        {
            get { return monday; }
            set { monday = value; IsEditing = true; }

        }

        [Label("Mardi")]
        private TimeSpan? tuesday;
        public TimeSpan? Tuesday
        {
            get { return tuesday; }
            set { tuesday = value; IsEditing = true; }
        }

        [Label("Mercerdi")]
        private TimeSpan? wednesday;
        public TimeSpan? Wednesday
        {
            get { return wednesday; }
            set { wednesday = value; IsEditing = true; }
        }

        [Label("Jeudi")]
        private TimeSpan? thursday;
        public TimeSpan? Thursday
        {
            get { return thursday; }
            set { thursday = value; IsEditing = true; }
        }

        [Label("Vendredi")]
        private TimeSpan? friday;
        public TimeSpan? Friday
        {
            get { return friday; }
            set { friday = value; IsEditing = true; }
        }

        [Label("Samedi")]
        private TimeSpan? saturday;
        public TimeSpan? Saturday
        {
            get { return saturday; }
            set { saturday = value; IsEditing = true; }
        }

        [Label("Dimanche")]
        private TimeSpan? sunday;
        public TimeSpan? Sunday
        {
            get { return sunday; }
            set { sunday = value; IsEditing = true; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (shiftNumber<0) yield return new ValidationResult("Le Numéro de shift doit être plus grand que 0", new string[] { "Name" });
            else
            {
                model.ShiftNumber = shiftNumber;
                if (monday != null)
                    model.Monday = Convert.ToDateTime(monday.ToString());
                else
                    model.Monday = null;
                if (tuesday != null)
                    model.Tuesday = Convert.ToDateTime(tuesday.ToString());
                else
                    model.Tuesday = null;
                if (wednesday != null)
                    model.Wednesday = Convert.ToDateTime(wednesday.ToString());
                else
                    model.Wednesday = null;
                if (thursday != null)
                    model.Thursday = Convert.ToDateTime(thursday.ToString());
                else
                    model.Thursday = null;
                if (friday != null)
                    model.Friday = Convert.ToDateTime(friday.ToString());
                else
                    model.Friday = null;
                if (saturday != null)
                    model.Saturday = Convert.ToDateTime(saturday.ToString());
                else
                    model.Saturday = null;
                if (sunday != null)
                    model.Sunday = Convert.ToDateTime(sunday.ToString());
                else
                    model.Sunday = null;
            }
        }
    }
}
