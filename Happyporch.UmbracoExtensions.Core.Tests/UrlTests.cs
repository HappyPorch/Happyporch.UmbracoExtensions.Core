using HappyPorch.UmbracoExtensions.Core.Extensions;
using NUnit.Framework;

namespace Endzone.Umbraco.Extensions.Tests
{
    [TestFixture]
    public class UrlTests
    {
        [Test]
        [TestCase("/relative", "b", "2", "/relative?b=2")]
        [TestCase("/relative?a=1", "b", "2", "/relative?a=1&b=2")]
        [TestCase("/relative?a=1&b=2", "b", "3", "/relative?a=1&b=3")]
        [TestCase("/relative?b=1", "b", "2", "/relative?b=2")]
        [TestCase("http://test.com?a=1", "b", "2", "http://test.com?a=1&b=2")]
        [TestCase("?a=1", "b", "2", "?a=1&b=2")]
        [TestCase("?a=1", "a", "2", "?a=2")]
        [TestCase("/relative?a=1#test", "b", "2", "/relative?a=1&b=2#test")]
        public static void CanChangeQueryParam(string url, string param, string value, string expected)
        {
            Assert.AreEqual(expected, UrlExtensions.SetUrlParameter(url, param, value));
        }
    }
}
