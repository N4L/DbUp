using System;
using System.Text.RegularExpressions;

namespace DbUp.Helpers
{
    class InMemorySourceFile
    {
        public bool IsMigration
        {
            get
            {
                return this.GetIsMigration();
            }
        }

        public string FileName
        {
            get;
            private set;
        }

        public string SourceCode
        {
            get;
            private set;
        }

        public string TypeName
        {
            get
            {
                return this.GetTypeName();
            }
        }

        public InMemorySourceFile(string fileName, string source)
        {
            this.FileName = fileName;
            this.SourceCode = source;
        }

        private bool GetIsMigration()
        {
            return (new Regex("\\w*Migration")).Match(this.SourceCode).Success;
        }

        private string GetTypeName()
        {
            string str;
            string str1;
            Regex regex;
            Regex regex1;
                str = "${class}";
                str1 = "${namespace}";
                regex = new Regex("class (?<class>\\w*)");
                regex1 = new Regex("namespace (?<namespace>[a-zA-Z0-9.-[{]]*)");

            string empty = string.Empty;
            Match match = regex1.Match(this.SourceCode);
            Match match1 = regex.Match(this.SourceCode);
            if (match1.Success && match.Success)
            {
                object[] objArray = new object[] { match.Result(str1), match1.Result(str) };
                empty = string.Format("{0}.{1}", objArray);
            }
            return empty;
        }
    }
}
