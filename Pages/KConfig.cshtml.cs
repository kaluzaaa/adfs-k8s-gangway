using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace adfs_net_core.Pages
{
    [Authorize]
    public class KConfigModel : PageModel
    {
        private readonly ILogger<KConfigModel> _logger;
        private readonly IConfiguration Configuration;


        public KConfigModel(ILogger<KConfigModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            ViewData["id_token"] = await HttpContext.GetTokenAsync("id_token");
            ViewData["refresh_token"] = await HttpContext.GetTokenAsync("refresh_token");
            ViewData["client_secret"] = Configuration["ADFS:ClientSecret"];
            ViewData["client_id"] = Configuration["ADFS:ClientId"];
            ViewData["issuer_url"] = Configuration["ADFS:Authority"];
            ViewData["cluster_name"] = Configuration["K8s:ClusterName"];
            ViewData["apiserver_url"] = Configuration["K8s:ApiServerUrl"];
            @ViewData["user"] = User.Claims.Where(c => c.Type == ClaimTypes.Upn).FirstOrDefault().Value;
        }
    }
}
