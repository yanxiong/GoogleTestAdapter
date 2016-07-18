using GoogleTestAdapter.Model;

namespace GoogleTestAdapter.Helpers
{
    public class FakeSourceFileFinder : ISourceFileFinder
    {
        public string Find(string file, TestCase testCase)
        {
            return file;
        }
        public string Find(string file, string scopedTrace)
        {
            return file;
        }
    }
}