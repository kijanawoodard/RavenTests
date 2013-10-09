using System;
using System.Linq;
using Raven.Tests.Helpers;
using Xunit;

namespace RavenTests
{
	public class SessionTests : RavenTestBase
	{
		[Fact]
		public void HasChangedWorks()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Foo(){Id = "foos/1"});
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					var foo = session.Load<Foo>("foos/1");
					foo.Whatever = "bar";
					Assert.True(session.Advanced.HasChanged(foo));
				}
			}
		}

		[Fact]
		public void CanChangeSessionAndSaveChanges()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Foo() { Id = "foos/1" });
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					var foo = session.Load<Foo>("foos/1");
					var etag = session.Advanced.GetEtagFor(foo);
					
					using (var anotherSession = store.OpenSession())
					{
						anotherSession.Store(foo, etag);

						foo.Whatever = "bar";
						anotherSession.SaveChanges();
					}
				}

				using (var session = store.OpenSession())
				{
					Assert.Equal("bar", session.Load<Foo>("foos/1").Whatever);
				}
				
			}
		}

		[Fact]
		public void RequestsThatDontTouchServerDontIncrementSessionCount()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Foo() { Id = "foos/1" });
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					session.Load<Foo>("foos/1");
					Assert.Equal(1, session.Advanced.NumberOfRequests);

					session.Load<Foo>("foos/1");
					Assert.Equal(1, session.Advanced.NumberOfRequests);
				}
			}
		}

		[Fact]
		public void AggressivelyCachedRequestsThatDontTouchServerDontIncrementSessionCount()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Foo() { Id = "foos/1" });
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					for (var i = 0; i < 30; i++)
					{
						using (store.AggressivelyCacheFor(TimeSpan.FromMinutes(2)))
						{
							session.Load<Foo>("foos/1");
							Assert.Equal(1, session.Advanced.NumberOfRequests);
						}
					}
				}
			}
		}

		[Fact]
		public void AggressivelyCachedQueriesThatDontTouchServerDontIncrementSessionCount()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Foo() { Id = "foos/1" });
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					for (var i = 0; i < 30; i++)
					{
						using (store.AggressivelyCacheFor(TimeSpan.FromMinutes(2)))
						{
							session.Query<Foo>().ToList();
							Assert.Equal(1, session.Advanced.NumberOfRequests);
						}
					}
				}
			}
		}

		public class Foo
		{
			public string Id { get; set; }
			public string Whatever { get; set; }
		}
	}
}