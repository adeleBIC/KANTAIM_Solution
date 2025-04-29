using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("RackProfil")]
    public class RackProfil
    {
        [ForeignKey("Rack")]
        public int RackId { get; set; }
        [ForeignKey("Profil")]
        public int ProfilId { get; set; }
        public Rack Rack { get; set; }
        public Profil Profil { get; set; }

    }
}
