using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.DAL;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class ColorProductPge
    {
        [Inject] public ProductFamilyService _productFamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _ColorService { get; set; }
        [Inject] public ColorProductService _ColorProductService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ProductFamilyVM> ProductFamilies { get; set; }
        public List<ColorVM> Colors { get; set; }
        public List<ProductVM> Products { get; set; }
        public ProductFamilyVM SelectedProductFamily { get; set; }
        private string _searchString;
        MudListItem selectedItem;
        object selectedValue;

        public HashSet<TreeItemData> ProductTreeItems { get; set; } = new HashSet<TreeItemData>();
        public HashSet<TreeItemData> ColorTreeItems { get; set; } = new HashSet<TreeItemData>();
        public TreeItemData ProductActivatedValue { get; set; }
        public TreeItemData ColorActivatedValue { get; set; }
        public HashSet<TreeItemData> ProductSelectedValues { get; set; }
        public HashSet<TreeItemData> ColorSelectedValues { get; set; }

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
                // if we active a Color
                case 0:
                    ProductActivatedValue = null;
                    ColorActivatedValue = item; break;
                // if we active a product
                case 1:
                    ColorActivatedValue = null;
                    ProductActivatedValue = item; break;
            }
            await Task.Run(RefreshData);
        }


        void RefreshData()
        {
            ColorTreeItems.Clear();
            ProductTreeItems.Clear();
            Colors = _ColorService.GetAll().Select(u => new ColorVM(u)).ToList();
            foreach (var Color in Colors)
            {
                if (ProductActivatedValue != null && _ColorProductService.FindLink(Color.Id, ProductActivatedValue.Id))
                {
                    ColorTreeItems.Add(new TreeItemData(Color.Id, Color.Name, true));
                }
                else
                {
                    ColorTreeItems.Add(new TreeItemData(Color.Id, Color.Name, false));
                }
            }
            ProductFamilies = _productFamilyService.GetAll().Select(u => new ProductFamilyVM(u)).ToList();
            foreach (var family in ProductFamilies)
            {
                var familyNode = new TreeItemData(family.Id, family.Name, false)
                {
                    TreeItems = new HashSet<TreeItemData>()
                };

                // Fetch products for each family
                var products = _productService.GetAllPerProductFamily(family.Id).Select(u => new ProductVM(u)).ToList();

                if (products.Count > 0)
                    familyNode.IsExpanded = true;

                foreach (var product in products)
                {
                    if (ColorActivatedValue != null && _ColorProductService.FindLink(ColorActivatedValue.Id, product.Id))
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
            foreach (var ColorItem in ColorSelectedValues)
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
                        _ColorProductService.DeleteLink(ColorItem.Id, productItem.Id);
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

            if (ColorSelectedValues.Count != 0)
            {
                _snackService.Add("Enlever les liens avec succès.", Severity.Success);
            }
        }

        async Task VerifyAsync()
        {

            foreach (var ColorItem in ColorSelectedValues)
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
                        if (!_ColorProductService.FindLink(ColorItem.Id, productItem.Id))
                        {
                            var newColorProduct = new ColorProduct
                            {
                                ColorID = ColorItem.Id,
                                ProductID = productItem.Id
                            };

                            _ColorProductService.UpSert(newColorProduct);
                        }
                          

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

            if (ColorSelectedValues.Count != 0)
            {
                _snackService.Add("Couleurs liens vers les produits ont été mis à jour avec succès.", Severity.Success);
            }
                
        }
    }
}