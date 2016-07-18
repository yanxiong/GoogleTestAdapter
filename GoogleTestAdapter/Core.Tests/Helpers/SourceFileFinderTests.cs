using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using GoogleTestAdapter.DiaResolver;
using GoogleTestAdapter.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GoogleTestAdapter.TestMetadata.TestCategories;

namespace GoogleTestAdapter.Helpers
{

    [TestClass]
    public class SourceFileFinderTests : AbstractCoreTests
    {
        private ISourceFileFinder _sourceFileFinder;

        [TestInitialize]
        public void Setup()
        {
            _sourceFileFinder = new SourceFileFinder(TestResources.SampleTestsSolutionDir);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void Find_SingleFile_FileIsFoundViaTestCase()
        {
            var discoverer = new GoogleTestDiscoverer(TestEnvironment, new DefaultDiaResolverFactory());
            IList<TestCase> testCases = discoverer.GetTestsFromExecutable(TestResources.HardCrashingSampleTests);
            TestCase testCase = testCases.First();
            string file = Path.GetFileName(testCase.CodeFilePath);

            string result = _sourceFileFinder.Find(file, testCase).ToLower();

            result.Should().Be(testCase.CodeFilePath);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void Find_TwoFiles_CorrectFilesAreFoundViaTestCase()
        {
            var discoverer = new GoogleTestDiscoverer(TestEnvironment, new DefaultDiaResolverFactory());
            IList<TestCase> testCases = discoverer.GetTestsFromExecutable(TestResources.SampleTests);
            TestCase testCase = testCases.Single(tc => tc.DisplayName == "TestMath.AddFails");
            string file = Path.GetFileName(testCase.CodeFilePath);

            string result = _sourceFileFinder.Find(file, testCase).ToLower();

            result.Should().Be(testCase.CodeFilePath);

            _sourceFileFinder = new SourceFileFinder(TestResources.SampleTestsSolutionDir);
            testCases = discoverer.GetTestsFromExecutable(TestResources.HardCrashingSampleTests);
            testCase = testCases.Single(tc => tc.DisplayName == "Crashing.LongRunning");
            Path.GetFileName(testCase.CodeFilePath).Should().Be(file);

            result = _sourceFileFinder.Find(file, testCase).ToLower();

            result.Should().Be(testCase.CodeFilePath);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void Find_SingleFile_FileIsFoundViaScopedTrace()
        {
            var discoverer = new GoogleTestDiscoverer(TestEnvironment, new DefaultDiaResolverFactory());
            IList<TestCase> testCases = discoverer.GetTestsFromExecutable(TestResources.SampleTests);
            TestCase testCase = testCases.Single(tc => tc.DisplayName == "MessageParserTests.ScopedTraceInTestMethodANdHelperMethod");
            string file = Path.GetFileName(testCase.CodeFilePath);

            string result = _sourceFileFinder.Find(file, "HelperMethod").ToLower();

            result.Should().Be(testCase.CodeFilePath);
        }

        [TestMethod]
        [TestCategory(Unit)]
        public void Find_NotExistingFile_InputIsReturned()
        {
            var discoverer = new GoogleTestDiscoverer(TestEnvironment, new DefaultDiaResolverFactory());
            IList<TestCase> testCases = discoverer.GetTestsFromExecutable(TestResources.HardCrashingSampleTests);
            TestCase testCase = testCases.First();
            string file = "whatever.file";

            string result = _sourceFileFinder.Find(file, testCase).ToLower();

            result.Should().Be(file);
        }

    }

}