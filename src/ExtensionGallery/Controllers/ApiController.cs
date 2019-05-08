using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExtensionGallery.Code;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExtensionGallery.Controllers
{
	public class ApiController : Controller
	{
		IHostingEnvironment _env;
		IHttpContextAccessor _httpContextAccessor;
		PackageHelper _helper;

		public ApiController(IHostingEnvironment env, IHttpContextAccessor httpContextAccessor)
		{
			_env = env;
			_httpContextAccessor = httpContextAccessor;
			_helper = new PackageHelper(env.WebRootPath);
		}

		public object Get(string id)
		{
			Response.Headers["Cache-Control"] = "no-cache";

			if (string.IsNullOrWhiteSpace(id))
			{
				var packages = _helper.PackageCache.OrderByDescending(p => p.DatePublished);

				if (this.IsConditionalGet(packages))
				{
					return Enumerable.Empty<Package>();
				}

				return packages;
			}

			var package = _helper.GetPackage(id);

			if (this.IsConditionalGet(package))
			{
				return new EmptyResult();
			}

			return package;
		}

		[HttpPost]
		public async Task<IActionResult> Upload()
		{
			var context = _httpContextAccessor.HttpContext;

			Stream bodyStream = context.Request.Body;
			string repo = context.Request.Query["repo"];
			string issueTracker = context.Request.Query["issuetracker"];

			try
			{
				Package package = await _helper.ProcessVsix(bodyStream, repo, issueTracker);

				return Json(package);
			}
			catch (Exception ex)
			{
				Response.StatusCode = 500;
				Response.Headers["x-error"] = ex.Message;
				return Content(ex.Message);
			}
		}
	}
}