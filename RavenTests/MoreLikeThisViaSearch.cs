using System.Linq;
using NUnit.Framework;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;

namespace RavenTests
{
	public class MoreLikeThisViaSearch : RavenTestBase
	{
		[Test]
		public void FindSimilarContent()
		{
			using (var store = NewDocumentStore())
			{
				new DocumentIndex().Execute(store);
				using (var session = store.OpenSession())
				{
					session.Store(new Document {Content = "This article is about cats."});
					session.Store(new Document { Content = "This article is about cats and dogs." });
					session.Store(new Document { Content = "This article is about fat cats and alley cats." });
					session.Store(new Document { Content = "nom nom nom" });
					session.SaveChanges();
				}

				WaitForIndexing(store);

				using (var session = store.OpenSession())
				{
					//pretend we got a request for a doc
					var docId = "documents/1";
					var doc = session.Load<Document>(docId);
					var morelikethis =
						session
							.Query<Document, DocumentIndex>()
							.Search(x => x.Content, doc.Content)
							.Take(20)
							.ToList()
							.Where(x => x.Id != docId) //filter client side so we don't show "this" doc in our more like this results
							.ToList();

					Assert.AreEqual(2, morelikethis.Count);
					Assert.AreEqual("This article is about fat cats and alley cats.", morelikethis.First().Content);
				}
			}
		}

		public class Document
		{
			public string Id { get; set; }
			public string Content { get; set; }
		}

		public class DocumentIndex : AbstractIndexCreationTask<Document>
		{
			public DocumentIndex()
			{
				Map = documents => from document in documents
				                   select new
				                   {
					                   document.Content
				                   };

				Index(x => x.Content, FieldIndexing.Analyzed);
			}
		}
	}
}