using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using LO_ASM_MATE_CONTROL.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LO_ASM_MATE_CONTROL.Services.Providers
{
    /// <summary>
    /// Data Access Layer pour accèder à la base de sonnées SQL Server SetupCenter.
    /// </summary>
    public class PackagingUnitProvider : IPackagingUnitProvider
    {
        private readonly string _connectionString = "Server=SRV-SQL;Database=SiplaceSetupCenter;Integrated Security=True;";

        public PackagingUnitProvider()
        {
        }
        /// <summary>
        /// Récupère toutes les bobines inscrites dans la base ASMPT Setup Center.
        /// </summary>
        /// <returns></returns>
        public List<PackagingUnit> GetAllPackagingUnits()
        {
            List<PackagingUnit> result = new List<PackagingUnit>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM dbo.PackagingUnit";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        Log.Information("La connection à la base ASM a réussie.");

                        command.CommandText = query;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            while(reader.Read())
                            {
                                string id, componentName, currentQuantity, magasin, client;

                                PackagingUnit pu;

                                CreatePackagingUnit(reader, out id, out componentName, out currentQuantity, out magasin, out client, out pu);

                                result.Add(pu);

                                Log.Information($"{id},{componentName},{currentQuantity},{magasin},{client}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        throw; // Ne jamais générer un inventaire avec une lecture SQL partielle.
                    }
                }

                
            }
            return result;
        }

        /// <summary>
        /// Méthode de filtrage pour afficher les bobines qui correspondent au mot clé donné en paramètre.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<PackagingUnit> GetPackagingUnits(string keyword)
        {
            List<PackagingUnit> result = new List<PackagingUnit>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM dbo.PackagingUnit WHERE PackagingUnitId LIKE @keyword OR SiplaceProComponent LIKE @keyword OR Extra1 LIKE @keyword OR Extra2 LIKE @keyword";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        Log.Information("La connection à la base ASM a réussie.");

                        command.CommandText = query;
                        command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                string id, componentName, currentQuantity, magasin, client;

                                PackagingUnit pu;

                                CreatePackagingUnit(reader, out id, out componentName, out currentQuantity, out magasin, out client, out pu);

                                result.Add(pu);

                                Log.Information($"{id},{componentName},{currentQuantity},{magasin},{client}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }


            }
            return result;
        }

        public async Task<int> ResetPackagingUnitQuantitiesAsync(string keyword)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE dbo.PackagingUnit SET Quantity = 0 WHERE Extra2 = @keyword";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        Log.Information("La connexion à la base ASM a réussi.");

                        command.Parameters.AddWithValue("@keyword", keyword);

                        rowsAffected = await command.ExecuteNonQueryAsync();

                        Log.Information($"{rowsAffected} lignes ont été mises à jour avec des quantités à 0 pour Extra2 = '{keyword}'.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Erreur lors de la mise à jour des quantités des unités de conditionnement.");
                        throw; // Relance l'exception après journalisation
                    }
                }
            }
            return rowsAffected; // Retourne le nombre de lignes affectées
        }



        private static void CreatePackagingUnit(SqlDataReader reader, out string id, out string componentName, out string currentQuantity, out string magasin, out string client, out PackagingUnit pu)
        {
            id = reader["PackagingUnitId"].ToString();
            componentName = reader["SiplaceProComponent"].ToString();
            string originalQuantity = reader["OriginalQuantity"].ToString();
            currentQuantity = reader["Quantity"].ToString();
            magasin = reader["Extra1"].ToString();
            client = reader["Extra2"].ToString();

            pu = new PackagingUnit
            {
                UID = id,
                ComponentBarcode = componentName,
                ComponentName = componentName,
                Quantity = int.Parse(currentQuantity),
                OriginalQuantity = int.Parse(originalQuantity),
                Extra1 = magasin,
                Extra2 = client
            };
        }

        /// <summary>
        /// Met à jour le champ Extra1 (codeMagasin) pour tous les enregistrements correspondant à un codeArticle.
        /// </summary>
        /// <param name="codeArticle">Le code de l'article à mettre à jour.</param>
        /// <param name="codeMagasin">Le nouveau code de magasin à appliquer.</param>
        /// <returns>Le nombre de lignes affectées.</returns>
        public async Task<int> UpdatePackagingUnitsMagasinAsync(string codeArticle, string codeMagasin)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE dbo.PackagingUnit SET Extra1 = @codeMagasin WHERE SiplaceProComponent = @codeArticle";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        Log.Information("La connexion à la base ASM a réussi.");

                        // Ajout des paramètres
                        command.Parameters.AddWithValue("@codeMagasin", codeMagasin);
                        command.Parameters.AddWithValue("@codeArticle", codeArticle);

                        // Exécution de la requête
                        rowsAffected = await command.ExecuteNonQueryAsync();

                        Log.Information($"{rowsAffected} enregistrements ont été mis à jour pour le code article '{codeArticle}' avec le code magasin '{codeMagasin}'.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Erreur lors de la mise à jour des enregistrements pour l'article et le magasin.");
                        throw; // Relance l'exception après journalisation
                    }
                }
            }

            return rowsAffected; // Retourne le nombre de lignes affectées
        }

    }
}
