using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Json.Linq;
using Xunit;
using Assert = Xunit.Assert;

namespace RavenTests
{
	public class TypeNameTests
	{
		[Fact]
		public void IndexTypeNameShouldMatchFindByTypeName()
		{
			var typeName = DocumentConvention.DefaultTypeTagName(typeof (BackgroundTask));
			var index = RavenJObject.FromObject(new TaskIndex().CreateIndexDefinition()).ToString();

			Assert.Contains(typeName, index);
		}

		public abstract class BackgroundTask
		{
			public static string StartingWith = "tasks/";

			public string Id { get; set; }
			
			protected BackgroundTask()
			{
				Id = StartingWith; //setup all tasks to have sequental ids
			}
		}

		class TaskIndex : AbstractIndexCreationTask<BackgroundTask, TaskIndex.Result>
		{
			public class Result
			{
				public Guid Etag { get; set; }
			}

			public TaskIndex()
			{
				Map = tasks => from task in tasks
							   select new
							   {
								   Etag = MetadataFor(task)["@etag"],
							   };

				Sort(x => x.Etag, SortOptions.String);
			}
		}
	}
}