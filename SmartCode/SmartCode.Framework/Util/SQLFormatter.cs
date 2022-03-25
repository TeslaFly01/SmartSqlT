using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCode.Helper
{
    public class SQLFormatter
    {
        static HashSet<string> BEGIN_CLAUSES = new HashSet<string>();
        static HashSet<string> END_CLAUSES = new HashSet<string>();
        static HashSet<string> LOGICAL = new HashSet<string>();
        static HashSet<string> QUANTIFIERS = new HashSet<string>();
        static HashSet<string> DML = new HashSet<string>();
        static HashSet<string> MISC = new HashSet<string>();

        static SQLFormatter()
        {
            BEGIN_CLAUSES.Add("left");
            BEGIN_CLAUSES.Add("right");
            BEGIN_CLAUSES.Add("inner");
            BEGIN_CLAUSES.Add("outer");
            BEGIN_CLAUSES.Add("group");
            BEGIN_CLAUSES.Add("order");

            END_CLAUSES.Add("where");
            END_CLAUSES.Add("set");
            END_CLAUSES.Add("having");
            END_CLAUSES.Add("from");
            END_CLAUSES.Add("by");
            END_CLAUSES.Add("join");
            END_CLAUSES.Add("into");
            END_CLAUSES.Add("union");

            LOGICAL.Add("and");
            LOGICAL.Add("or");
            LOGICAL.Add("when");
            LOGICAL.Add("else");
            LOGICAL.Add("end");

            QUANTIFIERS.Add("in");
            QUANTIFIERS.Add("all");
            QUANTIFIERS.Add("exists");
            QUANTIFIERS.Add("some");
            QUANTIFIERS.Add("any");

            DML.Add("insert");
            DML.Add("update");
            DML.Add("delete");

            MISC.Add("select");
            MISC.Add("%select");
            MISC.Add("on");
        }

        private static string INDENT_STRING = "    ";
        private string INITIAL = Environment.NewLine + INDENT_STRING;

        bool beginLine = true;
        bool afterBeginBeforeEnd;
        bool afterByOrSetOrFromOrSelect;
        bool afterOn;
        bool afterBetween;
        bool afterInsert;
        int inFunction;
        int parensSinceSelect;
        private LinkedList<int> parenCounts = new LinkedList<int>();
        private LinkedList<bool> afterByOrFromOrSelects = new LinkedList<bool>();

        int indent = 1;

        StringBuilder result = new StringBuilder();
        Queue<string> tokens;
        string lastToken;
        string token;
        string lcToken;

        public IEnumerable<string> SplitAndKeep(string s, char[] delims)
        {
            int start = 0, index;

            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                    yield return s.Substring(start, index - start);
                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }

        public SQLFormatter(string sql)
        {
            tokens = new Queue<string>(SplitAndKeep(sql, new char[] { '(', ')', '+', '*', '/', '-', '=', '<', '>', '\'', '`', '\\', '"', '[', ']', ',', ' ', '\n', '\r', '\f', '\t' }));
        }

        private bool IsWhitespace(string token)
        {
            return " \n\r\f\t".Contains(token);
        }

        public string Format()
        {
            result.Append(INITIAL);

            while (tokens.Count > 0)
            {
                token = tokens.Dequeue();
                lcToken = token.ToLower();

                if ("'".Equals(token))
                {
                    string t;
                    do
                    {
                        t = tokens.Dequeue();
                        token += t;
                    } while ("'".Equals(t) == false && tokens.Count > 0);
                }
                else if ("\"".Equals(token))
                {
                    string t;
                    do
                    {
                        t = tokens.Dequeue();
                        token += t;
                    } while ("\"".Equals(t) == false && tokens.Count > 0);
                }
                else if ("[".Equals(token))
                {
                    String t;
                    do
                    {
                        t = tokens.Dequeue();
                        token += t;
                    }
                    while (!"]".Equals(t) && tokens.Count > 0);
                }

                if (afterByOrSetOrFromOrSelect && ",".Equals(token))
                {
                    CommaAfterByOrFromOrSelect();
                }
                else if (afterOn && ",".Equals(token))
                {
                    CommaAfterOn();
                }
                else if ("(".Equals(token))
                {
                    OpenParen();
                }
                else if (")".Equals(token))
                {
                    CloseParen();
                }
                else if (BEGIN_CLAUSES.Contains(lcToken))
                {
                    BeginNewClause();
                }
                else if (END_CLAUSES.Contains(lcToken))
                {
                    EndNewClause();
                }
                else if ("select".Equals(lcToken))
                {
                    Select();
                }
                else if (DML.Contains(lcToken))
                {
                    UpdateOrInsertOrDelete();
                }
                else if ("values".Equals(lcToken))
                {
                    Values();
                }
                else if ("on".Equals(lcToken))
                {
                    On();
                }
                else if (afterBetween && lcToken.Equals("and"))
                {
                    Misc();
                    afterBetween = false;
                }
                else if (LOGICAL.Contains(lcToken))
                {
                    Logical();
                }
                else if (IsWhitespace(token))
                {
                    White();
                }
                else
                {
                    Misc();
                }

                if (IsWhitespace(token) == false)
                {
                    lastToken = lcToken;
                }

            }
            return result.ToString();
        }

        private void White()
        {
            if (beginLine == false)
            {
                result.Append(" ");
            }
        }

        private void Logical()
        {
            if ("end".Equals(lcToken))
            {
                indent--;
            }
            Newline();
            Out();
            beginLine = false;
        }

        private void Misc()
        {
            Out();
            if ("between".Equals(lcToken))
            {
                afterBetween = true;
            }
            if (afterInsert)
            {
                Newline();
                afterInsert = false;
            }
            else
            {
                beginLine = false;
                if ("case".Equals(lcToken))
                {
                    indent++;
                }
            }
        }

        private void On()
        {
            indent++;
            afterOn = true;
            Newline();
            Out();
            beginLine = false;
        }

        private void Values()
        {
            indent--;
            Newline();
            Out();
            indent++;
            Newline();
        }

        private void UpdateOrInsertOrDelete()
        {
            Out();
            indent++;
            beginLine = false;
            if ("update".Equals(lcToken))
            {
                Newline();
            }
            if ("insert".Equals(lcToken))
            {
                afterInsert = true;
            }
        }

        private void Select()
        {
            //Newline();
            Out();
            indent++;
            Newline();
            parenCounts.AddLast(parensSinceSelect);
            afterByOrFromOrSelects.AddLast(afterByOrSetOrFromOrSelect);
            parensSinceSelect = 0;
            afterByOrSetOrFromOrSelect = true;
        }

        private void EndNewClause()
        {
            if (!afterBeginBeforeEnd)
            {
                indent--;
                if (afterOn)
                {
                    indent--;
                    afterOn = false;
                }
                Newline();
            }
            Out();
            if (!"union".Equals(lcToken))
            {
                indent++;
            }
            Newline();
            afterBeginBeforeEnd = false;
            afterByOrSetOrFromOrSelect = "by".Equals(lcToken)
                    || "set".Equals(lcToken)
                    || "from".Equals(lcToken);
        }

        private void BeginNewClause()
        {
            if (!afterBeginBeforeEnd)
            {
                if (afterOn)
                {
                    indent--;
                    afterOn = false;
                }
                indent--;
                Newline();
            }
            Out();
            beginLine = false;
            afterBeginBeforeEnd = true;
        }

        private void CloseParen()
        {
            parensSinceSelect--;
            if (parensSinceSelect < 0)
            {
                indent--;
                parensSinceSelect = parenCounts.Last.Value;
                parenCounts.RemoveLast();
                afterByOrSetOrFromOrSelect = afterByOrFromOrSelects.Last.Value;
                afterByOrFromOrSelects.RemoveLast();
            }
            if (inFunction > 0)
            {
                inFunction--;
                Out();
            }
            else
            {
                if (!afterByOrSetOrFromOrSelect)
                {
                    indent--;
                    Newline();
                }
                Out();
            }
            beginLine = false;
        }

        private void OpenParen()
        {
            if (isFunctionName(lastToken) || inFunction > 0)
            {
                inFunction++;
            }
            beginLine = false;
            if (inFunction > 0)
            {
                Out();
            }
            else
            {
                Out();
                if (!afterByOrSetOrFromOrSelect)
                {
                    indent++;
                    Newline();
                    beginLine = true;
                }
            }
            parensSinceSelect++;
        }

        private bool isFunctionName(string tok)
        {
            if (tok == null || tok.Length == 0)
            {
                return false;
            }

            char begin = tok[0];

            bool isIdentifier = (Char.IsLetter(begin) || begin == '$' || begin == '_') || '"' == begin;
            return isIdentifier &&
                    LOGICAL.Contains(tok) == false &&
                    END_CLAUSES.Contains(tok) == false &&
                    QUANTIFIERS.Contains(tok) == false &&
                    DML.Contains(tok) == false &&
                    MISC.Contains(tok) == false;
        }

        private void CommaAfterOn()
        {
            Out();
            indent--;
            Newline();
            afterOn = false;
            afterByOrSetOrFromOrSelect = true;
        }

        private void CommaAfterByOrFromOrSelect()
        {
            Out();
            Newline();
        }

        private void Newline()
        {
            result.Append(Environment.NewLine);
            for (int i = 0; i < indent; i++)
            {
                result.Append(INDENT_STRING);
            }
            beginLine = true;
        }

        private void Out()
        {
            result.Append(token);
        }
    }
}
