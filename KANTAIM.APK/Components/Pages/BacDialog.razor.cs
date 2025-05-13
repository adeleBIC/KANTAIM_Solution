using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;


namespace KANTAIM.APK.Components.Pages
{
    public partial class BacDialog
    {
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public LogService _logService { get; set; }

        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public List<Container> BacList { get; set; }

        void Cancel() => MudDialog.Cancel();

        void RemoveBac(Container bac)
        {
            bac.ContainerAction = _actionService.GetByStatus(0);/*0: Stocké vide*/
            bac.ActionID = bac.ContainerAction.Id;
            bac.BigContainer = null;
            bac.ContainerID = null;
            bac.PressID = null;
            bac.ProdColorID = null;
            bac.ProductID = null;
            _contenaireService.UpSert(bac);
            BacList.Remove(bac);
            // Vous pouvez également ajouter des appels à des services ici pour supprimer dans la base de données
        }

        //[Parameter]
        //public List<ContainerInfo> BacList { get; set; }

        //public class ContainerInfo
        //{
        //    public Container container { get; set; }
        //    public DateTime EventTime { get; set; }
        //}

        //void Cancel() => MudDialog.Cancel();

        //void RemoveBac(Container bac)
        //{
        //    bac.ContainerAction = _actionService.GetByStatus(0); /* 0: Stocké vide */
        //    bac.ActionID = bac.ContainerAction.Id;
        //    bac.BigContainer = null;
        //    bac.ContainerID = null;
        //    _contenaireService.UpSert(bac);
        //    BacList.RemoveAll(b => b.container.Id == bac.Id); // Removing the container based on Id
        //}
    }
}