using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using GoogleTestAdapter.Helpers;
using GoogleTestAdapter.Model;

namespace GoogleTestAdapter.TestResults
{

    public class ErrorMessageParser
    {
        private static readonly string ValidCharRegex;

        static ErrorMessageParser()
        {
            IEnumerable<char> invalidChars =
                Path.GetInvalidFileNameChars().Where(c => Path.GetInvalidPathChars().Contains(c));
            ValidCharRegex = "[^" + Regex.Escape(new string(invalidChars.ToArray())) + "]";
        }

        public string ErrorMessage { get; private set; }
        public string ErrorStackTrace { get; private set; }

        private readonly TestCase _testCase;
        private readonly ISourceFileFinder _fileFinder;

        private readonly Regex _splitRegex;
        private readonly Regex _parseRegex;
        private readonly Regex _scopedTraceStartRegex;
        private readonly Regex _scopedTraceRegex;

        private readonly IList<string> _errorMessages;

        public ErrorMessageParser(string consoleOutput, string baseDir, TestCase testCase, ISourceFileFinder finder) 
            : this(baseDir, testCase, finder)
        {
            _errorMessages = SplitConsoleOutput(consoleOutput);
        }

        public ErrorMessageParser(XmlNodeList failureNodes, string baseDir, TestCase testCase, ISourceFileFinder finder) 
            : this(baseDir, testCase, finder)
        {
            _errorMessages = (from XmlNode failureNode in failureNodes select failureNode.InnerText).ToList();
        }

        private ErrorMessageParser(string baseDir, TestCase testCase, ISourceFileFinder finder)
        {
            string escapedBaseDir = Regex.Escape(baseDir ?? "");
            string file = $"((?:{escapedBaseDir})?{ValidCharRegex}*)";
            string line = "([0-9]+)";
            string fileAndLine = $@"{file}((:{line})|(\({line}\):))";
            string error = @"((error: )|(Failure\n))";

            _testCase = testCase;
            _fileFinder = finder;

            _parseRegex = new Regex($"{fileAndLine}(:? {error})?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _splitRegex = new Regex($"{fileAndLine}:? {error}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _scopedTraceStartRegex = new Regex(@"Google Test trace:\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _scopedTraceRegex = new Regex($@"{file}\({line}\): (.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public void Parse()
        {
            switch (_errorMessages.Count)
            {
                case 0:
                    ErrorMessage = "";
                    ErrorStackTrace = "";
                    break;
                case 1:
                    HandleSingleFailure();
                    break;
                default:
                    HandleMultipleFailures();
                    break;
            }
        }

        public static string CreateStackTraceEntry(string label, string fullFileName, string lineNumber)
        {
            return $"at {label} in {fullFileName.ToLower()}:line {lineNumber}{Environment.NewLine}";
        }

        private IList<string> SplitConsoleOutput(string errorMessage)
        {
            MatchCollection matches = _splitRegex.Matches(errorMessage);
            if (matches.Count == 0)
                return new List<string>();

            var errorMessages = new List<string>();
            int startIndex, length;
            for (int i = 0; i < matches.Count - 1; i++)
            {
                startIndex = matches[i].Index;
                length = matches[i + 1].Index - startIndex;
                errorMessages.Add(errorMessage.Substring(startIndex, length));
            }
            startIndex = matches[matches.Count - 1].Index;
            length = errorMessage.Length - startIndex;
            errorMessages.Add(errorMessage.Substring(startIndex, length));

            return errorMessages;
        }

        private void HandleSingleFailure()
        {
            string errorMessage = _errorMessages[0];
            string stackTrace;
            CreateErrorMessageAndStacktrace(ref errorMessage, out stackTrace);

            ErrorMessage = errorMessage;
            ErrorStackTrace = stackTrace;
        }

        private void HandleMultipleFailures()
        {
            var finalErrorMessages = new List<string>();
            var finalStackTraces = new List<string>();
            for (int i = 0; i < _errorMessages.Count; i++)
            {
                string errorMessage = _errorMessages[i];
                int msgId = i + 1;
                string stackTrace;
                CreateErrorMessageAndStacktrace(ref errorMessage, out stackTrace, msgId);

                finalErrorMessages.Add($"#{msgId} - {errorMessage}");
                finalStackTraces.Add(stackTrace);
            }

            ErrorMessage = string.Join("\n", finalErrorMessages);
            ErrorStackTrace = string.Join("", finalStackTraces);
        }

        private void CreateErrorMessageAndStacktrace(ref string errorMessage, out string stackTrace, int msgId = 0)
        {
            Match match = _parseRegex.Match(errorMessage);
            if (!match.Success)
            {
                stackTrace = "";
                return;
            }

            string fullFileName = _fileFinder.Find(match.Groups[1].Value, _testCase);
            string fileName = Path.GetFileName(fullFileName);
            string lineNumber = match.Groups[4]. Value;
            if (string.IsNullOrEmpty(lineNumber))
                lineNumber = match.Groups[6].Value;

            string msgReference = msgId == 0 ? "" : $"#{msgId} - ";

            stackTrace = CreateStackTraceEntry($"{msgReference}{fileName}:{lineNumber}", fullFileName, lineNumber);
            errorMessage = errorMessage.Replace(match.Value, "").Trim();

            match = _scopedTraceStartRegex.Match(errorMessage);
            if (match.Success)
            {
                string scopedTraces = errorMessage.Substring(match.Index + match.Value.Length);
                errorMessage = errorMessage.Substring(0, match.Index).Trim();
                MatchCollection matches = _scopedTraceRegex.Matches(scopedTraces);
                foreach (Match traceMatch in matches)
                {
                    string traceMessage = traceMatch.Groups[3].Value.Trim();
                    fullFileName = _fileFinder.Find(traceMatch.Groups[1].Value, traceMessage);
                    lineNumber = traceMatch.Groups[2].Value;

                    stackTrace += CreateStackTraceEntry($"-->{traceMessage}", fullFileName, lineNumber);
                }
            }
        }

    }

}