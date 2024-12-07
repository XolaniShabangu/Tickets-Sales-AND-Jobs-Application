using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookBuffetSem2.Models;
using System.Data.Entity;

namespace TheBookBuffetSem2.Controllers
{
    public class ManagementController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // *** AUTHOR MANAGEMENT ***

        // GET: Management/CreateAuthor
        public ActionResult CreateAuthor()
        {
            return View();
        }

        // POST: Management/CreateAuthor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAuthor([Bind(Include = "Name,Email")] Author author)
        {
            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                db.SaveChanges();
                return RedirectToAction("IndexAuthors"); // Redirect to a view listing all authors
            }

            return View(author);
        }

        // GET: Management/IndexAuthors
        public ActionResult IndexAuthors()
        {
            var authors = db.Authors.ToList();
            return View(authors);
        }

        // *** BOOK MANAGEMENT ***

        // GET: Management/CreateBook
        // GET: Book/Create
        public ActionResult CreateBook()
        {
            // Fetch all authors from the database
            var authors = db.Authors.ToList();

            // Convert authors to SelectListItems and store in ViewBag
            ViewBag.AuthorID = new SelectList(authors, "AuthorID", "Name");

            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBook([Bind(Include = "Title,Price,AuthorID")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("IndexBooks");
            }

            // If the model is not valid, repopulate the ViewBag with authors
            var authors = db.Authors.ToList();
            ViewBag.AuthorID = new SelectList(authors, "AuthorID", "Name", book.AuthorID);

            return View(book);
        }


        // GET: Management/IndexBooks
        public ActionResult IndexBooks()
        {
            var books = db.Books.Include(b => b.Author).ToList();
            return View(books);
        }


        // *** MEDIA MANAGEMENT ***

        // GET: Management/CreateMedia
        public ActionResult CreateMedia()
        {
            return View();
        }

        // POST: Management/CreateMedia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMedia([Bind(Include = "Name,Email")] Media media)
        {
            if (ModelState.IsValid)
            {
                db.Media.Add(media);
                db.SaveChanges();
                return RedirectToAction("IndexMedia"); // Redirect to a view listing all media contacts
            }

            return View(media);
        }

        // GET: Management/IndexMedia
        public ActionResult IndexMedia()
        {
            var mediaContacts = db.Media.ToList();
            return View(mediaContacts);
        }

        // Dispose of the db context
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        // *** AUTHOR MANAGEMENT ***

        // GET: Management/EditAuthor
        public ActionResult EditAuthor(int id)
        {
            var author = db.Authors.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // POST: Management/EditAuthor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAuthor([Bind(Include = "AuthorID,Name,Email")] Author author)
        {
            if (ModelState.IsValid)
            {
                db.Entry(author).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexAuthors");
            }
            return View(author);
        }

        // GET: Management/DeleteAuthor
        public ActionResult DeleteAuthor(int id)
        {
            var author = db.Authors.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // POST: Management/DeleteAuthor
        [HttpPost, ActionName("DeleteAuthor")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAuthorConfirmed(int id)
        {
            var author = db.Authors.Find(id);
            db.Authors.Remove(author);
            db.SaveChanges();
            return RedirectToAction("IndexAuthors");
        }

        // *** BOOK MANAGEMENT ***

        // GET: Management/EditBook
        public ActionResult EditBook(int id)
        {
            var book = db.Books.Include(b => b.Author).FirstOrDefault(b => b.BookID == id);
            if (book == null)
            {
                return HttpNotFound();
            }

            ViewBag.Authors = new SelectList(db.Authors, "AuthorID", "Name", book.AuthorID);
            return View(book);
        }

        // POST: Management/EditBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBook([Bind(Include = "BookID,Title,AuthorID")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexBooks");
            }

            ViewBag.Authors = new SelectList(db.Authors, "AuthorID", "Name", book.AuthorID);
            return View(book);
        }

        // GET: Management/DeleteBook
        public ActionResult DeleteBook(int id)
        {
            var book = db.Books.Include(b => b.Author).FirstOrDefault(b => b.BookID == id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Management/DeleteBook
        [HttpPost, ActionName("DeleteBook")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBookConfirmed(int id)
        {
            var book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("IndexBooks");
        }

        // *** MEDIA MANAGEMENT ***

        // GET: Management/EditMedia
        public ActionResult EditMedia(int id)
        {
            var media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            return View(media);
        }

        // POST: Management/EditMedia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMedia([Bind(Include = "MediaID,Name,Email")] Media media)
        {
            if (ModelState.IsValid)
            {
                db.Entry(media).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexMedia");
            }
            return View(media);
        }

        // GET: Management/DeleteMedia
        public ActionResult DeleteMedia(int id)
        {
            var media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            return View(media);
        }

        // POST: Management/DeleteMedia
        [HttpPost, ActionName("DeleteMedia")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMediaConfirmed(int id)
        {
            var media = db.Media.Find(id);
            db.Media.Remove(media);
            db.SaveChanges();
            return RedirectToAction("IndexMedia");
        }

    }

}