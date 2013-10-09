using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq.Indexing;
using Raven.Tests.Helpers;

namespace RavenTests
{
	public class IndexFailTests : RavenTestBase
	{
		[Test]
		public void CanUpdateAnIndexWithALotOfData()
		{
			var sw = Stopwatch.StartNew();
			Debug.WriteLine("starting " + sw.Elapsed.TotalSeconds);
			using (var store = new DocumentStore() { DefaultDatabase = "test-status-hang", Url = "http://localhost:8080" })
			{
				store.Initialize();
				
				var index1 = new ProductIndexOriginial().CreateIndexDefinition();
				store.DatabaseCommands.PutIndex("ProductIndex", index1);
				
				var max = 2 * 1000 * 1000;
				using (var bulkInsert = store.BulkInsert(options: new BulkInsertOptions()))
				{
					Enumerable.Range(1, max)
					          .Select(x => new Product() {Id = "products/" + x, Name = "Product " + x})
					          .ToList()
					          .ForEach(x => bulkInsert.Store(x));

					var stat = new ProductStat()
					{
						Id = "products/1/stats",
						ProductId = "products/1",
						Likes = 1,
						Buys = 0,
						Created = DateTimeOffset.UtcNow
					};

					bulkInsert.Store(stat);
				}

				Debug.WriteLine("products written " + sw.Elapsed.TotalSeconds);

				WaitForIndexing(store);
				Debug.WriteLine("indexed " + sw.Elapsed.TotalSeconds);

				using (var session = store.OpenSession())
				{
					var count = session.Query<Product>("ProductIndex").Count();
					Assert.True(max == count);
				}

				Debug.WriteLine("query complete - count matches " + sw.Elapsed.TotalSeconds);

				new ProductIndex().Execute(store);
				Debug.WriteLine("index update applied " + sw.Elapsed.TotalSeconds);

				Debug.WriteLine("done " + sw.Elapsed.TotalSeconds);
			}
		}

		public class Product
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public bool IsAvailableForPublicSearch { get; set; } //hook for search
			public double TrendScore { get; set; }
		}

		public class ProductStat
		{
			public string Id { get; set; }
			public string ProductId { get; set; }
			public int Likes { get; set; }
			public int Buys { get; set; }
			public DateTimeOffset Created { get; set; }
			public bool IsAvailableForPublicSearch { get; set; }

			public double Trend
			{
				get
				{
					var wants = Math.Max(Likes*10, 1);
					var clicks = Math.Max(Buys/2, 1);
					var score = Math.Log10(wants + clicks);
					var epoch = new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero);
					var seconds = (Created - epoch).TotalSeconds/200*1000;
					var trend = Math.Pow(Math.Round(score + seconds, 7), 0.25);
					return trend;
				}
			}
		}

		public class ProductIndexOriginial : AbstractIndexCreationTask<Product>
		{
			public ProductIndexOriginial()
			{
				Map = products => from product in products
								  let stat = LoadDocument<ProductStat>(product.Id + "/stats")
								  let wants = stat == null ? 0 : Math.Max(stat.Likes * 10, 1)
								  let clicks = stat == null ? 0 : Math.Max(stat.Buys / 2, 1)
								  let score = stat == null ? 0 : Math.Log10(wants + clicks)
								  let epoch = new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)
								  let seconds = stat == null ? 0 : (stat.Created - epoch).TotalSeconds / 200 * 1000
								  let trend = Math.Pow(Math.Round(score + seconds, 7), 0.25)
								  select new
								  {
									  product.Name,
									  IsAvailableForPublicSearch = (stat == null || stat.IsAvailableForPublicSearch),
									  TrendScore = trend,
								  }.Boost((float)trend);

				Index(x => x.Name, FieldIndexing.Analyzed);
				Sort(x => x.TrendScore, SortOptions.Double);
			}
		}

		public class ProductIndex : AbstractIndexCreationTask<Product>
		{
			public ProductIndex()
			{
				Map = products => from product in products
								  let stat = LoadDocument<ProductStat>(product.Id + "/stats")
								  let wants = stat == null ? 0 : Math.Max(stat.Likes * 10, 1)
								  let clicks = stat == null ? 0 : Math.Max(stat.Buys / 2, 1)
								  let score = stat == null ? 0 : Math.Log10(wants + clicks)
								  let epoch = new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)
								  let seconds = stat == null ? 0 : (stat.Created - epoch).TotalSeconds / 200 * 1000
								  let trend = Math.Pow(Math.Round(score + seconds, 7), 0.25)
								  select new
								  {
									  product.Name,
									  IsAvailableForPublicSearch = (stat == null || stat.IsAvailableForPublicSearch),
									  TrendScore = trend,
								  }.Boost((float)trend + 1);

				Index(x => x.Name, FieldIndexing.Analyzed);
				Sort(x => x.TrendScore, SortOptions.Double);
			}
		}
	}
}