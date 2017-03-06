// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Test.Utility;
using Test.Utility;
using Xunit;

namespace NuGet.Protocol.Tests
{
    public class PackageUpdateResourceTests
    {
        private const string ApiKeyHeader = "X-NuGet-ApiKey";
        private const string NuGetClientVersionHeader = "X-NuGet-Client-Version";

        [Fact]
        public async Task PackageUpdateResource_IncludesApiKeyWhenDeleting()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                HttpRequestMessage actualRequest = null;
                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/DeepEqual/1.4.0.1-rc",
                        request =>
                        {
                            actualRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    }
                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                var apiKey = "SomeApiKey";

                // Act
                await resource.Delete(
                    packageId: "DeepEqual",
                    packageVersion: "1.4.0.1-rc",
                    getApiKey: _ => apiKey,
                    confirm: _ => true,
                    log: NullLogger.Instance);

                // Assert
                Assert.NotNull(actualRequest);
                Assert.Equal(HttpMethod.Delete, actualRequest.Method);

                IEnumerable<string> values;
                actualRequest.Headers.TryGetValues(ProtocolConstants.ApiKeyHeader, out values);
                Assert.Equal(1, values.Count());
                Assert.Equal(apiKey, values.First());

                Assert.False(
                    actualRequest.GetOrCreateConfiguration().PromptOn403,
                    "When the API key is provided, the user should not be prompted on HTTP 403.");
            }
        }

        [Fact]
        public async Task PackageUpdateResource_AllowsNoApiKeyWhenDeleting()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                HttpRequestMessage actualRequest = null;
                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/DeepEqual/1.4.0.1-rc",
                        request =>
                        {
                            actualRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    }
                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                var apiKey = string.Empty;

                // Act
                await resource.Delete(
                    packageId: "DeepEqual",
                    packageVersion: "1.4.0.1-rc",
                    getApiKey: _ => apiKey,
                    confirm: _ => true,
                    log: NullLogger.Instance);

                // Assert
                Assert.NotNull(actualRequest);
                Assert.Equal(HttpMethod.Delete, actualRequest.Method);

                IEnumerable<string> values;
                actualRequest.Headers.TryGetValues(ProtocolConstants.ApiKeyHeader, out values);
                Assert.Null(values);

                Assert.True(
                    actualRequest.GetOrCreateConfiguration().PromptOn403,
                    "When the API key is not provided, the user should be prompted on HTTP 403.");
            }
        }

        [Fact]
        public async Task PackageUpdateResource_AllowsApiKeyWhenPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                HttpRequestMessage actualRequest = null;
                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/",
                        request =>
                        {
                            actualRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    }
                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                var apiKey = "SomeApiKey";

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: null,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => null,
                    log: NullLogger.Instance);

                // Assert
                Assert.NotNull(actualRequest);
                Assert.Equal(HttpMethod.Put, actualRequest.Method);

                IEnumerable<string> values;
                actualRequest.Headers.TryGetValues(ProtocolConstants.ApiKeyHeader, out values);
                Assert.Equal(1, values.Count());
                Assert.Equal(apiKey, values.First());

                Assert.False(
                    actualRequest.GetOrCreateConfiguration().PromptOn403,
                    "When the API key is provided, the user should not be prompted on HTTP 403.");
            }
        }

        [Fact]
        public async Task PackageUpdateResource_AllowsNoApiKeyWhenPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                HttpRequestMessage actualRequest = null;
                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/",
                        request =>
                        {
                            actualRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    }
                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                var apiKey = string.Empty;

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: null,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => null,
                    log: NullLogger.Instance);

                // Assert
                Assert.NotNull(actualRequest);
                Assert.Equal(HttpMethod.Put, actualRequest.Method);

                IEnumerable<string> values;
                actualRequest.Headers.TryGetValues(ProtocolConstants.ApiKeyHeader, out values);
                Assert.Null(values);

                Assert.True(
                    actualRequest.GetOrCreateConfiguration().PromptOn403,
                    "When the API key is not provided, the user should be prompted on HTTP 403.");
            }
        }

        [Fact]
        public async Task PackageUpdateResource_SourceAndSymbolNuGetOrgPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                var symbolSource = "https://nuget.smbsrc.net/";
                HttpRequestMessage sourceRequest = null;
                HttpRequestMessage symbolRequest = null;
                var apiKey = "serverapikey";

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");
                var symbolPackageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0", isSymbolPackage: true);

                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/",
                        request =>
                        {
                            sourceRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://nuget.smbsrc.net/api/v2/package/",
                        request =>
                        {
                            symbolRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://www.nuget.org/api/v2/package/create-verification-key/test/1.0.0",
                        request =>
                        {
                            var content = new StringContent(JsonData.tempApiKeyJsonData, Encoding.UTF8, "application/json");
                            var response = new HttpResponseMessage(HttpStatusCode.OK);
                            response.Content = content;
                            return Task.FromResult(response);
                        }
                    }

                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                UserAgent.SetUserAgentString(new UserAgentStringBuilder("test client"));

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: symbolSource,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => apiKey,
                    log: NullLogger.Instance);

                // Assert
                IEnumerable<string> apiValues;
                IEnumerable<string> symbolClientVersionValues;
                IEnumerable<string> sourceClientVersionValues;
                symbolRequest.Headers.TryGetValues(ApiKeyHeader, out apiValues);
                symbolRequest.Headers.TryGetValues(NuGetClientVersionHeader, out symbolClientVersionValues);
                sourceRequest.Headers.TryGetValues(NuGetClientVersionHeader, out sourceClientVersionValues);

                Assert.Equal("tempkey", apiValues.First());
                Assert.NotNull(symbolClientVersionValues.First());
                Assert.NotNull(sourceClientVersionValues.First());
            }
        }
        
        [Fact]
        public async Task PackageUpdateResource_NuGetOrgSourceOnlyPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                var symbolSource = "https://other.smbsrc.net/";
                HttpRequestMessage sourceRequest = null;
                HttpRequestMessage symbolRequest = null;
                var apiKey = "serverapikey";

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");
                var symbolPackageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0", isSymbolPackage: true);

                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/",
                        request =>
                        {
                            sourceRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://other.smbsrc.net/api/v2/package/",
                        request =>
                        {
                            symbolRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://www.nuget.org/api/v2/package/create-verification-key/test/1.0.0",
                        request =>
                        {
                            var content = new StringContent(JsonData.tempApiKeyJsonData, Encoding.UTF8, "application/json");
                            var response = new HttpResponseMessage(HttpStatusCode.OK);
                            response.Content = content;
                            return Task.FromResult(response);
                        }
                    }

                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                UserAgent.SetUserAgentString(new UserAgentStringBuilder("test client"));

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: symbolSource,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => apiKey,
                    log: NullLogger.Instance);

                // Assert
                IEnumerable<string> apiValues;
                IEnumerable<string> symbolClientVersionValues;
                IEnumerable<string> sourceClientVersionValues;
                symbolRequest.Headers.TryGetValues(ApiKeyHeader, out apiValues);
                symbolRequest.Headers.TryGetValues(NuGetClientVersionHeader, out symbolClientVersionValues);
                sourceRequest.Headers.TryGetValues(NuGetClientVersionHeader, out sourceClientVersionValues);

                Assert.Equal("tempkey", apiValues.First());
                Assert.NotNull(symbolClientVersionValues.First());
                Assert.NotNull(sourceClientVersionValues.First());
            }
        }

        [Fact]
        public async Task PackageUpdateResource_SymbolSourceOnlyPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.myget.org/api/v2";
                var symbolSource = "https://nuget.smbsrc.net/";
                HttpRequestMessage sourceRequest = null;
                HttpRequestMessage symbolRequest = null;
                var apiKey = "serverapikey";

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");
                var symbolPackageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0", isSymbolPackage: true);

                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.myget.org/api/v2/",
                        request =>
                        {
                            sourceRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://nuget.smbsrc.net/api/v2/package/",
                        request =>
                        {
                            symbolRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://www.nuget.org/api/v2/package/create-verification-key/test/1.0.0",
                        request =>
                        {
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden));
                        }
                    }

                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                UserAgent.SetUserAgentString(new UserAgentStringBuilder("test client"));

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: symbolSource,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => apiKey,
                    log: NullLogger.Instance);

                // Assert
                IEnumerable<string> apiValues;
                IEnumerable<string> symbolClientVersionValues;
                IEnumerable<string> sourceClientVersionValues;
                symbolRequest.Headers.TryGetValues(ApiKeyHeader, out apiValues);
                symbolRequest.Headers.TryGetValues(NuGetClientVersionHeader, out symbolClientVersionValues);
                sourceRequest.Headers.TryGetValues(NuGetClientVersionHeader, out sourceClientVersionValues);

                Assert.Equal("ffffffff-0000-ffff-0000-ffffffffffff", apiValues.First());
                Assert.NotNull(symbolClientVersionValues.First());
                Assert.NotNull(sourceClientVersionValues.First());
            }
        }

        [Fact]
        public async Task PackageUpdateResource_NoSymbolSourcePushingSymbol()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://nuget.smbsrc.net/";

                HttpRequestMessage symbolRequest = null;
                var apiKey = "serverapikey";
                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");
                var symbolPackageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0", isSymbolPackage: true);

                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://nuget.smbsrc.net/api/v2/package/",
                        request =>
                        {
                            symbolRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://www.nuget.org/api/v2/package/create-verification-key/test/1.0.0",
                        request =>
                        {
                            var content = new StringContent(JsonData.tempApiKeyJsonData, Encoding.UTF8, "application/json");
                            var response = new HttpResponseMessage(HttpStatusCode.OK);
                            response.Content = content;
                            return Task.FromResult(response);
                        }
                    }

                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                UserAgent.SetUserAgentString(new UserAgentStringBuilder("test client"));

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: null,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => null,
                    log: NullLogger.Instance);

                // Assert
                IEnumerable<string> apiValues;
                IEnumerable<string> symbolClientVersionValues;
                symbolRequest.Headers.TryGetValues(ApiKeyHeader, out apiValues);
                symbolRequest.Headers.TryGetValues(NuGetClientVersionHeader, out symbolClientVersionValues);

                Assert.Equal("tempkey", apiValues.First());
                Assert.NotNull(symbolClientVersionValues.First());
            }
        }

        [Fact]
        public async Task PackageUpdateResource_PackageNotExistOnNuGetOrgPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://nuget.smbsrc.net/";

                HttpRequestMessage symbolRequest = null;
                var apiKey = "serverapikey";

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");
                var symbolPackageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0", isSymbolPackage: true);

                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://nuget.smbsrc.net/api/v2/package/",
                        request =>
                        {
                            symbolRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://www.nuget.org/api/v2/package/create-verification-key/test/1.0.0",
                        request =>
                        {
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                        }
                    }

                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                UserAgent.SetUserAgentString(new UserAgentStringBuilder("test client"));

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: null,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => null,
                    log: NullLogger.Instance);

                // Assert
                IEnumerable<string> apiValues;
                IEnumerable<string> symbolClientVersionValues;
                symbolRequest.Headers.TryGetValues(ApiKeyHeader, out apiValues);
                symbolRequest.Headers.TryGetValues(NuGetClientVersionHeader, out symbolClientVersionValues);

                Assert.Equal("ffffffff-0000-ffff-0000-ffffffffffff", apiValues.First());
                Assert.NotNull(symbolClientVersionValues.First());
            }
        }

        [Fact]
        public async Task PackageUpdateResource_SourceAndSymbolDifferentKeyPushing()
        {
            // Arrange
            using (var workingDir = TestDirectory.Create())
            {
                var source = "https://www.nuget.org/api/v2";
                var symbolSource = "https://other.smbsrc.net/";
                HttpRequestMessage sourceRequest = null;
                HttpRequestMessage symbolRequest = null;
                var apiKey = "serverapikey";
                var symbolKey = "symbolKey";

                var packageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0");
                var symbolPackageInfo = SimpleTestPackageUtility.CreateFullPackage(workingDir, "test", "1.0.0", isSymbolPackage: true);

                var responses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                {
                    {
                        "https://www.nuget.org/api/v2/",
                        request =>
                        {
                            sourceRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                    {
                        "https://other.smbsrc.net/api/v2/package/",
                        request =>
                        {
                            symbolRequest = request;
                            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                        }
                    },
                };

                var repo = StaticHttpHandler.CreateSource(source, Repository.Provider.GetCoreV3(), responses);
                var resource = await repo.GetResourceAsync<PackageUpdateResource>();
                UserAgent.SetUserAgentString(new UserAgentStringBuilder("test client"));

                // Act
                await resource.Push(
                    packagePath: packageInfo.FullName,
                    symbolSource: symbolSource,
                    timeoutInSecond: 5,
                    disableBuffering: false,
                    getApiKey: _ => apiKey,
                    getSymbolApiKey: _ => symbolKey,
                    log: NullLogger.Instance);

                // Assert
                IEnumerable<string> apiValues;
                IEnumerable<string> symbolClientVersionValues;
                IEnumerable<string> sourceClientVersionValues;
                symbolRequest.Headers.TryGetValues(ApiKeyHeader, out apiValues);
                symbolRequest.Headers.TryGetValues(NuGetClientVersionHeader, out symbolClientVersionValues);
                sourceRequest.Headers.TryGetValues(NuGetClientVersionHeader, out sourceClientVersionValues);

                Assert.Equal("symbolKey", apiValues.First());
                Assert.NotNull(symbolClientVersionValues.First());
                Assert.NotNull(sourceClientVersionValues.First());
            }
        }
    }
}
