using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TablePagination_Asp_net_core_3.Data;
using TablePagination_Asp_net_core_3.Models;

namespace TablePagination_Asp_net_core_3.Controllers
{
    public class BookListController : Controller
    {
        private readonly ApplicationDbContext _db;
        public IEnumerable<Book> Books { get; set; }

        public BookListController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index2()
        {
        
            Books = await _db.Book.ToListAsync();

            return View(Books);
        }


        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {

            int ExcludeRecords = (pageSize * pageNumber) - pageSize;

            var Books = _db.Book
                 .Skip(ExcludeRecords)
                 .Take(pageSize);

            var result = new PagedResult<Book>
            {
                /*
                 No tracking queries are useful when the results are used in a read-only scenario. They're quicker to execute because there's no need to set up the change tracking information. If you don't need to update the entities retrieved from the database, then a no-tracking query should be used. You can swap an individual query to be no-tracking.
                 */
                Data = await Books.AsNoTracking().ToListAsync(),
                TotalItems = _db.Book.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return View(result);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book bookObj)
        {
            if (ModelState.IsValid)
            {

                await _db.Book.AddAsync(bookObj);
                await _db.SaveChangesAsync();

                Books = await _db.Book.ToListAsync();

                return RedirectToAction("Index", "BookList", Books);

            }
            else
            {
                return View();
            }

        }

        public Book SingleBook { get; set; }
        public async Task<IActionResult> Edit(int id)
        {
            SingleBook = await _db.Book.FindAsync(id);

            return View(SingleBook);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Book bookObj)
        {

            if (ModelState.IsValid)
            {
                var BookFromDB = await _db.Book.FindAsync(bookObj.Id);
                BookFromDB.Name = bookObj.Name;
                BookFromDB.Author = bookObj.Author;
                BookFromDB.ISBN = bookObj.ISBN;
                await _db.SaveChangesAsync();

                Books = await _db.Book.ToListAsync();

                return RedirectToAction("Index", "BookList", Books);

            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var book = await _db.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _db.Book.Remove(book);
            await _db.SaveChangesAsync();

            Books = await _db.Book.ToListAsync();

            return RedirectToAction("Index", "BookList", Books);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.Book.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteByApiId(int id)
        {
            var bookFromDb = await _db.Book.FirstOrDefaultAsync(u => u.Id == id);
            if (bookFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _db.Book.Remove(bookFromDb);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete Succesfull" });

        }
    }
}