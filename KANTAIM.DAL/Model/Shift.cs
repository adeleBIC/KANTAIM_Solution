using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TShift")]
    public class Shift : IObject
    {
        [Key]
        [Column("TShiftID")]
        public int Id { get; set; }

        [Column("Shift")]
        public int ShiftNumber { get; set; }

        public DateTime? Monday { get; set; }

        public DateTime? Tuesday { get; set; }

        public DateTime? Wednesday { get; set; }

        public DateTime? Thursday { get; set; }

        public DateTime? Friday { get; set; }

        public DateTime? Saturday { get; set; }

        public DateTime? Sunday { get; set; }
    }
}
