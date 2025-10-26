using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogWebsite.Data;
using BlogWebsite.Models;
using Microsoft.AspNetCore.Authorization;

namespace BlogWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/category/[action]/{id?}")] //
    //[Authorize(Roles = RoleName.Administrator)] //kiem
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            //var appDbContext = _context.Categories.Include(c => c.ParentCategory);
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);//
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList(); //chi lay cac category co cha = null va cac con cua no o trong childent

            return View(categories);
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        private void CreateSelectItems(List<Category> source, List<Category> des, int level)
        {
            String prefix = string.Concat(Enumerable.Repeat("---", level)); //tao chuoi * lap lai x lan
            foreach (var category in source)
            {

                //category.Title = prefix +" "+ category.Title;
                des.Add(new Category()
                {
                    Id = category.Id,
                    Title = prefix + " " + category.Title
                });
                if(category.CategoryChildren?.Count()>0)
                {
                    CreateSelectItems(category.CategoryChildren.ToList(), des, level + 1);

                }
            }

        }
        // GET: Admin/Categories/Create
        public async Task< IActionResult> CreateAsync()
        {
            var qr = (from c in _context.Categories select c)
                   .Include(c => c.ParentCategory)
                   .Include(c => c.CategoryChildren);//
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList(); //chi lay cac category co cha = null va cac con cua no o trong childent
            categories.Insert(0,new Category()
            {
                Id=-1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories,items , 0);

            var selectList = new SelectList(items,"Id","Title");



            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]//
        public async Task<IActionResult> Create([Bind("Id,ParentCategoryId,Title,Description,Slug")] Category category)
        {
            if (ModelState.IsValid)
            {
                if(category.ParentCategoryId == -1)
                {
                    category.ParentCategoryId = null;
                }
                //
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.Categories select c)
                  .Include(c => c.ParentCategory)
                  .Include(c => c.CategoryChildren);//
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList(); //chi lay cac category co cha = null va cac con cua no o trong childent
            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);

            var selectList = new SelectList(items, "Id", "Title");


            ViewData["ParentCategoryId"] = selectList;
            return View();

            
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.Categories select c)
                  .Include(c => c.ParentCategory)
                  .Include(c => c.CategoryChildren);//
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList(); //chi lay cac category co cha = null va cac con cua no o trong childent
            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);

            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Description,Slug")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            if (category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError(string.Empty, "Phai chon danh muc cha khac !");
            }
            if (ModelState.IsValid && category.ParentCategoryId != category.Id)
            {


               
                try
                {
                    if (category.ParentCategoryId == -1) {
                        category.ParentCategoryId = null;   
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.Categories select c)
                  .Include(c => c.ParentCategory)
                  .Include(c => c.CategoryChildren);//
            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList(); //chi lay cac category co cha = null va cac con cua no o trong childent
            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);

            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.CategoryChildren)
                .FirstOrDefaultAsync(c => c.Id == id); // lấy các category con va chính nó
            if (category == null)
            {
                return NotFound();
            }
            //neu co
            foreach (var cCategory in category.CategoryChildren)
            {
                cCategory.ParentCategoryId = category.ParentCategoryId;// ví dụ có 3:nếu xóa 2 thì 3 sẽ là con của 1
            }
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
