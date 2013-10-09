using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;

namespace RavenTests
{
	[TestFixture]
	public class MailingListTests : RavenTestBase
	{

		[Test]
		public void GetLatestBlogViewings()
		{
			using (var store = NewDocumentStore())
			{
				new ViewingsWithPosts().Execute(store);

				using (var session = store.OpenSession())
				{
					//posts are created 
					session.Store(new Post { Id = "posts/1234", Name = "A Great Post" });
					session.Store(new Post { Id = "posts/9999", Name = "Another Post" });

					//when the user is created
					var viewing = new PostViewings { UserId = "users/1" };
					session.Store(viewing);

					session.SaveChanges();
				}

				//user looks at some posts 
				using (var session = store.OpenSession())
				{
					var viewing = session.Load<PostViewings>(PostViewings.FormatId("users/1"));
					viewing.RecordViewing("posts/1234");
					viewing.RecordViewing("posts/9999");

					session.SaveChanges();
				}

				//check out most viewed
				using (var session = store.OpenSession())
				{
					var viewing = session.Load<PostViewings>(PostViewings.FormatId("users/1"));

					var mostViewed = viewing.RecentPosts
					                        .Take(1).ToList();
					Assert.AreEqual(1, mostViewed.Count);
				}
			}
		}


		[Test]
		public void GetLatestBlogViewingsWithTransformers()
		{
			using (var store = NewDocumentStore())
			{
				new ViewingsWithPosts().Execute(store);

				using (var session = store.OpenSession())
				{
					//posts are created 
					session.Store(new Post { Id = "posts/1234", Name = "A Great Post" });
					session.Store(new Post { Id = "posts/9999", Name = "Another Post" });

					//when the user is created
					var viewing = new PostViewings { UserId = "users/1" };
					session.Store(viewing);

					session.SaveChanges();
				}

				//user looks at some posts 
				using (var session = store.OpenSession())
				{
					var viewing = session.Load<PostViewings>(PostViewings.FormatId("users/1"));
					viewing.RecordViewing("posts/1234");
					viewing.RecordViewing("posts/9999");

					session.SaveChanges();
				}

				//check out most viewed
				using (var session = store.OpenSession())
				{
					var mostViewed =
						session
							.Load<ViewingsWithPosts, ViewingsWithPosts.Result>(
								PostViewings.FormatId("users/1"),
								configuration => configuration.AddQueryParam("take", 1)
							);
					var result = mostViewed.Posts.Take(1).ToList();
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("posts/1234", result.First().Id);
				}
			}
		}

		[Test]
		public void LoadEmptyString()
		{
			using (var store = NewDocumentStore())
			using (var session = store.OpenSession())
			{
				session.Load<Post>(string.Empty);
			}
		}

		public class Post
		{
			public string Id { get; set; }
			public string Name { get; set; }

			//lots of other properties
			public string Author { get; set; }
			public string Body { get; set; }
			public DateTimeOffset Created { get; set; }
			//.....
		}

		public class PostViewings
		{
			public static string FormatId(string userid)
			{
				return string.Format("{0}/viewings", userid);
			}

			public string Id { get { return FormatId(UserId); } }
			public string UserId { get; set; }
			public ConcurrentDictionary<string, DateTimeOffset> Viewings { get; set; }
			public IEnumerable<string> RecentPosts { get { return Viewings.OrderBy(x => x.Value).Select(x => x.Key); } }

			public void RecordViewing(string postId)
			{
				Viewings.AddOrUpdate(
					postId,
					addValue: DateTimeOffset.UtcNow,
					updateValueFactory: (id, dt) => DateTimeOffset.UtcNow);
			}

			public PostViewings()
			{
				Viewings = new ConcurrentDictionary<string, DateTimeOffset>();
			}
		}

		public class ViewingsWithPosts : AbstractTransformerCreationTask<PostViewings>
		{
			public class Result
			{
				public IEnumerable<ViewModel> Posts { get; set; }
				public int Take { get; set; }

				public class ViewModel
				{
					public string Id { get; set; }
					public string Name { get; set; }
				}
			}

			public ViewingsWithPosts()
			{
				TransformResults = docs => from doc in docs
				                           select new
				                           {
					                           Posts =
					                           LoadDocument<Post>(doc.RecentPosts)
					                           .Select(x => new Result.ViewModel { Id = x.Id, Name = x.Name }),
					                           Take = "" + Query("take")
				                           };
			}
		}
	}
}