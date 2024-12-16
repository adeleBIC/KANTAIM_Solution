using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using static MudBlazor.CategoryTypes;
using System.Linq;
using KANTAIM.WEB.Components.ProductfamilyComp;
using KANTAIM.DAL.Model;
using KANTAIM.DAL;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class CellProductPge
    {
        [Inject] public ProductFamilyService _productFamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] public CellProductService _cellProductService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ProductFamilyVM> ProductFamilies { get; set; }
        public List<CellVM> cells { get; set; }
        public List<ProductVM> Products { get; set; }
        public ProductFamilyVM SelectedProductFamily { get; set; }
        private string _searchString;
        MudListItem selectedItem;
        object selectedValue;

        public HashSet<TreeItemData> ProductTreeItems { get; set; } = new HashSet<TreeItemData>();
        public HashSet<TreeItemData> CellTreeItems { get; set; } = new HashSet<TreeItemData>();
        public TreeItemData? ProductActivatedValue { get; set; }
        public TreeItemData? CellActivatedValue { get; set; }
        public HashSet<TreeItemData> ProductSelectedValues { get; set; }
        public HashSet<TreeItemData> CellSelectedValues { get; set; }

        public class TreeItemData
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool IsExpanded { get; set; }
            public bool IsSelected { get; set; }
            public bool IsActivated { get; set; }

            public HashSet<TreeItemData> TreeItems { get; set; }

            public TreeItemData(int id, string name, bool isSelected)
            {
                Id = id;
                Name = name;
                IsSelected = isSelected;
            }
        }
        protected override async Task OnInitializedAsync()
        {

            //RefreshData();
            await Task.Run(RefreshData);
        }

        bool Editer = false;

        private void OnExpandCollapseClick()
        {
            Editer = !Editer;
            Task.Run(RefreshData);
        }

        async Task ActiveAsync(TreeItemData item, int type)
        {
            switch (type)
            {
                // if we active a cell
                case 0:
                    ProductActivatedValue = null;
                    CellActivatedValue = item; break;
                // if we active a product
                case 1:
                    CellActivatedValue = null;
                    ProductActivatedValue = item; break;
            }
            await Task.Run(RefreshData);
        }


        void RefreshData()
        {
            CellTreeItems.Clear();
            ProductTreeItems.Clear();
            cells = _cellService.GetAll().OrderBy(c => c.Name.Length).ThenBy(c => c.Name).Select(u => new CellVM(u)).ToList();
            foreach (var cell in cells)
            {
                if (ProductActivatedValue != null && _cellProductService.FindLink(cell.Id, ProductActivatedValue.Id))
                {
                    CellTreeItems.Add(new TreeItemData(cell.Id, cell.Name, true));
                }
                else
                {
                    CellTreeItems.Add(new TreeItemData(cell.Id, cell.Name, false));
                }
            }
            ProductFamilies = _productFamilyService.GetAll().OrderBy(p => p.Name).Select(u => new ProductFamilyVM(u)).ToList();
            foreach (var family in ProductFamilies)
            {
                var familyNode = new TreeItemData(family.Id, family.Name, false)
                {
                    TreeItems = new HashSet<TreeItemData>()
                };

                // Fetch products for each family
                var products = _productService.GetAllPerProductFamily(family.Id).OrderBy(p => p.Name).Select(u => new ProductVM(u)).ToList();

                if (products.Count > 0)
                    familyNode.IsExpanded = true;

                foreach (var product in products)
                {
                    if (CellActivatedValue != null && _cellProductService.FindLink(CellActivatedValue.Id, product.Id))
                    {
                        // Create a new TreeItemData object for each product and add to the family node
                        familyNode.TreeItems.Add(new TreeItemData(product.Id, product.Name, true));
                    }
                    else
                    {
                        familyNode.TreeItems.Add(new TreeItemData(product.Id, product.Name, false));
                    }

                }
                // Add the family node to the TreeItems set
                ProductTreeItems.Add(familyNode);
            }

        }

        async Task CancelLink()
        {
            foreach (var cellItem in CellSelectedValues)
            {
                //_ColorProductService.DeleteAllByColorId(ColorItem.Id);
                foreach (var productItem in ProductSelectedValues)
                {
                    // Only add product, not the productFamily
                    if (productItem.TreeItems == null)
                    {
                        // Check if the Color and product combination already exists in the database

                        //if (!_ColorProductService.FindLink(ColorItem.Id, productItem.Id))
                        //{
                        // If the link does not exist, create it and add to the database
                        _cellProductService.DeleteLink(cellItem.Id, productItem.Id);
                        //}
                    }
                }
            }
            // If using Entity Framework, save changes to the database
            // If you use some other method to batch insert, do it here.
            using (DataKANTAIMContext ctx = new DataKANTAIMContext())
            {
                await ctx.SaveChangesAsync();
            }

            if (CellSelectedValues.Count != 0)
            {
                _snackService.Add("Enlever les liens avec succès.", Severity.Success);
            }
        }

        async Task VerifyAsync()
        {
            
            foreach (var cellItem in CellSelectedValues)
            {
                foreach (var productItem in ProductSelectedValues)
                {
                    // Only add product, not the productFamily
                    if (productItem.TreeItems == null)
                    {
                        // Check if the cell and product combination already exists in the database

                        if (!_cellProductService.FindLink(cellItem.Id, productItem.Id))
                        {
                            // If the link does not exist, create it and add to the database
                            var newCellProduct = new CellProduct
                            {
                                CellID = cellItem.Id,
                                ProductID = productItem.Id
                            };

                            _cellProductService.UpSert(newCellProduct);

                        }
                    }

                }


            }
            // If using Entity Framework, save changes to the database
            // If you use some other method to batch insert, do it here.
            using (DataKANTAIMContext ctx = new DataKANTAIMContext())
            {
                await ctx.SaveChangesAsync();
            }
            if (CellSelectedValues.Count != 0)
                // Optionally, you can send a notification to the user about the completion of the process
                _snackService.Add("Cellules liens vers les produits ont été mis à jour avec succès.", Severity.Success);
        }
    }

}
