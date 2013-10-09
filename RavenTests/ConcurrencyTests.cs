using System;
using System.Linq;
using Raven.Abstractions.Smuggler;
using Raven.Database.Smuggler;
using Raven.Tests.Helpers;
using Xunit;

namespace RavenTests
{
	public class ConcurrencyTests : RavenTestBase
	{
		public static string FindTypeByTagName(Type type)
		{
			string name = type.Name;
			//if (name.EndsWith("Base"))
			//{
			//    name = name.Substring(0, name.Length - 4);
			//}
			//name = Raven.Client.Util.Inflector.Pluralize(name);
			return name;
		}

		[Fact]
		public void CanSaveImplicitChangesToDocumentsFromAQuery_UsingDunpFile()
		{
			using (var store = NewDocumentStore())
			{
				store.Conventions.FindTypeTagName = FindTypeByTagName;

				var options = new SmugglerOptions
				{
					BackupPath = @"Dump of test-concurrency-exception2, 21 May 2013 14-36.ravendump"
				};

				var dumper = new DataDumper(store.DocumentDatabase, options);
				dumper.ImportData(options);

				using (var session = store.OpenSession())
				{
					session.Advanced.UseOptimisticConcurrency = true;
					var foos =
						session.Query<SectionData>()
						       .Customize(x => x.WaitForNonStaleResults())
						       .Take(1024)
						       .ToList();

					Assert.True(foos.Count > 200);
					session.SaveChanges();
				}
			}
		}
	}

	public class SectionData
	{
		public string Id { get; set; }
		
	}
}