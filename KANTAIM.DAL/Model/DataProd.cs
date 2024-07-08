using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TDataProd")]
    public class DataProd : IObject
    {
        [Key]
        [Column("TDataProdID")]
        public int Id { get; set; }

        public int NumDayShift { get; set; }

        public int NumWeekShift { get; set; }

        public int Counter { get; set; }

        public decimal TRS { get; set; }

        public decimal OpenTime { get; set; }

        public decimal Objective { get; set; }

        public bool ObjOk { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateProd { get; set; }

        public DateTime DateExtract { get; set; }

        public string? Comment { get; set; }

        [Column("FKTPressID")]
        public int PressID { get; set; }
        [ForeignKey(nameof(PressID))]
        public virtual Press Press { get; set; }

        [Column("FKTProductID")]
        public int ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product Product { get; set; }
    }
}
