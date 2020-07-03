using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yort.Zip.InStore.Tests
{
	[TestClass]
	public class ZipEnvironmentTests
	{
		[TestMethod]
		public void ZipEnvironment_NewZealand_Test_IsNotNull()
		{
			Assert.IsNotNull(ZipEnvironment.NewZealand.Test);
			Assert.IsFalse(String.IsNullOrWhiteSpace(ZipEnvironment.NewZealand.Test.Audience));
			Assert.IsNotNull(ZipEnvironment.NewZealand.Test.BaseUrl);
			Assert.IsNotNull(ZipEnvironment.NewZealand.Test.TokenEndpoint);
		}

		[TestMethod]
		public void ZipEnvironment_NewZealand_Production_IsNotNull()
		{
			Assert.IsNotNull(ZipEnvironment.NewZealand.Production);
			Assert.IsFalse(String.IsNullOrWhiteSpace(ZipEnvironment.NewZealand.Production.Audience));
			Assert.IsNotNull(ZipEnvironment.NewZealand.Production.BaseUrl);
			Assert.IsNotNull(ZipEnvironment.NewZealand.Production.TokenEndpoint);
		}
	}
}
