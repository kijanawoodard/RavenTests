using System;
using System.IO;
using System.Reflection;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
using NUnit.Framework;

namespace RavenTests
{
	public class CombineUserAndSiteDocuments
	{
		[Test]
		public void CanCombineDocuments()
		{
			var site = new Product
			{
				Id = "products/site/someupc",
				Name = "Jam",
				Price = 3.25m
			};

			var user = new Product
			{
				Id = "products/user/someupc",
				Name = "Jam",
				Price = 2.99m
			};

			var merged = Merge(site, user);
			Assert.AreEqual(2.99m, merged.Price);
		}

		private dynamic Merge(Product site, Product user)
		{
			var engine = new V8ScriptEngine();
			var underscore = File.ReadAllText("scripts/underscore.min.js");
			engine.Execute(underscore);
			engine.Execute("function extend(thing1, thing2) { return _.extend({}, thing1, thing2) }");
			var product = engine.Script.extend(site, user);
			return product;
		}

		public class Product
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public decimal Price { get; set; }
		}
	}



	//http://odetocode.com/blogs/scott/archive/2013/09/10/hosting-a-javascript-engine-in-net.aspx
	public interface IJavaScriptMachine : IDisposable
	{
		dynamic Evaluate(string expression);
	}

	public class JavaScriptMachine : JScriptEngine,
								 IJavaScriptMachine
	{
		public JavaScriptMachine()
		{
			LoadDefaultScripts();
		}

		void LoadDefaultScripts()
		{
			var assembly = Assembly.GetExecutingAssembly();
			foreach (var baseName in _scripts)
			{
				var fullName = _scriptNamePrefix + baseName;
				using (var stream = assembly.GetManifestResourceStream(fullName))
				using (var reader = new StreamReader(stream))
				{
					var contents = reader.ReadToEnd();
					Execute(contents);
				}
			}
		}

		const string _scriptNamePrefix = "Foo.Namespace.";
		readonly string[] _scripts = new[]
        {
            "underscore.js", "other.js"
        };
	}
}