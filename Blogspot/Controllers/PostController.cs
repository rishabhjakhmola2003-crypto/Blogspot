using Blogspot.Data;
using Blogspot.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blogspot.Models;
namespace Blogspot.Controllers
{
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        public PostController(AppDbContext context , IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult Index(int? categoryId)
        {
            var postsQuery = _context.Posts.Include(p => p.Category).AsQueryable();

           if(categoryId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.CategoryId == categoryId.Value);
            }
            var posts = postsQuery.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View(posts);
        }




        [HttpGet]
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel();
            postViewModel.Categories = _context.Categories.Select(c=>
                new SelectListItem 
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }
                ).ToList();


            return View(postViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (ModelState.IsValid) { 
                var inputFileExtension = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtensions.Contains(inputFileExtension);

                if (!isAllowed) { 
                    ModelState.AddModelError("FeatureImage", "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");
                    return View(postViewModel);
                }
              postViewModel.Post.FeatureImagePath=  await UploadFiletoFolder(postViewModel.FeatureImage);
                 _context.Posts.AddAsync(postViewModel.Post);
                 _context.SaveChangesAsync();
               return RedirectToAction("Index");
            }
            postViewModel.Categories = _context.Categories.Select(c =>
      new SelectListItem
      {
          Value = c.Id.ToString(),
          Text = c.Name
      }
  ).ToList();

            return View(postViewModel);
           // return View(postViewModel);

        }
        private async Task<string> UploadFiletoFolder(IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName);
            var fileName =Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");
            if(!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);

            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw; // Rethrow the exception to be handled by the calling method
            }
            return "/images/" + fileName;
        }


    }
}
