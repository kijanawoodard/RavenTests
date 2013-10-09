using Raven.Tests.Helpers;
using Xunit;

namespace RavenTests
{
	public class HttpIdTest : RavenTestBase
	{
		[Fact]
		public void CanLoadIdWithHttp()
		{
			using (var store = NewRemoteDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Foo{Id = "http://whatever"});
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					var foo = session.Load<Foo>("http://whatever");
					Assert.NotNull(foo);
				}
			}
		}

		public class Foo
		{
			public string Id { get; set; }
		}
	}
}