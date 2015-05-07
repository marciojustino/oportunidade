
using System.Xml.Linq;

namespace MinutoSegurosApp
{
    public class Feed
    {
        public XElement Title { get; set; }
        public XElement Link { get; set; }
        public XElement Comments { get; set; }
        public XElement Date { get; set; }
        public XElement Creator { get; set; }
        public XElement Category { get; set; }
        public XElement Guid { get; set; }
        public XElement Description { get; set; }
        public XElement Content { get; set; }
        public int ContentWordsCount { get; set; }
        public int DescriptionWordsCount { get; set; }

    }
}
