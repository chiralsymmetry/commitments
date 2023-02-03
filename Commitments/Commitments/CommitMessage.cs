using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commitments
{
    public class CommitMessage
    {
        public string Types { get; set; } = string.Empty;
        public bool IsBreaking { get; set; } = false;
        public string Header { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;

        public string FullHeader
        {
            get
            {
                if (Types.Length > 0)
                {
                    string breakingMark = string.Empty;
                    if (IsBreaking)
                    {
                        breakingMark = "!";
                    }
                    return $"{Types}{breakingMark}: {Header}";
                }
                return Header;
            }
        }

        public string Message
        {
            get
            {
                return string.Join($"{Environment.NewLine}{Environment.NewLine}", new string[] { FullHeader, Body, Footer });
            }
        }

        public int BodyWidth
        {
            get
            {
                return MaxWidth(Body);
            }
        }

        public int FooterWidth
        {
            get
            {
                return MaxWidth(Footer);
            }
        }

        private static int MaxWidth(string s)
        {
            int maxWidth = 0;
            s = s.ReplaceLineEndings();
            foreach (var line in s.Split(Environment.NewLine))
            {
                maxWidth = Math.Max(maxWidth, line.Length);
            }
            return maxWidth;
        }
    }
}
