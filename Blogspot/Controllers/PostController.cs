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

        public IActionResult Details(int id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var post = _context.Posts.Include(p => p.Category).Include(p =>p.Comments).FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
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


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
           var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            var editViewModel = new EditViewModel
            {
                Post = postFromDb,
                Categories = _context.Categories.Select(c =>
                    new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList()
            };
            return View(editViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditViewModel editViewModel)
        {
            if (id != editViewModel.Post.Id)
            {
                return NotFound();
            }

            var postFromDb = await _context.Posts.FindAsync(id);
            if (postFromDb == null)
            {
                return NotFound();
            }

            if (editViewModel.FeatureImage != null && editViewModel.FeatureImage.Length > 0)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.FeatureImage.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(inputFileExtension))
                {
                    ModelState.AddModelError("FeatureImage", "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                editViewModel.Categories = GetCategories();
                editViewModel.Post.FeatureImagePath = postFromDb.FeatureImagePath;
                return View(editViewModel);
            }

            postFromDb.Title = editViewModel.Post.Title;
            postFromDb.Content = editViewModel.Post.Content;
            postFromDb.Author = editViewModel.Post.Author;
            postFromDb.CategoryId = editViewModel.Post.CategoryId;
            postFromDb.PublishedDate = editViewModel.Post.PublishedDate;

            if (editViewModel.FeatureImage != null && editViewModel.FeatureImage.Length > 0)
            {
                postFromDb.FeatureImagePath = await UploadFiletoFolder(editViewModel.FeatureImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = postFromDb.Id });
        }

        public JsonResult AddComment([FromBody]Comments comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.Content) || comment.PostId <= 0)
            {
                return Json(new { success = false, message = "Invalid comment data." });
            }
            comment.CommentDate = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return Json(new { 
                username = comment.UserName,
                commentDate = comment.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                content = comment.Content,

                success = true, message = "Comment added successfully.", comment });
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

        private List<SelectListItem> GetCategories()
        {
            return _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }


    }
}
