using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KANTAIM.WEB.Pages.Kanban
{
    public partial class TransferPge
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public LogService _logService { get; set; }
        public string? ContainerValue { get; set; }
        public string? autreValue { get; set; }
        public Container? ContainerScanner { get; set; }
        public Container? AutreScanner { get; set; }
        public Log logRescent { get; set; }
        public bool bienTransfer { get; set; } = false;



        void containerScan(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                string[] parts = _scanService.scanCode(ContainerValue);
                if (parts != null)
                {
                    int.TryParse(parts[0], out int type);
                    if (type != 1)
                    {
                        _snackService.Add("Svp scanner le QR code de la Contenaire.", Severity.Error);
                    }
                    else
                    {
                        int.TryParse(parts[1], out int Number);
                        ContainerScanner = _contenaireService.GetContainerByNumber(Number).FirstOrDefault();
                        logRescent = _logService.GetByContenaireId(Number);
                    }
                }
            }
        }

        void autreScan(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                string[] parts = _scanService.scanCode(autreValue);
                if (parts != null)
                {
                    int.TryParse(parts[0], out int type);
                    if (type != 1)
                    {
                        _snackService.Add("Svp scanner le QR code de la Contenaire.", Severity.Error);
                    }
                    else
                    {
                        int.TryParse(parts[1], out int Number);
                        AutreScanner = _contenaireService.GetContainerByNumber(Number).FirstOrDefault();
                        if(AutreScanner.FillStatus != 1)
                        {
                            _snackService.Add("Svp vérifier la Contenaire est vide.", Severity.Error);
                            AutreScanner = null;
                        }
                    }
                }
            }
        }

        void transferInformation()
        {
            AutreScanner.CellId = ContainerScanner.CellId;
            AutreScanner.ActionID = ContainerScanner.ActionID;
            AutreScanner.FillStatus = ContainerScanner.FillStatus;
            AutreScanner.Status = ContainerScanner.Status;
            AutreScanner.InJail = ContainerScanner.InJail;
            AutreScanner.InMaintenance = ContainerScanner.InMaintenance;
            AutreScanner.Comment = ContainerScanner.Comment;
            _contenaireService.UpSert(AutreScanner);

            ContainerScanner.FillStatus = 1; // vide
            ContainerScanner.ActionID = 0;
            _contenaireService.UpSert(ContainerScanner);



            Log u_autre = new Log()
            {
                EventTime = DateTime.Now,
                Operation = 6, // Mise en contenaire
                ProductID = logRescent.ProductID,
                Press = logRescent.Press,
                PressID = logRescent.PressID,
                Shape = logRescent.Shape,
                ShapeID = logRescent.ShapeID,
                Container = AutreScanner,
                ContainerID = AutreScanner.Id,
                ProdColor = logRescent.ProdColor,
                ProdColorID = logRescent.ProdColorID,
                CellID = logRescent.CellID,
                FillStatus = logRescent.FillStatus
            };
            _logService.UpSert(u_autre);

            Log u_contenaire = new Log()
            {
                EventTime = DateTime.Now,
                Operation = 6, // Mise en contenaire
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                FillStatus = ContainerScanner.FillStatus
            };
            _logService.UpSert(u_contenaire);

            bienTransfer = true;
        }
    }
}