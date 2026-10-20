using LO_ASM_MATE_CONTROL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.Navigation
{
    /// <summary>
    /// Service de navigation entre les différent viewModels
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// ViewModel en cours d'affichage
        /// </summary>
        ViewModel CurrentViewModel { get; }

        /// <summary>
        /// Naviguer vers le T ViewModel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void NavigateTo<T>() where T : ViewModel;
    }
}
