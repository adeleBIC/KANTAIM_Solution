using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Components.ProductfamilyComp
{
    public partial class ProductDetail
    {
        [Inject] public ProductService _productService { get; set; }
        [Inject] public CellProductService _cellProductService { get; set;}
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        [Parameter] public EventCallback OnSaved { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        [Parameter] public ProductFamilyVM ProductFamily { get; set; }
        [Parameter] public List<ProductVM> Products { get; set; }

        private string _searchString;

        void Add()
        {
            ProductVM item = new ProductVM() { IsEditing = true };
            Products.Insert(0,item);
            //await InvokeAsync(StateHasChanged);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ProductVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }

        async Task SaveAsync()
        {
            foreach (ProductVM vm in Products.Where(vm => vm.IsEditing))
            {
                vm.ProductFamilyID = ProductFamily.Id;
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    Product u = (Product)vm;
                    _productService.UpSert(u);
                    vm.IsEditing = false;

                    _snackService.Add("Données sauvgardées !", Severity.Success);
                }
                else
                {
                    string txt = "<ul>";
                    foreach (ValidationResult item in validationResults)
                        txt += $"<li>{item.ErrorMessage}</li>";

                    txt += "</ul>";
                    _snackService.Add(txt, Severity.Error);
                }
            }
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteAsync()
        {
            var list = Products.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (ProductVM item in list)
                    {
                        if (item.Id != 0)
                        {
                            foreach (CellProduct lier in _cellProductService.GetAllPerProduct(item.Id)) {
                                _cellProductService.Delete(lier.Id);
                            }
                            _productService.Delete(item.Id);
                            RefreshData();
                            _snackService.Add("Données supprimées !", Severity.Success);
                        }
                    }
                });
            }

            await OnSaved.InvokeAsync(StateHasChanged);
        }

        async Task CancelAsync()
        {
            RefreshData();
            await OnCancel.InvokeAsync(StateHasChanged);
        }

        public string RowClassFct(ProductVM vm, int row)
        {
            return vm.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            if (ProductFamily != null) Products = _productService.GetAllPerProductFamily(ProductFamily.Id).Select(u => new ProductVM(u)).OrderBy(p => p.Name).ToList();
        }

        void SelectionChanged(HashSet<ProductVM> changes)
        {
            foreach (var u in Products)
                u.IsChecked = changes.Contains(u);
        }
    }
}