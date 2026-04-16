using System;
using System.Collections.Generic;

class ParseError
{
    public string Fragment { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; }
    public int Index { get; set; }
    public int Length { get; set; }
}

class Parser
{
    private string text;
    private int pos;
    private int line = 1;
    private int column = 1;

    public List<ParseError> Errors = new List<ParseError>();

    public Parser(string input)
    {
        text = input;
        pos = 0;
    }

    private char Current => pos < text.Length ? text[pos] : '\0';

    private void Next()
    {
        if (Current == '\n')
        {
            line++;
            column = 1;
        }
        else column++;

        pos++;
    }

    private void SkipSpaces()
    {
        while (char.IsWhiteSpace(Current))
            Next();
    }

    public void Parse()
    {
        Statement();

        if (Current != '\0')
            AddError(Current.ToString(), "Лишние символы в конце");
    }

    // stmt ::= id = expr ;
    private void Statement()
    {
        SkipSpaces();

        Identifier();

        SkipSpaces();
        Expect("=");

        SkipSpaces();
        Expression();

        SkipSpaces();
        Expect(";");
    }

    // expr ::= id if condition else id
    private void Expression()
    {
        Identifier();

        SkipSpaces();
        ExpectWord("if");

        SkipSpaces();
        Condition();

        SkipSpaces();
        ExpectWord("else");

        SkipSpaces();
        Identifier();
    }

    // condition ::= id op id
    private void Condition()
    {
        Identifier();

        SkipSpaces();
        Operator();

        SkipSpaces();
        Identifier();
    }
    private string ReadInvalidSequence()
    {
        int start = pos;

        while (!char.IsLetterOrDigit(Current) &&
               !char.IsWhiteSpace(Current) &&
               Current != ';' &&
               Current != '\0')
        {
            Next();
        }

        return text.Substring(start, pos - start);
    }

    private void Identifier()
    {
        if (!char.IsLetter(Current))
        {
            string wrong = ReadInvalidSequence();

            if (string.IsNullOrEmpty(wrong))
                wrong = Current.ToString();

            AddError(wrong, "Ожидался идентификатор");
            return;
        }

        while (char.IsLetterOrDigit(Current))
            Next();
    }

    private void Operator()
    {
        if (Current == '>' || Current == '<')
        {
            Next();
        }
        else if (Current == '=' && Peek() == '=')
        {
            Next(); Next();
        }
        else if (Current == '!' && Peek() == '=')
        {
            Next(); Next();
        }
        else
        {
            string wrong = ReadInvalidSequence();
            AddError(wrong, "Ожидался оператор сравнения");
        }
    }

    private char Peek()
    {
        return pos + 1 < text.Length ? text[pos + 1] : '\0';
    }

    private void Expect(string symbol)
    {
        SkipSpaces();

        if (symbol == "=")
        {
            if (Current == '=')
            {
                int start = pos;

                // 🔥 съедаем ВСЕ подряд '='
                while (Current == '=')
                    Next();

                int count = pos - start;

                // если больше одного '=' → ошибка
                if (count > 1)
                {
                    string wrong = text.Substring(start, count);
                    AddError(wrong, "Лишние символы '='");
                }

                return;
            }
            else
            {
                string wrong = ReadInvalidSequence();
                AddError(wrong, $"Ожидалось '{symbol}'");
                return;
            }
        }

        // обычная логика
        foreach (char c in symbol)
        {
            if (Current == c)
                Next();
            else
            {
                AddError(Current.ToString(), $"Ожидалось '{symbol}'");
                return;
            }
        }
    }

    private void ExpectWord(string word)
    {
        SkipSpaces();

        int start = pos;

        foreach (char c in word)
        {
            if (Current == c)
                Next();
            else
            {
                while (char.IsLetterOrDigit(Current))
                    Next();

                string wrong = ReadInvalidSequence();
                AddError(wrong, $"Ожидалось '{word}'");

                AddError(wrong, $"Ожидалось '{word}'");
                return;
            }
        }

        if (char.IsLetterOrDigit(Current))
        {
            while (char.IsLetterOrDigit(Current))
                Next();

            string wrong = text.Substring(start, pos - start);

            AddError(wrong, $"Ожидалось '{word}'");
        }
    }


    private void AddError(string fragment, string message)
    {
        if (string.IsNullOrEmpty(fragment) || fragment == "\0")
            fragment = "EOF";

        Errors.Add(new ParseError
        {
            Fragment = fragment,
            Line = line,
            Column = column,
            Index = pos,             
            Length = fragment.Length, 
            Message = message
        });
    }
}