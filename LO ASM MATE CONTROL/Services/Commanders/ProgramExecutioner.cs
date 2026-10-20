using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.Commanders
{
    /// <summary>
    /// Classe responsable de lancer des exécutables extèrieurs à LO ASM MATE CONTROL.
    /// </summary>
    public class ProgramExecutioner : IProgramExecutioner
    {
        public void ExecuteProgram(string program, string dir)
        {
            try
            {
                // Configuration du processus
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = "",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = dir
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    Log.Information(output);
                    Log.Error(error);

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
