using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Model;
using KANTAIM.DAL;
using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.Components.Dialog;

namespace KANTAIM.APK.Components.Pages
{
    public partial class ScannerPge : BasePage
    {
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ProfilService _profilService { get; set; }
        [Inject] public ProfilSessionService _profilSessionService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public DevModeService _devModeService { get; set; }
        public string? TextValue { get; set; }
        private bool PwdOK = false;

        public bool DevMode
        {
            get {
                return _devModeService.DevMode;
            }
            set {
                _devModeService.DevMode = value;
                Profils = _profilService.GetAll().ToList();
            }
        }



        public List<Profil> Profils { get; set; }
        private Profil profilSelected;

        public Profil ProfilSelected
        {
            get { return profilSelected; }
            set { profilSelected = value; _profilSessionService.SetProfil(ProfilSelected); }
        }

        protected override async Task OnInitializedAsync()
        {
            DevMode = false;
#if DEBUG            
            PwdOK = true;
            DevMode = true; 
#endif
            var context = new DataKANTAIMContext();
            bool isConnected = await context.TestConnectionAsync();
            if (isConnected)
            {
                Console.WriteLine("Connexion réussie à la base de données.");
            }
            else
            {
                Console.WriteLine("Échec de la connexion à la base de données.");
            }

            Profils = _profilService.GetAll().ToList();
            ProfilSelected = _profilSessionService.CurrentProfil;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ClearTextField();
            }
        }

        private void ClearTextField()
        {
            TextValue = string.Empty;
        }

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            string p = msg.Code;

            if (ProfilSelected != null)
            {
                if (!string.IsNullOrEmpty(p))
                {
                    // Diviser la chaîne TextValue en morceaux en fonction du délimiteur #

                    string[] parts = _scanService.ParseCode(p);

                    if (parts != null)
                    {
                        string? part = parts[0];
                        // Vérifier si la partie n'est pas vide et commence par un chiffre
                        if (!string.IsNullOrEmpty(part) && int.TryParse(part, out int typeNumber))
                        {
                            // Récupérer le premier caractère (qui est le numéro du type)
                            //char typeNumber = part[0];

                            // Utiliser une structure switch pour traiter chaque type différemment
                            switch (typeNumber)
                            {
                                case 1:
                                    // Traiter le conteneur
                                    string c = parts[1];
                                    if (int.TryParse(c, out int containerNumber))
                                    {
                                        try
                                        {
                                            Container? containerScanner = _contenaireService.GetContainerByNumber(containerNumber);
                                            if (containerScanner != null)
                                            {
                                                switch (containerScanner.ContainerAction.Status)
                                                {
                                                    case 0:
                                                        if (containerScanner.ContainerType.IsContainable)
                                                        {
                                                            _snackService.Add("Ce contenaire est un bac, impossible de le scanner en premier !", MudBlazor.Severity.Error);
                                                            break;
                                                        }
                                                        /*Quand on scan un contenaire vide, on l'initialise sur press.*/
                                                        NavigationManager.NavigateTo($"/InitialisationPge/0/{containerNumber}");
                                                        break;
                                                    case 1:
                                                        /*Après initialisation, on choisie son fillstatus, et après on le mise en rack.*/
                                                        NavigationManager.NavigateTo($"/StockagePge/1/{containerNumber}");
                                                        break;
                                                    case 2:
                                                        /*Traite un contenaire qui stock avec produit, on peut sortir stock ou le déplacer.*/
                                                        NavigationManager.NavigateTo($"/ShipmentPge/1/{containerNumber}");
                                                        break;
                                                    case 3:
                                                        /*Après sortie le contenaire avec produit, on vas le mise en Machine*/
                                                        NavigationManager.NavigateTo($"/InjectPge/1/{containerNumber}", forceLoad: true);
                                                        break;
                                                    case 4:
                                                        /*Apres vidange le contenaire est vide, on Mise en rack.*/
                                                        NavigationManager.NavigateTo($"/StockagePge/1/{containerNumber}");
                                                        break;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _snackService.Add(ex.Message + containerNumber, MudBlazor.Severity.Error);
                                        }

                                    }
                                    break;
                                case 2:
                                    _snackService.Add("Impossible de scanner une machine en premier !", MudBlazor.Severity.Error);
                                    break;
                                case 3:
                                    string? pressName = parts[1];
                                    if (int.TryParse(pressName, out int pressNumber))
                                    {
                                        NavigationManager.NavigateTo($"/InitialisationPge/3/{pressNumber}");
                                    }

                                    // Traiter la presse
                                    break;
                                case 4:
                                    _snackService.Add("Impossible de scanner une cellule en premier !", MudBlazor.Severity.Error);
                                    break;
                                case 6:
                                    // Recherche le produit
                                    string? produitName = parts[1];
                                    if (int.TryParse(produitName, out int produitNumber))
                                    {
                                        NavigationManager.NavigateTo($"/FindProductPge/5/{produitNumber}");
                                    }
                                    break;
                                case 99:
                                    if (parts[1] == "17418419")
                                    {
                                        NavigationManager.NavigateTo($"/ResetContPge");
                                    }
                                    else _snackService.Add("Mauvais QRCode scanné !", MudBlazor.Severity.Error);
                                    break;
                                default:
                                    // Cas par défaut si le numéro du type n'est pas valide
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                _snackService.Add("Veuillez choisir un profil !", MudBlazor.Severity.Error);
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task ShowPwd()
        {
            var options = new DialogOptions()
            {
                BackdropClick = false,
                MaxWidth = MaxWidth.ExtraSmall,
                FullWidth = true,
                NoHeader = true,
                CloseOnEscapeKey = true
            };

            var dialog = await _dialogService.ShowAsync<PwdDialog>("Code PIN", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is bool success && success)
            {
                PwdOK = true;
            }
        }
    }

}