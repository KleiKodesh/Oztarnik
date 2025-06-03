using Otzarnik.FsViewer;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Otzarnik.Search
{
    public class ResultCollection
    {
        public string Content { get; set; }
        public IEnumerable<ResultModel> Results { get; set; }
    }

    public class ResultModel
    {
        public string Pre { get; set; }
        public string Post { get; set; }
        public string MatchValue { get; set; }
        public TreeItem TreeItem { get; set; }
        public int MatchIndex { get; set; }
        public Match Match { get; set; }
    }
}
