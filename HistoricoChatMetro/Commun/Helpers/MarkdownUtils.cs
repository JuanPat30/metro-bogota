using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commun.Helpers
{
    public static class MarkdownUtils
    {
        public static string ToHtml(string markdownText)
        {
            return Markdown.ToHtml(markdownText);
        }
    }
}
