using ApplicationCore.Entities.Products;
using ApplicationWeb.Data;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CommonController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment webHost;
        private readonly IUploadService uploadService;

        public CommonController(ApplicationDbContext context, IWebHostEnvironment webHost, IUploadService uploadService)
        {
            this.context = context;
            this.webHost = webHost;
            this.uploadService = uploadService;
        }
      
        [HttpGet]
        public IActionResult GetAllDirectoryImages()
        {
            try
            {
                List<Upload> uploads = uploadService.GetAll();
                ViewBag.uploads = uploads;
                return PartialView("_DirectoryImage");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> GetCombos(IEnumerable<KeyValuePair<int, List<string>>> remainingTags)
        {
            if (remainingTags.Count() == 1)
            {
                return remainingTags.First().Value;
            }
            else
            {
                var current = remainingTags.First();
                List<string> outputs = new List<string>();
                List<string> combos = GetCombos(remainingTags.Where(tag => tag.Key != current.Key));

                foreach (var tagPart in current.Value)
                {
                    foreach (var combo in combos)
                    {
                        outputs.Add(tagPart + "-" + combo);
                    }
                }
                return outputs;
            }
        }

        [HttpDelete]
        public IActionResult DeleteImages(string ids)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}