using LO_ASM_MATE_CONTROL.Core;
using LO_ASM_MATE_CONTROL.MVVM.Models;
using LO_ASM_MATE_CONTROL.Services.ApiData;
using LO_ASM_MATE_CONTROL.Services.AsmXmlFiles;
using LO_ASM_MATE_CONTROL.Services.Commanders;
using LO_ASM_MATE_CONTROL.Services.Navigation;
using LO_ASM_MATE_CONTROL.Services.Providers;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace LO_ASM_MATE_CONTROL.MVVM.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private INavigationService _navigationService;
        private readonly IProgramExecutioner _pExec;
        private readonly IAsmFileGen _asmFileGen;
        private readonly IPackagingUnitProvider _packagingUnitProvider;
        private readonly IApiService _apiService;


        private List<ComponentStockLocation> _componentStockLocations = new List<ComponentStockLocation>();
        public List<ComponentStockLocation> ComponentStockLocations { get => _componentStockLocations; set => _componentStockLocations = value; }
        public RelayCommand GenerateGPAOStockCommand { get; set; }
        public RelayCommand ExportTotalQuantitiesCommand { get; set; }


        public MainWindowViewModel(INavigationService navigationService,
            IProgramExecutioner pExec,
            IAsmFileGen asmFileGen,
            IPackagingUnitProvider packagingUnitProvider,
            IApiService apiService)
        {
            _navigationService = navigationService;
            _pExec = pExec;
            _asmFileGen = asmFileGen;
            _packagingUnitProvider = packagingUnitProvider;
            _apiService = apiService;

            NavigateToHomeViewModel = new RelayCommand(_ => true, parameter => { NavigationService.NavigateTo<HomeViewModel>(); });
            NavigateToPackagingUnitManagementViewModel = new RelayCommand(_ => true, parameter => { NavigationService.NavigateTo<PackagingUnitManagementViewModel>(); });
            NavigateToAsmVisualizationDatabaseViewModel = new RelayCommand(_ => true, parameter => { NavigationService.NavigateTo<AsmVisualizationDatabaseViewModel>(); });
            NavigateToExitSubContractingViewModel = new RelayCommand(_ => true, parameter => { NavigationService.NavigateTo<ExitSubContractingViewModel>(); });

            GenerateStockCommand = new RelayCommand(_ => true, _ => GenerateStockCms());
            UpdateWarehouseCommand = new RelayCommand(_ => true, _ => UpdateStockLocations());
            UpdateStoreLocationCommand = new RelayCommand(_ => true, _ => UpdateStoreLocationFromApi());

            // ✅ Nouvelle commande pour générer le stock GPAO
            GenerateGPAOStockCommand = new RelayCommand(_ => true, async _ => await GenerateCsvFileGpao());

            SearchArticleQuantityCommand = new RelayCommand(_ => true, _ => SearchArticleQuantity());
            ExportTotalQuantitiesCommand = new RelayCommand(_ => true, async _ => await ExportTotalQuantitiesToCsv());


        }

        private void SearchArticleQuantity()
        {
            var searchWindow = new LO_ASM_MATE_CONTROL.MVVM.Views.SearchArticleWindow(_packagingUnitProvider);
            searchWindow.Owner = Application.Current.MainWindow;
            searchWindow.ShowDialog();
        }

        

        private async Task GenerateCsvFileGpao()
        {
            var filePath = Path.GetFullPath("INV_CMS.CSV");
            var temporaryPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                var packagingUnits = _packagingUnitProvider.GetAllPackagingUnits();
                // Filtrer les bobines avant regroupement : stock propre dans les magasins autorisés.
                var groupedQuantities = packagingUnits
                    .Where(pu => !string.IsNullOrWhiteSpace(pu.ComponentBarcode)
                        && LO_ASM_MATE_CONTROL.Services.InventoryCsvGenerator.IsEligiblePackagingUnit(
                            pu.UID, pu.Extra2, pu.Extra1))
                    .GroupBy(pu => pu.ComponentBarcode.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Sum(pu => pu.Quantity), StringComparer.OrdinalIgnoreCase);

                var lines = await LO_ASM_MATE_CONTROL.Services.InventoryCsvGenerator.BuildLinesAsync(
                    groupedQuantities, _apiService.GetArticleByArticleUid);

                // Ne remplacer le fichier qu'après récupération et validation de tous les articles.
                File.WriteAllLines(temporaryPath, lines, Encoding.UTF8);
                if (File.Exists(filePath))
                    File.Replace(temporaryPath, filePath, null);
                else
                    File.Move(temporaryPath, filePath);

                Log.Information("Inventaire généré : {FilePath}, {ArticleCount} articles.", filePath, lines.Count);
                MessageBox.Show($"Le fichier {filePath} a été généré avec {lines.Count} articles.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erreur lors de la génération de l'inventaire.");
                MessageBox.Show($"Erreur lors de la génération du fichier.\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
        private async Task ExportTotalQuantitiesToCsv(string filePath = "StockByArticle.csv")
        {
            try
            {
                // Récupérer toutes les unités d’emballage
                var packagingUnits = _packagingUnitProvider.GetAllPackagingUnits();

                // Grouper par code article et sommer les quantités
                var grouped = packagingUnits
                    .GroupBy(pu => pu.ComponentBarcode)
                    .Select(g => new
                    {
                        ArticleCode = g.Key,
                        TotalQuantity = g.Sum(pu => pu.Quantity)
                    })
                    .OrderBy(x => x.ArticleCode) // Optionnel : tri par code article
                    .ToList();

                // Construire les lignes CSV
                var lines = new List<string>
                {
                    "ArticleCode;TotalQuantity" // En-tête
                };
                lines.AddRange(grouped.Select(x => $"{x.ArticleCode};{x.TotalQuantity}"));

                // Écrire dans le fichier
                File.WriteAllLines(filePath, lines, Encoding.UTF8);

                Log.Information("✅ Fichier CSV généré : {FilePath}", filePath);
                MessageBox.Show($"✅ Fichier CSV généré : {filePath}", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erreur lors de la génération du fichier CSV des quantités totales.");
                MessageBox.Show($"❌ Erreur lors de la génération du CSV.\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async Task UpdateStoreLocationFromApi()
        {
            Log.Information("UpdateStoreLocationFromApi called.");
            try
            {
                var articles = await _apiService.GetAllArticles();

                foreach (var article in articles)
                {
                    Log.Information(article.ToString());
                    int res = await _packagingUnitProvider.UpdatePackagingUnitsMagasinAsync(article.ArticleUid, article.Store);
                }

                Log.Information("Mise à jour des emplacements magasin terminée avec succès.");
                MessageBox.Show("✅ Mise à jour des emplacements magasin terminée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex) when (ex.Number == 229)
            {
                Log.Error(ex, "Droits SQL insuffisants pour mettre à jour les emplacements de magasin.");
                MessageBox.Show(
                    "Impossible de mettre à jour les emplacements magasin : le compte SQL/Windows utilisé par l'application n'a pas le droit UPDATE sur dbo.PackagingUnit.\n\n" +
                    $"Compte Windows courant : {WindowsIdentity.GetCurrent().Name}\n" +
                    "Demandez l'ajout du droit UPDATE sur la base SiplaceSetupCenter, table dbo.PackagingUnit.",
                    "Droits SQL insuffisants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erreur lors de la mise à jour des emplacements de magasin.");
                MessageBox.Show($"❌ Erreur lors de la mise à jour des emplacements de magasin.\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateStockLocations()
        {
            try
            {
                UpdateListOfLocationsForPackagingUnit();

                var filePath = "D:\\WarehouseStockLocation\\StockLocation_Horizon.xml";
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                _asmFileGen.GenerateXmlFile(ComponentStockLocations, filePath);

                Log.Information("Fichier des emplacements magasin généré avec succès.");
                MessageBox.Show("✅ Fichier des emplacements magasin généré avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erreur lors de la génération du fichier des emplacements magasin.");
                MessageBox.Show($"❌ Erreur lors de la génération du fichier des emplacements magasin.\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateListOfLocationsForPackagingUnit()
        {
            int cpt = 0;
            ComponentStockLocations.Clear();
            var packagingUnits = _packagingUnitProvider.GetAllPackagingUnits();

            foreach (var unit in packagingUnits)
            {
                if (string.IsNullOrWhiteSpace(unit.Extra2))
                {
                    ComponentStockLocations.Add(new ComponentStockLocation()
                    {
                        Component = "Horizon Telecom\\" + unit.ComponentBarcode,
                        StockLocation = unit.Extra1
                    });
                    cpt++;
                }
            }

            Log.Information("COMPTE BOBINE : {Cpt}", cpt);
        }


        private void GenerateStockCms()
        {
            try
            {
                _pExec.ExecuteProgram("C:\\Users\\ASMPT\\Desktop\\GEN DB STOCK BASE ASMPT\\TrainSetupCenterManager.exe",
                                      "C:\\Users\\ASMPT\\Desktop\\GEN DB STOCK BASE ASMPT\\");

                Log.Information("Feuille d’inventaire générée avec succès.");
                MessageBox.Show("✅ Feuille d’inventaire générée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erreur lors de la génération de la feuille d’inventaire.");
                MessageBox.Show($"❌ Erreur lors de la génération de la feuille d’inventaire.\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public INavigationService NavigationService
        {
            get { return _navigationService; }
            set
            {
                _navigationService = value;
                OnPropertyChanged(nameof(NavigationService));
            }
        }

        public RelayCommand NavigateToPackagingUnitManagementViewModel { get; set; }
        public RelayCommand NavigateToHomeViewModel { get; set; }
        public RelayCommand NavigateToAsmVisualizationDatabaseViewModel { get; set; }
        public RelayCommand NavigateToExitSubContractingViewModel { get; set; }
        public RelayCommand GenerateStockCommand{ get; set; }
        public RelayCommand UpdateWarehouseCommand { get; set; }
        public RelayCommand UpdateStoreLocationCommand { get; set; }
        public RelayCommand SearchArticleQuantityCommand { get; set; }


    }
}
