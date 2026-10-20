using LO_ASM_MATE_CONTROL.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.ApiData
{
    public interface IApiService
    {
        Task<List<Article>> GetAllArticles();
        Task<Article> GetArticleByArticleUid(string articleUid);
    }
}
