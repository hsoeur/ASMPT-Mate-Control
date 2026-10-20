using LO_ASM_MATE_CONTROL.MVVM.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.ApiData
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["Api:Uri"]);
        }
        public async Task<List<Article>> GetAllArticles()
        {
            Log.Information("GetAllArticles ApiService Called");
            var uri = _configuration["Api:ArticlesEndpoint"];

            if (string.IsNullOrWhiteSpace(uri))
            {
                uri = "api/Article/GetAllArticles";
            }

            var fullUri = new Uri(_httpClient.BaseAddress, uri);
            Log.Information("Appel API articles : {FullUri}", fullUri);

            var response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("Erreur API articles. Url: {FullUri}, StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Response: {Response}",
                    fullUri,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    errorContent);

                throw new HttpRequestException($"Erreur API articles : {(int)response.StatusCode} {response.ReasonPhrase}. URL appelee : {fullUri}");
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Article>>(content);

        }

        public async Task<Article> GetArticleByArticleUid(string articleUid)
        {
            if (string.IsNullOrWhiteSpace(articleUid))
            {
                throw new ArgumentException("Le code article est obligatoire.", nameof(articleUid));
            }

            Log.Information("GetArticleByArticleUid ApiService Called for {ArticleUid}", articleUid);
            var uri = _configuration["Api:ArticleByUidEndpoint"];

            if (string.IsNullOrWhiteSpace(uri))
            {
                uri = "api/Articles/by-article-uid";
            }

            var separator = uri.Contains("?") ? "&" : "?";
            var requestUri = $"{uri}{separator}articleUid={Uri.EscapeDataString(articleUid)}";
            var fullUri = new Uri(_httpClient.BaseAddress, requestUri);
            Log.Information("Appel API article par UID : {FullUri}", fullUri);

            var response = await _httpClient.GetAsync(requestUri);

            // Une référence absente de la GPAO est un cas normal pour l'inventaire.
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                response.Dispose();
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log.Error("Erreur API article par UID. Url: {FullUri}, ArticleUid: {ArticleUid}, StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Response: {Response}",
                    fullUri,
                    articleUid,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    errorContent);

                throw new HttpRequestException($"Erreur API article {articleUid} : {(int)response.StatusCode} {response.ReasonPhrase}. URL appelee : {fullUri}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Article>(content);
        }

    }
}
