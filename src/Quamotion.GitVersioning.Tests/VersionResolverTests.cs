﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nerdbank.GitVersioning;
using Quamotion.GitVersioning.Git;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Quamotion.GitVersioning.Tests
{
    public class VersionResolverTests
    {
        private readonly ITestOutputHelper output;

        public VersionResolverTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Theory]
        [InlineData("xunit", "version.json", "0.1.0-pre.98")] // https://github.com/xunit/xunit
        [InlineData("xunit2", "version.json", "0.1.0-pre.98")] // https://github.com/xunit/xunit, shared clone
        [InlineData("SuperSocket", "version.json", "2.0.0-beta7")] // https://github.com/kerryjiang/SuperSocket
        [InlineData("Cuemon", "version.json", "6.0.0-preview.10")] // https://github.com/gimlichael/Cuemon
        [InlineData("NerdBank.GitVersioning", "version.json", "3.3.22-alpha")] // https://github.com/dotnet/nerdbank.GitVersioning/
        [InlineData("WebDriver", "src/version.json", "0.149.20")] // internal repository
        public void GetVersionTest(string repositoryName, string versionPath, string expectedVersion)
        {
            string path =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    @"Source\Repos",
                    repositoryName);


            const int count = 100;

            ILogger<VersionResolver> logger = count == 1 ? output.BuildLoggerFor<VersionResolver>() : NullLogger<VersionResolver>.Instance;

            for (int i = 0; i < count; i++)
            {
                GitRepository repository = new GitRepository(path);
                VersionResolver resolver = new WalkingVersionResolver(repository, versionPath, logger);

                var version = resolver.GetVersion();
                Assert.Equal(expectedVersion, version);

                if (count == 1)
                {
                    var stats = repository.GetCacheStatistics();
                }
            }
        }

        [Theory]
        [InlineData("xunit", "version.json", "0.1.0-pre.98")] // https://github.com/xunit/xunit
        [InlineData("SuperSocket", "version.json", "2.0.0-beta7")] // https://github.com/kerryjiang/SuperSocket
        [InlineData("Cuemon", "version.json", "6.0.0-preview.10")] // https://github.com/gimlichael/Cuemon
        [InlineData("NerdBank.GitVersioning", "version.json", "3.3.22-alpha")] // https://github.com/dotnet/nerdbank.GitVersioning/
        [InlineData("WebDriver", "src/version.json", "0.149.20")] // internal repository
        public void GetNbgvVersionTest(string repositoryName, string versionPath, string expectedVersion)
        {
            string path =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    @"Source\Repos",
                    repositoryName);

            var oracleA = VersionOracle.Create(Path.Combine(path, Path.GetDirectoryName(versionPath)));
            var version = oracleA.CloudBuildNumber;

            Assert.Equal(expectedVersion, version);
        }
    }
}
