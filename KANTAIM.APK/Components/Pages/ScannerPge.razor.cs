using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Model;
using Microsoft.AspNetCore.Components.Web;
using KANTAIM.DAL;
using static Android.Renderscripts.ScriptGroup;

namespace KANTAIM.APK.Components.Pages
{
    public partial class ScannerPge
    {
        [Inject]private IJSRuntime JSRuntime { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] public PressService _pressService { get; set; }

        public KANTAIM.DAL.Model.Cell CellScanner { get; set; }
        public string? TextValue { get; set; }

        private string currentUrl;
        private string pageUrl;
        string? PressName;
        string? ProduitName;
        string? MachineName;

        private static ScannerPge _instance;

        //private MudTextField<string> scanField;

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            currentUrl = NavigationManager.Uri;
            pageUrl = NavigationManager.Uri;
            _instance = this;
            NavigationManager.LocationChanged += OnLocationChanged;

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
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            // Mettre à jour l'URL actuelle lorsque l'URL change
            currentUrl = e.Location;
            // Vous pouvez ajouter ici toute logique que vous souhaitez exécuter lorsque l'URL change
        }

        public void Dispose()
        {
            // Se désabonner de l'événement pour éviter les fuites de mémoire
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        private string LastKey { get; set; }

        [JSInvokable]
        public static void CaptureInput(string key)
        {
            _instance?.HandleInput(key);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ClearTextField();
            }
        }

        private async void HandleInput(string input)
        {
            if (currentUrl == pageUrl)
            {
                if (input == "Enter")
                {
                    string p = TextValue;
                    if (!string.IsNullOrEmpty(p))
                    {
                        // Diviser la chaîne TextValue en morceaux en fonction du délimiteur #

                        string[] parts = _scanService.scanCode(p);

                        if (parts != null)
                        {
                            string part = parts[0];
                            // Vérifier si la partie n'est pas vide et commence par un chiffre
                            if (!string.IsNullOrEmpty(part) && char.IsDigit(part[0]))
                            {
                                // Récupérer le premier caractère (qui est le numéro du type)
                                char typeNumber = part[0];

                                // Utiliser une structure switch pour traiter chaque type différemment
                                switch (typeNumber)
                                {
                                    case '1':
                                        // Traiter le conteneur
                                        string c = parts[1];
                                        if (int.TryParse(c, out int containerNumber))
                                        {

                                            Container? containerScanner = _contenaireService.GetContainerByNumber(containerNumber);
                                            if (containerScanner != null)
                                            {
                                                switch (containerScanner.ContainerAction.Status)
                                                {
                                                    case 0:
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
                                                        NavigationManager.NavigateTo($"/InjectPge/1/{containerNumber}");
                                                        break;
                                                    case 4:
                                                        /*Apres vidange le contenaire est vide, on Mise en rack.*/
                                                        NavigationManager.NavigateTo($"/StockagePge/1/{containerNumber}");
                                                        break;
                                                }
                                            }

                                        }
                                        break;
                                    case '2':
                                        MachineName = parts[1];
                                        if (int.TryParse(MachineName, out int MachineNumber))
                                        {
                                            NavigationManager.NavigateTo($"/InjectPge/2/{MachineNumber}");
                                        }
                                        break;
                                    case '3':
                                        PressName = parts[1];
                                        if (int.TryParse(PressName, out int PressNumber))
                                        {
                                            NavigationManager.NavigateTo($"/InitialisationPge/3/{PressNumber}");
                                        }

                                        // Traiter la presse
                                        break;
                                    case '4':
                                        // Traiter la cell
                                        string X = parts[1];
                                        string Y = parts[2];

                                        if (int.TryParse(X, out int x) && int.TryParse(Y, out int y))
                                        {
                                            CellScanner = _cellService.GetByXY(x, y);
                                            //NavigationManager.NavigateTo($"/StockagePge/4/{CellScanner.Id}");
                                        }
                                        break;
                                    case '5':
                                        // Recherche le produit
                                        ProduitName = parts[1];
                                        if (int.TryParse(ProduitName, out int ProduitNumber))
                                        {
                                            NavigationManager.NavigateTo($"/FindProductPge/5/{ProduitNumber}");
                                        }
                                        break;
                                    default:
                                        // Cas par défaut si le numéro du type n'est pas valide
                                        break;
                                }
                            }
                        }

                    }

                    TextValue = null;
                    //_snackService.Add("Mauvais QRCode scanné !",MudBlazor.Severity.Error);
                }
                else
                {
                    TextValue += input;
                    //StateHasChanged();
                    await InvokeAsync(StateHasChanged);
                }
            }

        }

        private void ClearTextField()
        {
            TextValue = string.Empty;
            //scanField.FocusAsync();
        }

    }

}