using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Commitments
{
    public class CommitMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string _types = string.Empty;
        private bool _isBreaking = false;
        private string _header = string.Empty;
        private string _body = string.Empty;
        private string _footer = string.Empty;

        public string Types
        {
            get
            {
                return _types;
            }
            set
            {
                _types = value;
                OnPropertyChanged();
            }
        }
        public bool IsBreaking
        {
            get
            {
                return _isBreaking;
            }
            set
            {
                _isBreaking = value;
                OnPropertyChanged();
            }
        }
        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }
        public string Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;
                OnPropertyChanged();
            }
        }
        public string Footer
        {
            get
            {
                return _footer;
            }
            set
            {
                _footer = value;
                OnPropertyChanged();
            }
        }

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
                var parts = new List<string>();
                if (FullHeader.Length > 0)
                {
                    parts.Add(FullHeader);
                }
                if (Body.Length > 0)
                {
                    parts.Add(Body);
                }
                if (Footer.Length > 0)
                {
                    parts.Add(Footer);
                }
                return string.Join($"{Environment.NewLine}{Environment.NewLine}", parts);
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
