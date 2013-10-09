using System;
using NUnit.Framework;
using Raven.Client.Document;
using Raven.Database.Config;
using Raven.Server;
using Raven.Tests.Helpers;

namespace RavenTests
{
	public class MailingListIncludeTest : RavenTestBase
	{
		[Test]
        public void Test()
        {
			using (var store = NewDocumentStore())
			{
				EntityA a = new EntityA { Id = Guid.NewGuid() };
				EntityB b = new EntityB { Id = a.Id };
                EntityC c = new EntityC { Id = Guid.NewGuid(), EntityAId = a.Id, EntityBId = b.Id };


                using (var session = store.OpenSession())
                {
                    session.Store(a);
                    session.Store(b);
                    session.Store(c);
                    session.SaveChanges();
					Assert.AreEqual(store.DatabaseCommands.GetStatistics().CountOfDocuments, 3);
                }

                using (var session = store.OpenSession())
                {
                    var resultA = session.Load<EntityA>(c.EntityAId);
                    var resultB = session.Load<EntityB>(c.EntityBId);

                    var resultC = session.Load<EntityC>(c.Id);

                    Assert.NotNull(resultA, "resultA");
                    Assert.NotNull(resultB, "resultB - Load");
                    Assert.NotNull(resultC, "resultC");

                    Assert.AreEqual(session.Advanced.NumberOfRequests, 3);
                }

                using (var session = store.OpenSession())
                {
	                var resultC =
		                session
			                .Include<EntityC, EntityA>(x => x.EntityAId)
			                .Include<EntityB>(x => x.EntityBId)
							.Load<EntityC>(c.Id);

                    var resultA = session.Load<EntityA>(a.Id);
                    var resultB = session.Load<EntityB>(b.Id);

                    Assert.AreEqual(1, session.Advanced.NumberOfRequests);
                    Assert.NotNull(resultC, "resultC");
                    Assert.NotNull(resultA, "resultA");
                    Assert.NotNull(resultB, "resultB - Include");
                }
            }
        }

        public class EntityA
        {
            public Guid Id { get; set; }
        }

        public class EntityB
        {
            public Guid Id { get; set; }
        }

		public class EntityC
		{
			public Guid Id { get; set; }

			public Guid EntityAId { get; set; }

			public Guid EntityBId { get; set; }
		}
	}

	public class MailingListIncludeTest2 : RavenTestBase
	{
[Test]
public void Test()
{
	using (var store = new DocumentStore())
	{
		store.Url = "http://localhost:8080";
		store.DefaultDatabase = "test-includes";
		store.Initialize();

		EntityA a = new EntityA { External = Guid.NewGuid() };
		EntityB b = new EntityB { External = a.External };
		EntityC c = new EntityC { External = Guid.NewGuid(), EntityAId = a.Id, EntityBId = b.Id };


		using (var session = store.OpenSession())
		{
			session.Store(a);
			session.Store(b);
			session.Store(c);
			session.SaveChanges();
			//Assert.AreEqual(store.DatabaseCommands.GetStatistics().CountOfDocuments, 3);
		}

		using (var session = store.OpenSession())
		{
			var resultA = session.Load<EntityA>(c.EntityAId);
			var resultB = session.Load<EntityB>(c.EntityBId);

			var resultC = session.Load<EntityC>(c.Id);

			Assert.NotNull(resultA, "resultA");
			Assert.NotNull(resultB, "resultB");
			Assert.NotNull(resultC, "resultC");

			Assert.AreEqual(session.Advanced.NumberOfRequests, 3);
		}

		using (var session = store.OpenSession())
		{
			var resultC =
				session
					.Include<EntityC>(x => x.EntityAId)
					.Include(x => x.EntityBId)
					.Load(c.Id);

			var resultA = session.Load<EntityA>(a.Id);
			var resultB = session.Load<EntityB>(b.Id);

			Assert.AreEqual(1, session.Advanced.NumberOfRequests);
			Assert.NotNull(resultC, "resultC");
			Assert.NotNull(resultA, "resultA");
			Assert.NotNull(resultB, "resultB");
		}
	}
}

public class EntityA
{
	public string Id { get { return "entityAs/" + External; } }
	public Guid External { get; set; }
}

public class EntityB
{
	public string Id { get { return "entityBs/" + External; } }
	public Guid External { get; set; }
}

public class EntityC
{
	public string Id { get { return "entityCs/" + External; } }
	public Guid External { get; set; }

	public string EntityAId { get; set; }

	public string EntityBId { get; set; }
}
	}
}