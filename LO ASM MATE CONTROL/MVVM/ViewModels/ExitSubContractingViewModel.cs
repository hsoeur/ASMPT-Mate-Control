using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using LO_ASM_MATE_CONTROL.Core;
using LO_ASM_MATE_CONTROL.Services.Providers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LO_ASM_MATE_CONTROL.MVVM.ViewModels
{

    public class ExitSubContractingViewModel : ViewModel
    {
        private readonly IPackagingUnitProvider _packagingUnitProvider;

        public ExitSubContractingViewModel(IPackagingUnitProvider packagingUnitProvider)
        {
            _packagingUnitProvider = packagingUnitProvider;
            SearchCommand = new RelayCommand(_ => true, _ => ExecuteSearch());
            GenerateCsvCommand = new RelayCommand(_ => true, _ => GenerateCsvFile());
            PackagingUnits = new ObservableCollection<PackagingUnit>();
        }

        // Commande pour la recherche
        public ICommand SearchCommand { get; }
        public ICommand GenerateCsvCommand { get; }

        private ObservableCollection<PackagingUnit> _packagingUnits;

        public ObservableCollection<PackagingUnit> PackagingUnits
        {
            get { return _packagingUnits; }
            set
            {
                _packagingUnits = value;
                OnPropertyChanged();
            }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
            }
        }

        // Méthode appelée par la commande SearchCommand
        private void ExecuteSearch()
        {
            FilterPu();
        }

        private void FilterPu()
        {
            PackagingUnits.Clear();
            foreach (PackagingUnit pu in _packagingUnitProvider.GetPackagingUnits(SearchKeyword))
            {
                PackagingUnits.Add(pu);
            }
        }

        // Méthode pour générer un fichier CSV
        // Méthode pour générer un fichier CSV et réinitialiser les quantités des P.U à 0
        private async Task GenerateCsvFile()
        {
            if (PackagingUnits.Count == 0)
                return;

            // Génération du fichier CSV
            var filePath = "PackagingUnitsClient.csv";
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var pu in PackagingUnits)
                {
                    writer.WriteLine($"{pu.UID},{pu.ComponentBarcode},{pu.Quantity}, {pu.Extra2}");
                }
            }

            // Appel de la méthode pour mettre les quantités à 0 pour les P.U où Extra2 correspond à SearchKeyword
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                int rowsAffected = await _packagingUnitProvider.ResetPackagingUnitQuantitiesAsync(SearchKeyword);
                // Vous pouvez notifier l'utilisateur du succès de la réinitialisation
                Log.Information($"{rowsAffected} unités de conditionnement ont été réinitialisées à 0 pour Extra2 = '{SearchKeyword}'.");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    
}
