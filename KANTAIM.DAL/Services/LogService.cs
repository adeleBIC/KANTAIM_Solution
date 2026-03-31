using Azure;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Ressources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KANTAIM.DAL.Services
{
    public class LogService
    {
        Repository<Log> _repo;
        Repository<Product> _repoProduct;
        Repository<Container> _repoContainer;
        Repository<ContainerType> _repoContainerType;
        Repository<Press> _repoPress;
        Repository<Shape> _repoShape;
        Repository<Machine> _repoMachine;
        Repository<ProdColor> _repoColor;
        Repository<Cell> _repoCell;
        DevModeService _devModeService;

        public LogService(
            Repository<Log> repo,
            Repository<Product> repoProduct,
            Repository<Container> repoContainer,
            Repository<Press> repoPress,
            Repository<Shape> repoShape,
            Repository<Machine> repoMachine,
            Repository<ProdColor> repoColor,
            Repository<Cell> repoCell,
            DevModeService devModeService)
        {
            _repo = repo;
            _repoProduct = repoProduct;
            _repoContainer = repoContainer;
            _repoPress = repoPress;
            _repoShape = repoShape;
            _repoMachine = repoMachine;
            _repoColor = repoColor;
            _repoCell = repoCell;
            _devModeService = devModeService;
        }

        // =========================================================
        // Legacy methods — kept for backward compatibility.
        // Do NOT call these inside loops; they load the full table.
        // =========================================================

        /// <summary>
        /// Loads every Log row with all related entities included.
        /// Use only when full navigation properties are required.
        /// Avoid calling this in loops or bulk operations.
        /// </summary>
        public IEnumerable<Log> GetAll()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs
                .Include(c => c.Product)
                .Include(c => c.Press)
                .Include(c => c.Shape)
                .Include(c => c.Cell).ThenInclude(c => c.RackCells).ThenInclude(r => r.Rack)
                .Include(c => c.ProdColor)
                .Include(c => c.Machine)
                .Include(c => c.Container).ThenInclude(x => x.ContainerType)
                .ToList();
        }

        public IEnumerable<Log> GetAllWithouInclude()
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs.ToList();
        }

        public void UpSert(Log item) => _repo.UpSert(item);
        public void Delete(int id) => _repo.Delete(id);

        public IEnumerable<Container> GetAllContainer() => _repoContainer.GetAll();
        public IEnumerable<Product> GetAllProduct() => _repoProduct.GetAll();
        public IEnumerable<Cell> GetAllCell() => _repoCell.GetAll();
        public IEnumerable<Press> GetAllPress() => _repoPress.GetAll();
        public IEnumerable<Shape> GetAllShape() => _repoShape.GetAll();
        public IEnumerable<ProdColor> GetAllColor() => _repoColor.GetAll();
        public IEnumerable<Machine> GetAllMachine() => _repoMachine.GetAll();

        // Legacy query methods — all delegate to GetAll() (full table load).
        // Replace with the Direct equivalents below for any performance-sensitive call.
        public Log? GetByContenaireId(int id)
            => GetAll().OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == id);

        public Log? GetByContenaireIdAction(int id, int operation)
            => GetAll().OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == id && c.Operation == operation);

        public Log? GetByContenaireNumber(int number)
            => GetAll().OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.Container.Number == number);

        public Log? GetByContenaireByOperationStatus(int contId, int opeStatus)
            => GetAll().Where(u => u.Operation == opeStatus).OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == contId);

        public Log? GetByContenaireByOperationStatus(int contId, int opeStatusStore, int opeStatusTransfer)
            => GetAll().Where(u => u.Operation == opeStatusStore || u.Operation == opeStatusTransfer).OrderByDescending(u => u.EventTime).FirstOrDefault(c => c.ContainerID == contId);

        public Log? GetByCell(int cellId)
            => GetAll().Where(u => u.CellID == cellId && u.Container.CellID == cellId).OrderByDescending(u => u.EventTime).FirstOrDefault();

        // =========================================================
        // Optimized methods — filtering is pushed down to SQL.
        // Prefer these over the legacy methods above.
        // =========================================================

        /// <summary>
        /// Returns the most recent Log for a given container and operation type.
        /// Filtering is done in SQL (WHERE + ORDER BY + FETCH FIRST 1 ROW).
        /// Replaces <see cref="GetByContenaireIdAction"/> for performance-critical paths.
        /// </summary>
        public Log? GetByContenaireIdActionDirect(int id, int operation)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs
                .Where(c => c.ContainerID == id && c.Operation == operation)
                .OrderByDescending(u => u.EventTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the most recent Log matching a container number, with navigation properties loaded.
        /// Replaces <see cref="GetByContenaireNumber"/> for performance-critical paths.
        /// </summary>
        public Log? GetByContenaireNumberDirect(int number)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs
                .Include(l => l.Container)
                .Include(l => l.Product)
                .Include(l => l.Press)
                .Include(l => l.Shape)
                .Include(l => l.ProdColor)
                .Include(l => l.Cell)
                .Where(l => l.Container.Number == number)
                .OrderByDescending(l => l.EventTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the most recent Log for a given container and a single operation status.
        /// Replaces <see cref="GetByContenaireByOperationStatus(int, int)"/> for performance-critical paths.
        /// </summary>
        public Log? GetByContenaireByOperationStatusDirect(int contId, int opeStatus)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs
                .Where(l => l.ContainerID == contId && l.Operation == opeStatus)
                .OrderByDescending(l => l.EventTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the most recent Log for a given container matching either of two operation statuses.
        /// Replaces <see cref="GetByContenaireByOperationStatus(int, int, int)"/> for performance-critical paths.
        /// </summary>
        public Log? GetByContenaireByOperationStatusDirect(int contId, int opeStatusStore, int opeStatusTransfer)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs
                .Where(l => l.ContainerID == contId &&
                            (l.Operation == opeStatusStore || l.Operation == opeStatusTransfer))
                .OrderByDescending(l => l.EventTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the most recent Log associated with a given cell.
        /// Replaces <see cref="GetByCell"/> for performance-critical paths.
        /// </summary>
        public Log? GetByCellDirect(int cellId)
        {
            using DataKANTAIMContext ctx = new(_devModeService.DevMode);
            return ctx.Logs
                .Include(l => l.Container)
                .Where(l => l.CellID == cellId && l.Container.CellID == cellId)
                .OrderByDescending(l => l.EventTime)
                .FirstOrDefault();
        }

        // =========================================================
        // Bulk optimized methods — single DB round-trip for N items.
        // =========================================================

        /// <summary>
        /// Fetches ProdTime and StockTime for a list of containers in a single database query.
        /// Designed for bulk refresh operations (e.g. JailPge, MaintenancePge) where
        /// calling GetByContenaireIdAction per row would cause N*2 full-table reads.
        /// Only three columns are selected; no navigation properties are loaded.
        /// </summary>
        /// <param name="containerIds">List of container IDs to query.</param>
        /// <returns>
        /// Dictionary keyed by ContainerID. Each value contains:
        ///   ProdTime  — latest EventTime with operation Initisalisation
        ///   StockTime — latest EventTime with operation Store
        /// </returns>
        public Dictionary<int, (DateTime? ProdTime, DateTime? StockTime)>
            GetTimesForContainers(List<int> containerIds)
        {
            if (containerIds == null || containerIds.Count == 0)
                return new Dictionary<int, (DateTime?, DateTime?)>();

            using DataKANTAIMContext ctx = new(_devModeService.DevMode);

            // Single DB round-trip: only rows matching the IDs and relevant operations are fetched.
            var relevantLogs = ctx.Logs
                .Where(l => containerIds.Contains(l.ContainerID) &&
                            (l.Operation == OperationContainer.Initisalisation ||
                             l.Operation == OperationContainer.Store))
                .Select(l => new
                {
                    l.ContainerID,
                    l.Operation,
                    l.EventTime
                })
                .ToList();

            // In-memory grouping — no further DB calls.
            return containerIds.ToDictionary(
                id => id,
                id =>
                {
                    var logsForId = relevantLogs.Where(l => l.ContainerID == id).ToList();
                    return (
                        ProdTime: logsForId
                            .Where(l => l.Operation == OperationContainer.Initisalisation)
                            .Max(l => (DateTime?)l.EventTime),
                        StockTime: logsForId
                            .Where(l => l.Operation == OperationContainer.Store)
                            .Max(l => (DateTime?)l.EventTime)
                    );
                }
            );
        }

        /// <summary>
        /// Fetches the most recent complete Log (with navigation properties) for each container
        /// in a single batch operation.
        /// Use when full Log objects are required rather than timestamps only.
        /// </summary>
        /// <param name="containerIds">List of container IDs to query.</param>
        /// <returns>Dictionary keyed by ContainerID; value is the latest Log for that container.</returns>
        public Dictionary<int, Log> GetLatestLogsForContainers(List<int> containerIds)
        {
            if (containerIds == null || containerIds.Count == 0)
                return new Dictionary<int, Log>();

            using DataKANTAIMContext ctx = new(_devModeService.DevMode);

            // Step 1: Retrieve the latest EventTime per container — lightweight, no includes.
            var latestTimes = ctx.Logs
                .Where(l => containerIds.Contains(l.ContainerID))
                .GroupBy(l => l.ContainerID)
                .Select(g => new
                {
                    ContainerID = g.Key,
                    MaxTime = g.Max(l => l.EventTime)
                })
                .ToList();

            // Step 2: Load each latest Log with its navigation properties.
            var result = new Dictionary<int, Log>();
            foreach (var item in latestTimes)
            {
                var log = ctx.Logs
                    .Include(l => l.Product)
                    .Include(l => l.Press)
                    .Include(l => l.Shape)
                    .Include(l => l.ProdColor)
                    .Include(l => l.Cell)
                    .Include(l => l.Container).ThenInclude(x => x.ContainerType)
                    .FirstOrDefault(l => l.ContainerID == item.ContainerID && l.EventTime == item.MaxTime);

                if (log != null)
                    result[item.ContainerID] = log;
            }

            return result;
        }
    }
}
