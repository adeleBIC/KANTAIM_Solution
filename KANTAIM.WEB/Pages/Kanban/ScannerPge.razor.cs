using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Services;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Kanban
{
    public partial class ScannerPge
    {
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] IJSRuntime JS { get; set; }

        public Cell CellScanner { get; set; }
        public string? TextValue { get; set; }
        public string? ContainerValue { get; set; }
        public string? PressValue { get; set; }
        public string? ColorValue { get; set; }

        Container? ContainerScanner;
        
        [Inject] public ContenaireService _contenaireService { get; set; }
        
        [Inject] public CellService _cellService { get; set; }
        [Inject] public PressService _pressService { get; set; }

        List<string> list = new List<string>() { "Contenaire", "Bac", "Pallete"};

        string? CellName;
        string? PressName;
        string? MouleName;
        string? ColorName;
        string? ContenaireName;
        string? ProduitName;
        string? MachineName;

        private static ScannerPge _instance;

        //private ElementReference myInputElement;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _instance = this;
                await JS.InvokeVoidAsync("preventKeyboardOnTouch", "myInput");
                //await JS.InvokeVoidAsync("eval", "document.addEventListener('keydown', function (event) {if (event.key === '§') {DotNet.invokeMethodAsync('KANTAIM.WEB', 'OnSpecialKeyPressed');}});");
                await JS.InvokeVoidAsync("initializeKeyListener");
            }
        }

        [JSInvokable]
        public static void OnSpecialKeyPressed()
        {
            Console.WriteLine("La touche § a été pressée!");
            // Ici, vous pouvez appeler une méthode ou exécuter toute autre logique nécessaire
        }

        [JSInvokable]
        public static void CaptureInput(string input)
        {
            _instance?.HandleInput(input);
        }
        private void HandleInput(string input)
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

                                        ContainerScanner = _contenaireService.GetContainerByNumber(containerNumber).FirstOrDefault();
                                        if (ContainerScanner != null)
                                        {
                                            switch (ContainerScanner.ActionID)
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
            }
            else
            {
                TextValue += input;
                StateHasChanged();

            }
        }

        public void TextfieldUserInputDetected(string p, KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                if (!string.IsNullOrEmpty(p))
                {
                // Diviser la chaîne TextValue en morceaux en fonction du délimiteur #

                string[] parts = _scanService.scanCode(p);

                if(parts != null)
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

                                    ContainerScanner = _contenaireService.GetContainerByNumber(containerNumber).FirstOrDefault();
                                    if (ContainerScanner != null)
                                    {
                                        switch (ContainerScanner.ActionID)
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
            }
                


        }

    }
}