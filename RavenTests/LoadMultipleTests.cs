using NUnit.Framework;
using Raven.Client.Document;
using Raven.Tests.Helpers;

namespace RavenTests
{
	public class LoadMultipleTests : RavenTestBase
	{
		private DocumentStore _documentStore;

		[SetUp]
		public void Initialize()
		{
			_documentStore = new DocumentStore() { DefaultDatabase = "test-load-multiple", Url = "http://localhost:8080" }; //NewDocumentStore();
			_documentStore.Initialize();
		}

		[Test]
		public void InsertAndSingleSelect()
		{
			var expected = new Bar { Id = "test/bar/1", Foo = "Some value" };
			using (var session = _documentStore.OpenSession())
			{
				session.Store(expected);
				session.SaveChanges();
			}
			using (var session = _documentStore.OpenSession())
			{
				var actual = session.Load<Bar>(expected.Id);
				Assert.AreEqual(expected.Id, actual.Id, "Id mismatch.");
				Assert.AreEqual(expected.Foo, actual.Foo, "Foo mismatch");
			}
		}

		[Test]
		public void InsertAndMultiSelect()
		{
			var expected = new Bar { Id = "test/bar/1", Foo = "Some value" };
			using (var session = _documentStore.OpenSession())
			{
				session.Store(expected);
				session.SaveChanges();
			}
			using (var session = _documentStore.OpenSession())
			{
				var actualList = session.Load<Bar>(expected.Id, "i do not exist");
				Assert.AreEqual(2, actualList.Length, "Count mismatch.");
				Assert.IsNotNull(actualList[0], "First element should not be null.");
				Assert.IsNull(actualList[1], "Second element should be null.");
				Assert.AreEqual(expected.Id, actualList[0].Id);
				Assert.AreEqual(expected.Foo, actualList[0].Foo);
			}
		}

		public class Bar
		{
			public string Id { get; set; }
			public string Foo { get; set; }
		}
	}
}