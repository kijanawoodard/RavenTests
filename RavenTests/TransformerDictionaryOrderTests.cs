﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;

namespace RavenTests
{
	[TestFixture]
	public class TransformerDictionaryOrderTests : RavenTestBase
	{
		[Test]
		public void CanOrderADictionary()
		{
			using (var store = NewDocumentStore())
			{
				new FooTransformer().Execute(store);

				using (var session = store.OpenSession())
				{
					session.Store(new Foo { Id = "foos/1", Dates = { { "hello", DateTimeOffset.UtcNow } } });
					session.SaveChanges();

					var results = session.Load<FooTransformer, FooTransformer.Result>("foos/1");
					Assert.AreEqual(1, results.Keys.Count);
				}
			}
		}

		public class Foo
		{
			public string Id { get; set; }
			public Dictionary<string, DateTimeOffset> Dates { get; set; }

			public Foo()
			{
				Dates = new Dictionary<string, DateTimeOffset>();
			}
		}

		public class FooTransformer : AbstractTransformerCreationTask<Foo>
		{
			public class Result
			{
				public List<string> Keys { get; set; }
			}

			public FooTransformer()
			{
				TransformResults = foos => from foo in foos
				                           select new
				                           {
					                           Keys = foo.Dates.OrderBy(x => x.Value).Select(x => x.Key).ToList()
				                           };
			}
		}
	}
}