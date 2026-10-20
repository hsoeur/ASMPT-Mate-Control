using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LO_ASM_MATE_CONTROL.Services.Managers
{
    public static class MessageManager
    {
        /// <summary>
        /// Méthode alimentant le panneau de logs de l'application pour les retours utlisateurs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logPannel"></param>
        public static void DisplayLogMessage(string message, string logPannel)
        {
            logPannel += Environment.NewLine + "[" + DateTime.Now.ToString("HH:mm:ss:ff") + "] => " + message;
        }
    }
}
