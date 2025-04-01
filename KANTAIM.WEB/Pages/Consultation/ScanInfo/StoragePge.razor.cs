using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Components.ProductfamilyComp;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.WEB.Ressources;


namespace KANTAIM.WEB.Pages.Consultation.ScanInfo
{
    public partial class StoragePge
    {
        [Inject] public ProductFamilyService _productFamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        public List<ProductFamilyVM> ProductFamilies { get; set; }
        public TreeItemData? SelectedValue { get; set; }
        public Product? selectedProduct { get; set; }
        public ProductFamily? selectedProductFamily { get; set; }
        public Dictionary<int, List<Container>> productContainer { get; set; } = new Dictionary<int, List<Container>>();
        public List<Container> InfoContainer { get; set; }
        public double totals { get; set; }
        public List<CellLog> cells { get; set; }
        public Dictionary<int, string> CellStatus { get; set; }

        public class CellLog
        {
            public DAL.Model.Cell Cell { get; set; }
            public DateTime EventTime { get; set; }
        }
        public class TreeItemData
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool IsExpanded { get; set; }
            public bool IsSelected { get; set; }
            public bool IsFamily { get; set; }

            public HashSet<TreeItemData> TreeItems { get; set; }

            public TreeItemData(int id, string name, bool isFamily)
            {
                Id = id;
                Name = name;
                IsFamily = isFamily;
            }
        }

        public HashSet<TreeItemData> ProductTreeItems { get; set; } = new HashSet<TreeItemData>();
        public TreeItemData? ProductActivatedValue { get; set; }

        // Function to build the product tree
        private void BuildProductTree()
        {
            ProductFamilies = _productFamilyService.GetAll()
                .OrderBy(p => p.Name)
                .Select(u => new ProductFamilyVM(u))
                .ToList();

            foreach (var family in ProductFamilies)
            {
                var familyNode = new TreeItemData(family.Id, family.Name, true)
                {
                    TreeItems = new HashSet<TreeItemData>()
                };

                // Fetch products for each family
                var products = _productService.GetAllPerProductFamily(family.Id)
                    .OrderBy(p => p.Name)
                    .Select(u => new ProductVM(u))
                    .ToList();

                foreach (var product in products)
                {
                    familyNode.TreeItems.Add(new TreeItemData(product.Number, product.Name, false));
                }

                ProductTreeItems.Add(familyNode);
            }
        }

        // Function to populate the product container dictionary
        private void PopulateProductContainer()
        {
            var containers = _contenaireService.GetAll().Where(c => c.ActionID == 5); // All containers in the storage status with a product.

            var count = containers.Count();
            foreach (var container in containers)
            {
                var logRescent = _logService.GetByContenaireNumber(container.Number);

                if (logRescent != null && logRescent.Product != null)
                {
                    // Check if the product is already in the dictionary
                    if (productContainer.ContainsKey(logRescent.Product.Number))
                    {
                        // Add the container to the existing list
                        productContainer[logRescent.Product.Number].Add(container);
                    }
                    else
                    {
                        // Create a new entry with the product and container
                        productContainer[logRescent.Product.Number] = new List<Container> { container };
                    }
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            BuildProductTree();
            PopulateProductContainer();
            //RefreshData();
            await Task.Run(RefreshData);
        }

        private void findCells()
        {
            foreach (Container container in _contenaireService.GetAll().Where(c => c.CellStock != null))
            {
                var logRescent = _logService.GetByContenaireByOperationStatus(container.Id, OperationContainer.Store, OperationContainer.Transfer);
                if (logRescent != null && logRescent.Product != null && logRescent.Product.Number == selectedProduct.Number)
                {
                    if (!cells.Any(c => c.Cell.Id == container.CellStock.Id))
                    {
                        cells.Add(new CellLog { Cell = container.CellStock, EventTime = logRescent.EventTime });
                    }
                }
            }
        }

        async Task SelectAsync(TreeItemData item)
        {
            SelectedValue = item;
            await Task.Run(RefreshData);
        }

        void RefreshData()
        {
            cells = new List<CellLog>();
            CellStatus = new StatusCell().Status;
            totals = 0;
           if(SelectedValue != null)
            {
                if(SelectedValue.IsFamily)
                {
                    selectedProductFamily = _productFamilyService.GetById(SelectedValue.Id);
                } else
                {
                    selectedProduct = _productService.GetByNumber(SelectedValue.Id);
                    if (productContainer.ContainsKey(selectedProduct.Number))
                    {
                        findCells();
                        InfoContainer = productContainer[selectedProduct.Number];
                        foreach (var container in InfoContainer)
                        {
                            totals += container.ContainerType.NbrMaxContainer * (container.FillStatus == StatusContainer.HalfFull ? 0.5 : 1);
                        }
                    }
                }
                
            }
        }
    }
}