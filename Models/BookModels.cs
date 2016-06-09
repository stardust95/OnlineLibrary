using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;

namespace OnlineLibrary.Models {

	public class Tag {
		public string Name;

	}
	
	public class Book {

		public static readonly string ConnectionString =
			@"User Id=cdb_outerroot;Host=5758ead546603.sh.cdb.myqcloud.com;Port=3603;Database=douban;Persist Security Info=True;password=ab123456";
			//@"Data Source=localhost; User Id=root; Database=douban";
			
		public static readonly List<string> BookAttributes = new List<string> {
			"ID", "Title", "SubTitile", "OriginTitle", "Alt_Title",
			"isbn10", "isbn13", "pub_date", "summary", "catalog", "price", "pages",
			"ebook_price", "url", "ebook_url", "image"
		};

		public static readonly List<string> BookGenreList = new List<string> {
			"Title", "Tag", "Author", "isbn10", "isbn13",
			"Summary", "Price"
		};

		public static List<string> OrderByList {
			get {
				return new List<string>(OrderByDictionary.Keys);
			}
		}

		public static readonly Dictionary<string, string> OrderByDictionary = new Dictionary<string, string> {
			{"Title","Title"},
			{"Rating", "Average DESC"},
			{"Publish Date", "pubdate DESC"},
			{"Popular", "numRaters DESC" }
			 
		};

		public static readonly List<string> briefviewList = new List<string> {

		};

		public static readonly string briefviewSelectString =
			"briefView.id, Title, Publisher, pubDate, image, pages, price, author, translator, summary, average, numRaters ";

		//[Key]
		//[Column("id")]
		public int ID { get; set; }

		//[Column("title")]
		public string Title { get; set; }

		//[Column("subtitle")]
		public string SubTitle { get; set; }

		//[Column("origin_title")]
		//[Display(Name = "Original Title")]
		public string OriginTitle { get; set; }

		//[Display(Name ="Alternative Title")]
		//[Column(name: "AltTitle", TypeName = "varchar(30)")]
		public string AltTitle { get; set; }

		//[Column("isbn10")]
		public string ISBN10 { get; set; }

		//[Column("isbn13")]
		public string ISBN13 { get; set; }

		//[Column("pubdate")]
		[Display(Name = "Publish Date")]
		public string PubDate { get; set; }

		//[Column("summary")]
		public string Summary { get; set; }

		////[Column("catalog")]
		public string Catalog { get; set; }

		//[Column("publisher")]
		public string Publisher { get; set; }

		//[Column("price")]
		public Decimal Price { get; set; }

		//[Column("pages")]
		public int Pages { get; set; }

		//[Display(Name ="EBook Price")]
		//[Column(name:"ebook_price")]
		public Decimal Ebook_Price { get; set; }

		//[Column("url")]
		public string URL { get; set; }
		
		public string EbookURL { get; set; }
		
		public string Image { get; set; }

		//[Column("title")]
		public List<string> Tags { get; set; }

		public class Ratings {
			public Decimal Average;
			public Decimal NumRaters;
		}
		public Ratings Rating { get; set; }

		public string Author { get; set; }

		public string AuthorInfo { get; set; }

		public string Translator { get; set; }
		

	}


	public static class QueryHandler {

		public static readonly int RecordsPerPage = 30;

		public static readonly string _RecordsPerPage = RecordsPerPage.ToString( );

		public static readonly int QueryLimit = 10000;

		private static void _sqlInjectionFilter(ref string s ) {
			s = s.Replace("=", "");
			s = s.Replace("'", "");
			s = s.Replace(";", "");
			s = s.Replace(" or ", " ");
			//s = s.Replace("select", "");
			//s = s.Replace("update", "");
			//s = s.Replace("insert", "");
			//s = s.Replace("delete", "");
			//s = s.Replace("declare", "");
			//s = s.Replace("exec", "");
			//s = s.Replace("drop", "");
			//s = s.Replace("create", "");
			s = s.Replace("%", "");
			s = s.Replace("--", "");
		}

		private static string _getMediumImage(int ID ) {
			using (var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {
				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = "Select * from images where id = " + ID;

				using(var r = cmd.ExecuteReader( ) ) {
					if ( r.Read( ) )
						return r["medium"].ToString( );		
				}

			}
			return null;
		}

		public static List<Book> GetBooksByAuthor(List<string> Authors, string orderby ) {
			List<Book> result = new List<Book>( );

			_sqlInjectionFilter(ref orderby);

			using ( var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {

				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = "Select " + Book.briefviewSelectString
					+ " from briefview left join authors on briefview.id = authors.id where author like "
																							+ string.Format("'%{0}%'", Authors.ElementAt(0));


				for ( int i = 1; i < Authors.Count; i++ ) {
					cmd.CommandText += " or author like " + string.Format("'%{0}%'", Authors.ElementAt(i));
				}

				cmd.CommandText += " group by authors.id";

				if ( !string.IsNullOrEmpty(orderby) )
					cmd.CommandText += " order by " + Book.OrderByDictionary[orderby];

                cmd.CommandText += " limit " + QueryHandler.QueryLimit;

				using ( var b = cmd.ExecuteReader( ) ) { 
					while ( b.Read( ) ) {
						result.Add(ReadBrief(b));
					}
				}

			}

			return result;
		}

		public static List<Book> GetBooksByTag(List<string> Tags, string orderby ) {
			List<Book> result = new List<Book>();

			_sqlInjectionFilter(ref orderby);

			using(var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {
				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = "Select "+ Book.briefviewSelectString 
					+" from briefview left join Tags on briefview.id = tags.id where tag like " + string.Format("'%{0}%'", Tags.ElementAt(0)) ;

				for(int i=1; i<Tags.Count; i++ ) {
					cmd.CommandText += " or tag like " + string.Format("'%{0}%'", Tags.ElementAt(i));
				}

				cmd.CommandText += " group by tags.id";

				if ( !string.IsNullOrEmpty(orderby) )
					cmd.CommandText += " order by " + Book.OrderByDictionary[orderby];

				cmd.CommandText += " limit " + QueryHandler.QueryLimit;

				using ( var b = cmd.ExecuteReader( ) ) {
					while( b.Read() ) {
						result.Add(ReadBrief(b));
					}
				}

			}

			return result;
		}

		public static Book GetDetail(int ID ) {
			Book bk = new Book( );

			using(var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {
				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = "Select * from books where id = " + ID;
				 
				using(var b = cmd.ExecuteReader( ) ) {
					if ( b.Read( ) ) {
						bk.ID = ID;
						bk.Title = b["Title"].ToString( );
						bk.SubTitle = b["sub_title"].ToString( );
						bk.AltTitle = b["alt_title"].ToString( );
						bk.OriginTitle = b["origin_title"].ToString( );
						bk.PubDate = b["pubdate"].ToString( );
						bk.Publisher = b["publisher"].ToString( );
						bk.Summary = "\n"+b["summary"].ToString( ).Trim();
						bk.Catalog = "\n" + b["catalog"].ToString( );
						var tmp = _getMediumImage(ID);
						if( !string.IsNullOrEmpty(tmp) )
							bk.Image = tmp;
						else
							bk.Image = b["image"].ToString( );
						bk.Pages = int.Parse(b["pages"].ToString( ));
						bk.Price = Decimal.Parse(b["price"].ToString( ));
						bk.ISBN10 = b["isbn10"].ToString( );
						bk.ISBN13 = b["isbn13"].ToString( );
						bk.URL = b["url"].ToString( );
						bk.EbookURL = b["ebook_url"].ToString( );
					}
				}

				cmd.CommandText = "Select * from authors where id = " + ID;
				using ( var b = cmd.ExecuteReader( ) ) {
					while ( b.Read( ) ) {
						if ( bk.Author == null ) {
							bk.Author = b["author"].ToString( ).Replace(';',',');
                        } else
							bk.Author += "; " + b["author"].ToString( );
					}
				}

				cmd.CommandText = "Select * from author_intro where author like " + "'" +
													bk.Author.Split(';').ElementAt(0).Replace("\"","%") + "'";
				using ( var b = cmd.ExecuteReader( ) ) {
					if ( b.Read( ) )
						bk.AuthorInfo = "\n" + b["author_intro"].ToString( );
				}
				cmd.CommandText = "Select * from translators where id = " + ID;
				using ( var b = cmd.ExecuteReader( ) ) {
					while ( b.Read( ) ) {
						if ( bk.Author == null )
							bk.Author = b["translator"].ToString( );
						else
							bk.Author += "; " + b["translator"].ToString( );
					}
				}

				bk.Tags = new List<string>( );
				cmd.CommandText = "Select * from tags where id = " + ID;
				using(var b = cmd.ExecuteReader( ) ) {
					while( b.Read( ) ) {
						bk.Tags.Add(b["tag"].ToString( ));
					}
				}
				bk.Rating = new Book.Ratings( );
				cmd.CommandText = "Select * from ratings where id = " + ID;
				using(var b = cmd.ExecuteReader( ) ) {
					if( b.Read( ) ) {
						bk.Rating.Average = Decimal.Parse(b["average"].ToString());
						bk.Rating.NumRaters = int.Parse(b["numRaters"].ToString( ));
					}
				}

			}
			return bk;
		}

		public static Book ReadBrief(MySqlDataReader b ) {
			Book bk = new Book {
				ID = int.Parse(b["ID"].ToString( )),
				Title = b["Title"].ToString( ),
				Publisher = b["Publisher"].ToString( ),
				PubDate = b["pubdate"].ToString( ),
				Image = b["image"].ToString( ),
				Pages = int.Parse(b["pages"].ToString( )),
				Price = Decimal.Parse(b["price"].ToString( )),
				Author = b["author"].ToString(),
				Translator = b["translator"].ToString(),
				Summary = b["summary"].ToString(),
				Rating = new Book.Ratings( ) {Average = Decimal.Parse(b["average"].ToString( )),
				NumRaters = int.Parse(b["numRaters"].ToString( ))
				}
			};
            return bk;
		}

		public static List<Book> Search(string genre , string query, string orderby) {

			char[ ] delims = { ' ', ',', '"', '\'', '\\', '/','.' };

			_sqlInjectionFilter(ref genre);
			_sqlInjectionFilter(ref query);
			_sqlInjectionFilter(ref orderby);

			List<Book> result = new List<Book>();

			List<string> Keywords = query.Split(delims, StringSplitOptions.RemoveEmptyEntries).ToList();

			using ( var cn = new MySqlConnection(connectionString:Book.ConnectionString) ) {

				var cmd = cn.CreateCommand( );
				
				if ( string.IsNullOrEmpty(genre) )
					genre = "Title";

				if ( genre == "Tag" ) {

					return GetBooksByTag(Keywords, orderby);

				} else if ( genre == "Author" ) {

					return GetBooksByAuthor(Keywords, orderby);

				} else {

					cmd.Connection.Open( );

					cmd.CommandText = "Select * from briefview";

					cmd.CommandText += string.Format(@" where {0} like '%{1}%' ", genre, Keywords.ElementAt(0));

					for ( int i = 1; i < Keywords.Count; i++ ) {
						cmd.CommandText += string.Format(@" or {0} like '%{1}%'", genre, Keywords.ElementAt(i));
					}

					if ( !string.IsNullOrEmpty(orderby) )
						cmd.CommandText += " order by " + Book.OrderByDictionary[orderby];

					cmd.CommandText += " limit " + QueryHandler.QueryLimit;

					using ( var b = cmd.ExecuteReader( ) ) {
						while ( b.Read( ) ) {
							result.Add(ReadBrief(b));
						}
					}

				}
			}

			return result;
		}

		public static string ReturnBooks(string username, string bookid ) {

			if ( string.IsNullOrEmpty(username) )
				return "You must Log in to borrow the book";

			_sqlInjectionFilter(ref username);
			_sqlInjectionFilter(ref bookid);

			using ( var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {

				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = string.Format(@"DELETE FROM user_borrow WHERE username = '{0}' and bookid = {1}", username, bookid);

				try {

					cmd.ExecuteNonQuery( );

				}

				catch ( MySqlException e ) {

					return e.Message;

				}
			}

			return "Return Success";
		}

		public static string BorrowBooks(string username, string bookid, string date ) {

			_sqlInjectionFilter(ref username);
			_sqlInjectionFilter(ref bookid);
			_sqlInjectionFilter(ref date);

			if ( string.IsNullOrEmpty(username) )
				return "You must Log in to borrow the book";

			using (var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {

				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = string.Format(@"SELECT * FROM user_borrow where bookid = '{0}'", bookid);

				using(var r = cmd.ExecuteReader( ) ) {
					if ( r.Read( ) )
						return "This book has been borrowed";
				}

				cmd.CommandText = string.Format(@"INSERT INTO user_borrow values('{0}', '{1}', '{2}')", username, bookid, date);

				try {

					cmd.ExecuteNonQuery();
					
				}

				catch(MySqlException e ) {
					
					Console.WriteLine( e.Message ) ;

				}

				cmd.Connection.Close( );

			}

			return "Borrow Success";
		}
		
		public static List<Book> GetBooksInBorrow(string username , out Dictionary<string,string> BorrowDate) {

			_sqlInjectionFilter(ref username);

			List<Book> result = new List<Book>();

			BorrowDate = new Dictionary<string, string>( );

			using(var cn = new MySqlConnection(connectionString: Book.ConnectionString) ) {

				var cmd = cn.CreateCommand( );

				cmd.Connection.Open( );

				cmd.CommandText = 
					string.Format(@"SELECT * FROM BriefView as B join user_borrow as U on B.id = U.bookId where U.username = '{0}'", username);
				
				using(var r = cmd.ExecuteReader( )){
					while ( r.Read( ) ) {
						result.Add(ReadBrief(r));
						BorrowDate.Add(r["id"].ToString(), r["borrow_date"].ToString( ).Split().ElementAt(0));
					}
				}

				cmd.Connection.Close( );
				
			}

			return result;
		}
		
	}
	


}