using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Services
{
    public class RackCellService
    {
        public RackCellService()
        {

        }
        public int Insert(RackCell model)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(model).State = EntityState.Added;
            ctx.SaveChanges();
            return model.RackId;
        }

        public void Update(RackCell model)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(model).State = EntityState.Modified;
            ctx.SaveChanges();
        }

        public void Delete(int rId, int cId)
        {
            using DataKANTAIMContext ctx = new();
            ctx.Entry(new RackCell() { RackId = rId, CellId = cId }).State = EntityState.Deleted;
            ctx.SaveChanges();
        }
        public IEnumerable<RackCell> GetAllRackCellByRack(int id)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackCells.Where(m => m.RackId == id).ToList();
        }

        public IEnumerable<RackCell> GetAllRackCellByCell(int id)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackCells.Where(m => m.CellId == id).ToList();
        }

        public IEnumerable<RackCell> GetAll()
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackCells.ToList();
        }

        public RackCell? GetById(int rId, int cId)
        {
            using DataKANTAIMContext ctx = new();
            return ctx.RackCells.SingleOrDefault(t => t.RackId == rId && t.CellId == cId); ;
        }
    }
}
