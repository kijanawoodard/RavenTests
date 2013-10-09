using System.Linq;
using NUnit.Framework;
using Raven.Client.Document;
using Raven.Tests.Helpers;

namespace RavenTests
{
	public class EnumTests : RavenTestBase
	{
		private DocumentStore _documentStore;

		[SetUp]
		public void Initialize()
		{
			_documentStore = new DocumentStore() { DefaultDatabase = "test-enum", Url = "http://localhost:8080" }; //NewDocumentStore();
			_documentStore.Initialize();
		}

		[Test]
		public void InsertInitial()
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(new Foo{Id = "foos/1", Bar = Bar.Orange});
				session.SaveChanges();
			}
		}

		[Test]
		public void ShouldMaintainEnumValues()
		{
			using (var session = _documentStore.OpenSession())
			{
				var foo = session.Load<Foo>("foos/1");
				var foos = session.Query<Foo>().ToList();
				Assert.AreEqual(Bar.Orange, foo.Bar);
//				foo.Bar = Bar.Orange;
				session.SaveChanges();
			}
			using (var session = _documentStore.OpenSession())
			{
				var foo = session.Load<Foo>("foos/1");
				Assert.AreEqual(Bar.Orange, foo.Bar);
			}
		}

		public enum Bar
		{
			Guyava,
			Papaya,
			Apple,
			Orange,
			Grapefruit,
			Carrot,
			Pineapple
		}

		public class Foo
		{
			public string Id { get; set; }
			public Bar Bar { get; set; }
		}
	}
}