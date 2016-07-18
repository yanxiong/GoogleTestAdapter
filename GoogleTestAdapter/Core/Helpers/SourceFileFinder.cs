using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GoogleTestAdapter.Model;

namespace GoogleTestAdapter.Helpers
{
    public class SourceFileFinder : ISourceFileFinder
    {
        private static readonly Regex TestNameRegex = new Regex(@"(.*)\.(.*)(?:/.*)?");
        private static readonly Regex TestRegex = new Regex(@"(?:TYPED_)?TEST(?:|_F|_P)(?:_TRAITS)?\(\s*(\w+)\s*,\s*(\w+)(?:\s*\w+\s*,\s*\w+\s*)*\s*\)");
        private static readonly Regex TracesRegex = new Regex(@"SCOPED_TRACE\(\s*""(.*)""\s*\)");

        private readonly string _baseDir;
        private readonly IDictionary<string, string[]> _fileToAbsoluteFiles = new Dictionary<string, string[]>();
        private readonly IDictionary<string, ISet<string>> _absoluteFileToTestCases = new Dictionary<string, ISet<string>>();
        private readonly IDictionary<string, ISet<string>> _absoluteFileToScopedTraces = new Dictionary<string, ISet<string>>();

        public SourceFileFinder(string baseDir)
        {
            _baseDir = baseDir;
        }

        public string Find(string file, TestCase testCase)
        {
            return Find(file, f => _absoluteFileToTestCases[f].Contains(GetTestCaseName(testCase)));
        }

        public string Find(string file, string scopedTrace)
        {
            return Find(file, f => _absoluteFileToScopedTraces[f].Contains(scopedTrace));
        }

        private string Find(string file, Func<string, bool> isMatchingAbsoluteFile)
        {
            if (Path.IsPathRooted(file))
                return file;

            if (!_fileToAbsoluteFiles.ContainsKey(file))
                CollectTestAndTracesData(file);

            string[] absoluteFiles = _fileToAbsoluteFiles[file].ToArray();
            if (absoluteFiles.Length == 1)
                return absoluteFiles.Single();

            string[] matchingAbsoluteFiles = absoluteFiles.Where(isMatchingAbsoluteFile).ToArray();
            if (matchingAbsoluteFiles.Length == 1)
                return matchingAbsoluteFiles.Single();

            return file;
        }

        private void CollectTestAndTracesData(string file)
        {
            string[] files = Directory.GetFiles(_baseDir, file, SearchOption.AllDirectories);
            string[] absoluteFiles = files.Select(f => Path.IsPathRooted(f) ? f : Path.GetFullPath(f)).ToArray();
            _fileToAbsoluteFiles.Add(file, absoluteFiles);

            if (absoluteFiles.Length <= 1)
                return;

            foreach (string absoluteFile in absoluteFiles)
            {
                string fileContent = File.ReadAllText(absoluteFile);

                ISet<string> tests = new HashSet<string>();
                MatchCollection testMatches = TestRegex.Matches(fileContent);
                foreach (Match testMatch in testMatches)
                {
                    tests.Add($"{testMatch.Groups[1].Value}.{testMatch.Groups[2].Value}");
                }

                ISet<string> traces = new HashSet<string>();
                MatchCollection traceMatches = TracesRegex.Matches(fileContent);
                foreach (Match traceMatch in traceMatches)
                {
                    traces.Add(traceMatch.Groups[1].Value);
                }

                _absoluteFileToTestCases.Add(absoluteFile, tests);
                _absoluteFileToScopedTraces.Add(absoluteFile, traces);
            }
        }

        private string GetTestCaseName(TestCase testCase)
        {
            Match match = TestNameRegex.Match(testCase.DisplayName);
            return $"{match.Groups[1].Value}.{match.Groups[2].Value}";
        }

    }

}