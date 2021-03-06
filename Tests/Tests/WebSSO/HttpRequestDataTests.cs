﻿using FluentAssertions;
using Sustainsys.Saml2.WebSso;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sustainsys.Saml2.Tests.WebSSO
{
    [TestClass]
    public class HttpRequestDataTests
    {
        [TestMethod]
        public void HttpRequestData_Ctor_FromParamsCalculatesApplicationUrl()
        {
            var url = new Uri("http://example.com:42/ApplicationPath/Path?name=DROP%20TABLE%20STUDENTS");
            string appPath = "/ApplicationPath";

            var subject = new HttpRequestData(
                 "GET",
                 url,
                 appPath,
                 new KeyValuePair<string, IEnumerable<string>>[]
                {
                    new KeyValuePair<string, IEnumerable<string>>("Key", new string[] { "Value" })
                },
                null,
                null);

            subject.ApplicationUrl.Should().Be(new Uri("http://example.com:42/ApplicationPath"));
        }

        [TestMethod]
        public void HttpRequestData_EscapeBase64CookieValue_Nullcheck()
        {
            Action a = () => HttpRequestData.ConvertBinaryData(null);

            a.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("data");
        }

        [TestMethod]
        public void HttpRequestData_Ctor_RelayStateButNoCookie()
        {
            var url = new Uri("http://example.com:42/ApplicationPath/Path?RelayState=Foo");
            string appPath = "/ApplicationPath";

            Action a = () => new HttpRequestData(
                 "GET",
                 url,
                 appPath,
                 new KeyValuePair<string, IEnumerable<string>>[]
                 {
                    new KeyValuePair<string, IEnumerable<string>>("Key", new string[] { "Value" })
                 },
                 Enumerable.Empty<KeyValuePair<string, string>>(),
                 null);

            a.ShouldNotThrow();
        }

        [TestMethod]
        public void HttpRequestData_Ctor_RelayStateExtractorOverride()
        {
            var state = Guid.NewGuid().ToString();
            var url = new Uri("http://example.com:42/ApplicationPath/Path?RelayState=" + Uri.EscapeDataString("https://localhost?RelayState=" + state));
            string appPath = "/ApplicationPath";

            var request = new HttpRequestData(
                "GET",
                url,
                appPath,
                new KeyValuePair<string, IEnumerable<string>>[]
                    {
                        new KeyValuePair<string, IEnumerable<string>>("Saml." + state, new string[] { "Value" })
                    },
                Enumerable.Empty<KeyValuePair<string, string>>(),
                null,
                null,
                relayStateExtractor: relayState => relayState.Replace("https://localhost?RelayState=", string.Empty));

            Assert.AreEqual(request.RelayState, state);
        }

        [TestMethod]
        public void HttpRequestData_Ctor_RelayStateExtractorNoOverride()
        {
            var state = Guid.NewGuid().ToString();
            var url = new Uri("http://example.com:42/ApplicationPath/Path?RelayState=" + state);
            string appPath = "/ApplicationPath";

            var request = new HttpRequestData(
                "GET",
                url,
                appPath,
                new KeyValuePair<string, IEnumerable<string>>[]
                    {
                        new KeyValuePair<string, IEnumerable<string>>("Saml." + state, new string[] { "Value" })
                    },
                Enumerable.Empty<KeyValuePair<string, string>>(),
                null,
                null,
                null);

            Assert.AreEqual(request.RelayState, state);
        }
    }
}