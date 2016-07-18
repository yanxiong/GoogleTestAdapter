using GoogleTestAdapter.Model;

namespace GoogleTestAdapter.Helpers
{
    public interface ISourceFileFinder
    {
        string Find(string file, TestCase testCase);
        string Find(string file, string scopedTrace);
    }
}