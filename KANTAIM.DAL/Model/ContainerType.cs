using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TContainerType")]
    public class ContainerType : IObject
    {
        [Key]
        [Column("TContainerTypeID")]
        public int Id { get; set; }

        public string Name { get; set; }
        public int NbrMaxContainer { get; set; }

        public int? TypeNumber { get; set; }

        public int Ymax { get; set; }
        public bool IsContainable { get; set; }

        public string? Comment { get; set; }
    }
}
