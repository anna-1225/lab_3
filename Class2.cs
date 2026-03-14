using System;
using System.Collections.Generic;
using System.Text;

namespace new2026
{
    public class Token
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public bool IsError { get; set; }
    }

    public class Scanner
    {
        private const string _letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
        private const string _digits = "0123456789";
        private const string _operators = "=<>!+-*/";
        private const string _delimiters = ";,.(){}[] ";

        private readonly string[] _keywords = new string[] { "if", "else" };

        private bool ContainsChar(string str, char c)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                    return true;
            }
            return false;
        }

        private bool IsKeyword(string word)
        {
            for (int i = 0; i < _keywords.Length; i++)
            {
                if (_keywords[i] == word)
                    return true;
            }
            return false;
        }

        public List<Token> Analyze(string text)
        {
            var tokens = new List<Token>();
            int line = 1;
            int pos = 0;

            while (pos < text.Length)
            {
                char c = text[pos];

                if (c == '\n')
                {
                    line++;
                    pos++;
                    continue;
                }

                int startPos = pos + 1;

                if (ContainsChar(_digits, c))
                {
                    string num = "";
                    while (pos < text.Length)
                    {
                        char current = text[pos];
                        if (ContainsChar(_digits, current) || current == '.')
                        {
                            num += current;
                            pos++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    tokens.Add(new Token
                    {
                        Code = 3,
                        Type = "Число",
                        Value = num,
                        Line = line,
                        Position = startPos
                    });
                }

                else if (ContainsChar(_letters, c))
                {
                    string word = "";
                    while (pos < text.Length)
                    {
                        char current = text[pos];
                        if (ContainsChar(_letters, current) || ContainsChar(_digits, current))
                        {
                            word += current;
                            pos++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (IsKeyword(word))
                        tokens.Add(new Token { Code = 1, Type = "Ключевое слово", Value = word, Line = line, Position = startPos });
                    else
                        tokens.Add(new Token { Code = 2, Type = "Идентификатор", Value = word, Line = line, Position = startPos });
                }

                else if (ContainsChar(_operators, c))
                {
                    if (pos + 1 < text.Length && ContainsChar(_operators, text[pos + 1]) &&
                        (c == '=' || c == '>' || c == '<' || c == '!'))
                    {
                        string op = c.ToString() + text[pos + 1];
                        tokens.Add(new Token { Code = 4, Type = "Оператор", Value = op, Line = line, Position = startPos });
                        pos += 2;
                    }
                    else
                    {
                        tokens.Add(new Token { Code = 4, Type = "Оператор", Value = c.ToString(), Line = line, Position = startPos });
                        pos++;
                    }
                }

                else if (ContainsChar(_delimiters, c))
                {
                    tokens.Add(new Token { Code = 5, Type = "Разделитель", Value = c.ToString(), Line = line, Position = startPos });
                    pos++;
                }

                else
                {
                    tokens.Add(new Token
                    {
                        Code = 99,
                        Type = "ОШИБКА",
                        Value = c.ToString(),
                        Line = line,
                        Position = startPos,
                        IsError = true
                    });
                    pos++;
                }
            }

            return tokens;
        }
    }
}