using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using static MudBlazor.CategoryTypes;
using KANTAIM.DAL.Model;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class ProductFamilyPge
    {
        [Inject] public ProductFamilyService _productFamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ProductFamilyVM> ProductFamilies { get; set; }
        public List<ProductVM> Products { get; set; }
        public ProductFamilyVM SelectedProductFamily { get; set; }
        private string _searchString;
        MudListItem selectedItem;
        object selectedValue;

        public bool IsEditing
        {
            get
            {
                if (SelectedProductFamily == null)
                {
                    return false;
                }
                else if (ProductFamilies.Count > 0)
                {
                    return ProductFamilies.Any(r => r.IsEditing);
                }
                else return false;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            foreach (var item in _productService.GetAll())
            {
                item.QRCode = "5#" + item.Number + "$";
                //_productService.UpSert(item);
            }
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ProductFamilyVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        private async void SelectProductFamily(ProductFamilyVM ws)
        {
            //foreach (var item in Receipes) item.IsEditing = false;
            if (IsEditing) await Cancel();
            SelectedProductFamily = ws;
            selectedValue = ws.Id;
            RefreshProductsData();
        }

        async Task SaveAsync()
        {
            RefreshData();
            await InvokeAsync(StateHasChanged);
        }

        public async Task Cancel()
        {
            RefreshData();
            if (SelectedProductFamily != null) SelectedProductFamily = ProductFamilies.SingleOrDefault(r => r.Id == SelectedProductFamily.Id);

            await InvokeAsync(StateHasChanged);
        }

        void AddAsync()
        {
            

            try
            {
                ProductFamilyVM vm = new ProductFamilyVM() { Name = "Nouvelle famille de produit", IsEditing = true };
                ProductFamilies.Insert(0, vm);
                SelectedProductFamily = vm;
                selectedValue = vm.Id;
            }
            catch (Exception ex)
            {

                _snackService.Add($"{ex.Message}{ex.InnerException.Message}", Severity.Error);
            }
            
            //await InvokeAsync(StateHasChanged);
        }

        async void EditAsync()
        {
            if (SelectedProductFamily != null) SelectedProductFamily.IsEditing = true;
        }

        async Task SaveProductAsync()
        {
            RefreshProductsData();
            await InvokeAsync(StateHasChanged);
        }

        async Task CancelProductAsync()
        {
            RefreshProductsData();
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteAsync()
        {
            await _dialogService.Confirm($"Souhaitez-vous le product family sélectionné ?", () =>
            {
                ProductFamilies.Remove(SelectedProductFamily);
                if (SelectedProductFamily.Id != 0) _productFamilyService.Delete(SelectedProductFamily.Id);
                SelectedProductFamily = null;
                _snackService.Add("Données supprimées !", Severity.Success);
            });

            await InvokeAsync(StateHasChanged);
        }

        public string RowClassFct(WorkshopVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            //_WorkshopService.ResetCache();
            ProductFamilies = _productFamilyService.GetAll().Select(u => new ProductFamilyVM(u)).ToList();
        }

        void RefreshProductsData()
        {
            //_WorkshopService.ResetCache();
            if (SelectedProductFamily != null) Products = _productService.GetAllPerProductFamily(SelectedProductFamily.Id).Select(u => new ProductVM(u)).OrderBy(p => p.Name).ToList();
        }
    }
}