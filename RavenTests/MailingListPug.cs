using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;
using Xunit;

namespace RavenTests
{
	public class MailingListPug : RavenTestBase
	{
		[Fact]
		public void ShouldBeAbleToQuery()
		{
			using (var store = NewDocumentStore())
			{
				new LevelEntryIndex().Execute(store);
				using (var session = store.OpenSession())
				{
					session.Store(new LevelEntry { LevelKey = "level/foo", Workers = new List<string> { "a", "b" }, Start = DateTimeOffset.Parse("12/31/2012") });
					session.Store(new LevelEntry { LevelKey = "level/bar", Workers = new List<string> { "a", "b", "c" }, Start = DateTimeOffset.Parse("1/1/2013") });
					session.SaveChanges();
				}

				WaitForIndexing(store);

				using (var session = store.OpenSession())
				{
					var entries =
						session
							.Query<LevelEntry, LevelEntryIndex>()
							.ToList();

					Assert.Equal(2, entries.Count);
				}

				using (var session = store.OpenSession())
				{
					var entries =
						session
							.Query<LevelEntry, LevelEntryIndex>()
							.Where(x => x.Workers.Count > 2)
							.ToList();

					Assert.Equal(1, entries.Count);
					Assert.Equal("level/bar", entries.First().LevelKey);
				}

				using (var session = store.OpenSession())
				{
					var entries =
						session
							.Query<LevelEntry, LevelEntryIndex>()
							.Where(x => x.Start < DateTimeOffset.Parse("1/1/2013"))
							.ToList();

					Assert.Equal(1, entries.Count);
					Assert.Equal("level/foo", entries.First().LevelKey);
				}
			}

		}

		public class LevelEntry
		{
			public string Id { get; set; }
			public string LevelKey { get; set; }
			public List<string> Workers { get; set; }
			public DateTimeOffset Start { get; set; }
		}

		public class LevelEntryIndex : AbstractIndexCreationTask<LevelEntry>
		{
			public LevelEntryIndex()
			{
				Map = entries => from entry in entries
				                 select new
				                 {
									 entry.LevelKey,
									 Workers_Count = entry.Workers.Count,
									 entry.Start
				                 };
			}
		}
	}
}