using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TUsers")]
    public class User : IObject
    {
        [Key]
        [Column("TUserID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LoginADUser { get; set; }

        public bool DarkMode { get; set; }

        public string? Comment { get; set; }

        [Column("FKTUserAccessLvlID")]
        public int UserAccessLvlId { get; set; }
        [ForeignKey(nameof(UserAccessLvlId))]
        public virtual UserAccessLvl UserAccessLvl { get; set; }

    }
}
