using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineLibrary.Models;

namespace OnlineLibrary.Controllers
{
    public class BookController : Controller
    {
		// GET: Book
        public ActionResult Index() {

			ViewBag.BookGenreList = new SelectList(Book.BookGenreList);

			return View( );
        }

		
		public ActionResult Search( string BookGenre, string SearchString, int page = 1, int pageSize = 30, string orderby = "") {
			ViewBag.BookGenreList = new SelectList(Book.BookGenreList);
			ViewBag.OrderByList = new SelectList(new List<string>(Book.OrderByList));

            if ( string.IsNullOrEmpty(SearchString) || string.IsNullOrWhiteSpace(SearchString) )
				return RedirectToAction("Index");

			PagedList.PagedList<Book> model = new PagedList.PagedList<Book>(
				QueryHandler.Search(BookGenre, SearchString, orderby),
				page, pageSize);

			ViewBag.BookGenre = BookGenre;
			ViewBag.SearchString = SearchString;
			ViewBag.OrderBy = orderby;

			return View(model);

		}

		// GET: Book/Details/5
		public ActionResult Details(int? id = 0 )
        {
			if ( id == 0 )
				return View( );
			Book model = QueryHandler.GetDetail((int)id);

			ViewBag.Recommendation = new List<Book>( );

			if ( model.Tags.Count > 0 )
				ViewBag.Recommendation = QueryHandler.GetBooksByTag(model.Tags.GetRange(0, 1), "Popular").GetRange(0, 5);

			return View(model);



        }

		public ActionResult Borrow(string userName, int? bookId ) {

			ViewBag.Result = QueryHandler.BorrowBooks(userName, bookId.ToString(), System.DateTime.Now.ToString());

			return RedirectToAction("Details", new { id=bookId });

		}
		
        //// GET: Book/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Book/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
		
    }
}
