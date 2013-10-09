using System;
using System.Linq;
using Raven.Client.Embedded;
using Raven.Tests.Helpers;
using Xunit;

namespace RavenTests
{
	public class AggressiveCachingTests : RavenTestBase
	{
		//Passes
		[Fact]
		public void AggressivelyCachedLoadsThatDontTouchServerDontIncrementSessionCount()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new SessionTests.Foo() { Id = "foos/1" });
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					for (var i = 0; i < 30; i++)
					{
						using (store.AggressivelyCacheFor(TimeSpan.FromMinutes(2)))
						{
							session.Load<SessionTests.Foo>("foos/1");
							Assert.Equal(1, session.Advanced.NumberOfRequests);
						}
					}
				}
			}
		}

		//Fails
		[Fact]
		public void AggressivelyCachedQueriesThatDontTouchServerDontIncrementSessionCount()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new SessionTests.Foo() { Id = "foos/1" });
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					for (var i = 0; i < 30; i++)
					{
						using (store.AggressivelyCacheFor(TimeSpan.FromMinutes(2)))
						{
							session.Query<SessionTests.Foo>().ToList();
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
