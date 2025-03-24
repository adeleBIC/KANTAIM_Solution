using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TCurrentPressCounter")]
    public class CurrentPressCounter : IObject
    {
        [Key]
        [Column("TCurrentPressCounterID")]
        public int Id { get; set; }

        public long PreviousCounter { get; set; }

        public long CurrentCounter { get; set; }

        [Required]
        [Column("FKTPressID")]
        public int PressID { get; set; }
        [ForeignKey(nameof(PressID))]
        public virtual Press Press { get; set; }
    }
}
