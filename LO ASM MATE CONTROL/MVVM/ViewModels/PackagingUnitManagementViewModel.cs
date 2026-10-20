using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using Asm.As.Oib.SiplaceSetupCenter.Proxy.Business.Objects;
using LO_ASM_MATE_CONTROL.Core;
using LO_ASM_MATE_CONTROL.Services.Managers;
using LO_ASM_MATE_CONTROL.Services.Providers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LO_ASM_MATE_CONTROL.MVVM.ViewModels
{
    public class PackagingUnitManagementViewModel : ViewModel
    {
        private readonly IPackagingUnitProvider _packagingUnitProvider;

        public PackagingUnitManagementViewModel(IPackagingUnitProvider packagingUnitProvider)
        {
            _packagingUnitProvider = packagingUnitProvider;

            UpdatePackagingUnitCommnd = new RelayCommand(_=>true, _ => UpdatePu());
            CreatePackagingUnitCommnd = new RelayCommand(_=>true, _ => CreatePu());
            ReadPackagingUnitCommand = new RelayCommand( _=>true, _ => ReadPu());
            DeletePackagingUnitCommnd = new RelayCommand(_=>true, _ => DeletePu());
            UpdateArticleMagasinCommand = new RelayCommand(_=>true, async _ => await UpdateArticleMagasinAsync());
        }


        public ICommand UpdatePackagingUnitCommnd { get; set; }
        public ICommand CreatePackagingUnitCommnd { get; set; }
        public ICommand DeletePackagingUnitCommnd { get; set; }
        public ICommand ReadPackagingUnitCommand { get; set; }
        public ICommand UpdateArticleMagasinCommand { get; set; }

        private string _realScan;
        public string RealScan
        {
            get { return _realScan; }
            set
            {
                _realScan = value;
            }
        }
        private string _puUid;
        public string PuUid
        {
            get { return _puUid; }
            set
            {
                _puUid = value;
                OnPropertyChanged(nameof(PuUid));
            }
        }
        private string _puCode;
        public string PuCode
        {
            get { return _puCode; }
            set
            {
                _puCode = value;
                OnPropertyChanged(nameof(PuCode));
            }
        }

        private string _puQuantity;
        public string PuQuantity
        {
            get { return _puQuantity; }
            set
            {
                _puQuantity = value;
                OnPropertyChanged(nameof(PuQuantity));
            }
        }
        private string _puOriginalQuantity;
        public string PuOriginalQuantity
        {
            get { return _puOriginalQuantity; }
            set
            {
                _puOriginalQuantity = value;
                OnPropertyChanged(nameof(PuOriginalQuantity));
            }
        }

        private string _puMagasin;
        public string PuMagasin
        {
            get { return _puMagasin; }
            set
            {
                _puMagasin = value;
                OnPropertyChanged(nameof(PuMagasin));
            }
        }

        private string _puClient;
        public string PuClient
        {
            get { return _puClient; }
            set
            {
                _puClient = value;
                OnPropertyChanged(nameof(PuClient));
            }
        }

        private string _articleCodeToUpdate;
        public string ArticleCodeToUpdate
        {
            get { return _articleCodeToUpdate; }
            set
            {
                _articleCodeToUpdate = value;
                OnPropertyChanged(nameof(ArticleCodeToUpdate));
            }
        }

        private string _newMagasin;
        public string NewMagasin
        {
            get { return _newMagasin; }
            set
            {
                _newMagasin = value;
                OnPropertyChanged(nameof(NewMagasin));
            }
        }


        private string _logPannel;
        public string LogPannel
        {
            get
            {
                return _logPannel;
            }
            set
            {
                _logPannel = value;
                OnPropertyChanged(nameof(LogPannel));
            }

        }

        private async Task UpdateArticleMagasinAsync()
        {
            string articleCode = ArticleCodeToUpdate?.Trim();
            string magasin = NewMagasin?.Trim();

            if (string.IsNullOrWhiteSpace(articleCode))
            {
                UpdateLogs("Le code article est obligatoire.");
                return;
            }

            if (string.IsNullOrWhiteSpace(magasin))
            {
                UpdateLogs("Le nouvel emplacement magasin est obligatoire.");
                return;
            }

            try
            {
                int rowsAffected = await _packagingUnitProvider.UpdatePackagingUnitsMagasinAsync(articleCode, magasin);

                UpdateLogs($"{rowsAffected} packaging unit(s) mise(s) a jour pour l'article {articleCode} avec le magasin {magasin}.");
                Log.Information("{RowsAffected} packaging unit(s) mises a jour pour l'article {ArticleCode} avec le magasin {Magasin}.", rowsAffected, articleCode, magasin);
                MessageBox.Show($"{rowsAffected} bobine(s) mise(s) a jour pour l'article {articleCode}.", "Mise a jour terminee", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateLogs(ex.Message);
                Log.Error(ex, "Erreur lors de la mise a jour du magasin pour l'article {ArticleCode}.", articleCode);
                MessageBox.Show($"Erreur lors de la mise a jour du magasin.\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReadPu()
        {

            Log.Information("PackagingUnitManagementViewModel::ReadPu()");
            try
            {
                #region DOC_READ_PACKAGING_UNIT

                Log.Information($"Lecture de la bobine scan n°{RealScan}");
                SiplaceSetupCenter proxy = SetupCenterManager.GetSetupCenterProxy();
                 
                PackagingUnit pu = new PackagingUnit();
               
                pu.UID = RealScan;

                PackagingUnitResult[] result = proxy.Get(new[] { pu });

                if (result.Length > 0 && result[0].ResultState == 1)
                {
                    Log.Information("Lecture réussie pour la bobine suivante :");
                    Log.Information("UID : " + result[0].PackagingUnit.UID);
                    Log.Information("ComponentName : " + result[0].PackagingUnit.ComponentName);
                    Log.Information("Original Quantity : " + result[0].PackagingUnit.OriginalQuantity.ToString(CultureInfo.InvariantCulture));
                    Log.Information("Quantity : " + result[0].PackagingUnit.Quantity.ToString(CultureInfo.InvariantCulture));
                    Log.Information("Magasin : " + result[0].PackagingUnit.Extra1);
                    Log.Information("CLient : " + result[0].PackagingUnit.Extra2);

                    PuUid = result[0].PackagingUnit.UID;
                    PuCode = result[0].PackagingUnit.ComponentName;
                    PuOriginalQuantity = result[0].PackagingUnit.OriginalQuantity.ToString(CultureInfo.InvariantCulture);
                    PuQuantity = result[0].PackagingUnit.Quantity.ToString(CultureInfo.InvariantCulture);
                    PuMagasin = result[0].PackagingUnit.Extra1;
                    PuClient = result[0].PackagingUnit.Extra2;

                    UpdateLogs($"Packaging unit {pu.UID} successfully read");
                }
                else if (result.Length > 0 && result[0].ResultState == 2 && result[0].Messages.Count > 0)
                {
                    UpdateLogs(string.Format("Packaging unit import failed, message: '{0}'", result[0].Messages[0].Message)); 
                    Log.Error(string.Format("Packaging unit import failed, message: '{0}'", result[0].Messages[0].Message)); 
                }
                else
                {
                    UpdateLogs("Packaging unit import failed with a bad Setup Center result data");
                    Log.Error("Packaging unit import failed with a bad Setup Center result data");
                }
                //UpdateLogPannel();
                #endregion
            }
            catch (Exception ex)
            {
                UpdateLogs(ex.Message);
                Log.Error(ex.ToString());
            }
        }

        private void ClearFields()
        {
            PuUid = string.Empty;
            PuQuantity = string.Empty;
            PuOriginalQuantity = string.Empty;
            PuCode = string.Empty;
            PuClient = string.Empty;
            PuMagasin = string.Empty;
        }

        private void DeletePu()
        {
            try
            {
                #region DOC_DELETE_PACKAGING_UNIT
                
                SiplaceSetupCenter proxy = SetupCenterManager.GetSetupCenterProxy();
                PackagingUnit pu = new PackagingUnit();

                pu.UID = PuUid;

                PackagingUnitResult[] result = proxy.Delete(new[] { pu });

                if (result.Length > 0 && result[0].ResultState == 1)
                {
                    UpdateLogs($"Packaging unit {pu.UID} successfully deleted");
                    Log.Information($"Packaging unit {pu.UID} successfully deleted");

                }
                else if (result.Length > 0 && result[0].ResultState == 2 && result[0].Messages.Count > 0)
                {
                    UpdateLogs(string.Format("Packaging unit deletion failed, message: '{0}'", result[0].Messages[0].Message));
                    Log.Error(string.Format("Packaging unit deletion failed, message: '{0}'", result[0].Messages[0].Message));
                }
                else
                {
                    UpdateLogs("Packaging unit update deletion with a bad Setup Center result data");
                    Log.Error("Packaging unit update deletion with a bad Setup Center result data");
                }

                ClearFields();

                #endregion
            }
            catch (Exception ex)
            {
                UpdateLogs(ex.Message);
                Log.Error(ex.ToString());
            }
        }

        private void CreatePu()
        {
            Log.Information("PackagingUnitManagementViewModel::CreatePu()");
            try
            {
                SiplaceSetupCenter proxy = SetupCenterManager.GetSetupCenterProxy();

                PackagingUnit pu = new PackagingUnit();

                pu.UID = PuUid;
                pu.ComponentName = PuCode;
                pu.ComponentBarcode = PuCode;
                pu.Extra1 = PuMagasin;
                pu.Extra2 = PuClient;

                Log.Information("UID : " + pu.UID);
                Log.Information("ComponentName : " + pu.ComponentName);
                Log.Information("Magasin : " + pu.Extra1);
                Log.Information("CLient : " + pu.Extra2);

                int startQuantitiy;
                if (!int.TryParse(PuOriginalQuantity, out startQuantitiy))
                {
                    UpdateLogs("Start Quantity need to be a number");
                    Log.Error("Start Quantity need to be a number");
                    return;
                }
                pu.OriginalQuantity = startQuantitiy;
                Log.Information("Original Quantity : " + pu.OriginalQuantity);

                int quantity;
                if (!int.TryParse(PuQuantity, out quantity))
                {
                    UpdateLogs("Quantity need to be a number");
                    Log.Error("Quantity need to be a number");
                    return;
                }
                pu.Quantity = quantity;
                Log.Information("Quantity : " + pu.Quantity);

                PackagingUnitResult[] result = SetupCenterManager.GetSetupCenterProxy().Create(new[] { pu });

                if (result.Length > 0 && result[0].ResultState == 1)
                {
                    UpdateLogs($"Packaging unit {pu.UID} successfully created");

                    Log.Information("Création réussie pour la bobine suivante :");
                    Log.Information("UID : " + pu.UID);
                    Log.Information("ComponentName : " + pu.ComponentName);
                    Log.Information("Original Quantity : " + pu.OriginalQuantity);
                    Log.Information("Quantity : " + pu.Quantity);
                    Log.Information("Magasin : " + pu.Extra1);
                    Log.Information("CLient : " + pu.Extra2);
                }
                else if (result.Length > 0 && result[0].ResultState == 2 && result[0].Messages.Count > 0)
                {
                    UpdateLogs(string.Format("Packaging unit create failed, message: '{0}'", result[0].Messages[0].Message));
                    Log.Error(string.Format("Packaging unit create failed, message: '{0}'", result[0].Messages[0].Message));
                }
                else
                {
                    UpdateLogs("Packaging unit create failed with a bad Setup Center result data");
                    Log.Error("Packaging unit create failed with a bad Setup Center result data");
                }
                ClearFields();

            }
            catch (Exception ex)
            {
                UpdateLogs(ex.Message);
                Log.Error(ex.ToString());
            }
        }

        private void UpdatePu()
        {
            Log.Information("PackagingUnitManagementViewModel::UpdatePu()");
            try
            {
                SiplaceSetupCenter proxy = SetupCenterManager.GetSetupCenterProxy();
                PackagingUnit pu = new PackagingUnit();

                pu.UID = PuUid;
                pu.ComponentName = PuCode;
                pu.ComponentBarcode = PuCode;
                pu.Extra1 = PuMagasin;
                pu.Extra2 = PuClient;


                Log.Information("UID : " + pu.UID);
                Log.Information("ComponentName : " + pu.ComponentName);
                Log.Information("Magasin : " + pu.Extra1);
                Log.Information("CLient : " + pu.Extra2);

                int startQuantitiy;
                if (!int.TryParse(PuOriginalQuantity, out startQuantitiy))
                {
                    UpdateLogs("Start Quantity need to be a number");
                    Log.Error("Start Quantity need to be a number");
                    return;
                }
                pu.OriginalQuantity = startQuantitiy;
                Log.Information("Original Quantity : " + pu.OriginalQuantity);

                int quantity;
                if (!int.TryParse(PuQuantity, out quantity))
                {
                    UpdateLogs("Quantity need to be a number");
                    Log.Error("Quantity need to be a number");
                    return;
                }
                pu.Quantity = quantity;
                Log.Information("Quantity : " + pu.Quantity);

                PackagingUnitResult[] result = SetupCenterManager.GetSetupCenterProxy().Update(new[] { pu });

                if (result.Length > 0 && result[0].ResultState == 1)
                {
                    UpdateLogs($"Packaging unit {pu.UID} successfully updated");

                    Log.Information("Mise à jour réussie pour la bobine suivante :");
                    Log.Information("UID : " + pu.UID);
                    Log.Information("ComponentName : " + pu.ComponentName);
                    Log.Information("Original Quantity : " + pu.OriginalQuantity);
                    Log.Information("Quantity : " + pu.Quantity);
                    Log.Information("Magasin : " + pu.Extra1);
                    Log.Information("CLient : " + pu.Extra2);
                }
                else if (result.Length > 0 && result[0].ResultState == 2 && result[0].Messages.Count > 0)
                {
                    
                    UpdateLogs(string.Format("Packaging unit update failed, message: '{0}'", result[0].Messages[0].Message));
                    Log.Error(string.Format("Packaging unit update failed, message: '{0}'", result[0].Messages[0].Message));
                }
                else
                {
                    UpdateLogs("Packaging unit update failed with a bad Setup Center result data");
                    Log.Error("Packaging unit update failed with a bad Setup Center result data");
                }
                ClearFields();

            }
            catch (Exception ex)
            {
                UpdateLogs(ex.Message);
                Log.Error(ex.ToString());
            }
        }

        private void UpdateLogs(string log)
        {
            LogPannel += "[" + DateTime.Now.ToString("HH:mm:ss:ff") + "] => " + log + "\n";
        }
    }

}
