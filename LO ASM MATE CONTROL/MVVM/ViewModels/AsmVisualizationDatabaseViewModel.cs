using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using LO_ASM_MATE_CONTROL.Core;
using LO_ASM_MATE_CONTROL.Services.Providers;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LO_ASM_MATE_CONTROL.MVVM.ViewModels
{
    public class AsmVisualizationDatabaseViewModel : ViewModel
    {

        private readonly IPackagingUnitProvider _packagingUnitProvider;
        public AsmVisualizationDatabaseViewModel(IPackagingUnitProvider packagingUnitProvider)
        {
            _packagingUnitProvider = packagingUnitProvider;
            PackagingUnits = new ObservableCollection<PackagingUnit>();
            LoadPackagingUnits();
        }

        private void FilterPu()
        {
            PackagingUnits.Clear();
            foreach (PackagingUnit pu in _packagingUnitProvider.GetPackagingUnits(SearchKeyword))
            {
                PackagingUnits.Add(pu);
            }
            
        }

        private void LoadPackagingUnits()
        {
            foreach(PackagingUnit pu in _packagingUnitProvider.GetAllPackagingUnits())
            {
                PackagingUnits.Add(pu);
            }
        }

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

        public string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                FilterPu();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
