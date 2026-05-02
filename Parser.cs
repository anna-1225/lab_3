using System;
using System.Collections.Generic;
using System.Linq;

public class Parser
{
    public class SyntaxError
    {
        public string Fragment { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }
    }

    private List<Lexer.Token> _tokens;
    private int _position;
    public List<SyntaxError> Errors { get; private set; }

    public Parser(List<Lexer.Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
        Errors = new List<SyntaxError>();
    }

    private Lexer.Token Current => _position < _tokens.Count ? _tokens[_position] : null;
    private Lexer.Token PeekNext => _position + 1 < _tokens.Count ? _tokens[_position + 1] : null;
    private bool IsAtEnd => Current == null;

    private void Advance()
    {
        if (Current != null)
            _position++;
    }

    private bool Check(Lexer.TokenType type, string value = null)
    {
        if (IsAtEnd) return false;
        if (Current.Type != type) return false;
        if (value != null && Current.Value != value) return false;
        return true;
    }

    private bool Match(Lexer.TokenType type, string value = null)
    {
        if (Check(type, value))
        {
            Advance();
            return true;
        }
        return false;
    }

    private void AddError(string message, Lexer.Token found)
    {
        if (Errors.Count > 0 && found != null)
        {
            var lastError = Errors.Last();
            if (lastError.Fragment == found.Value &&
                lastError.Line == found.Line &&
                lastError.Position == found.Column)
            {
                return;
            }
        }

        Errors.Add(new SyntaxError
        {
            Fragment = found != null ? found.Value : "конец файла",
            Line = found != null ? found.Line : 1,
            Position = found != null ? found.Column : 1,
            Description = message
        });
    }

    private Lexer.Token _lastValidTokenBeforeSemicolon = null;

    private void UpdateLastValidToken(Lexer.Token token)
    {
        if (token != null && token.Type != Lexer.TokenType.Error)
        {
            _lastValidTokenBeforeSemicolon = token;
        }
    }

    private bool SkipErrorTokens()
    {
        bool skipped = false;
        string lastErrorChar = null;

        while (!IsAtEnd && Current.Type == Lexer.TokenType.Error)
        {
            string currentChar = Current.Value;

            if (lastErrorChar == null || currentChar != lastErrorChar)
            {
                AddError($"Недопустимый символ '{currentChar}'", Current);
                lastErrorChar = currentChar;
            }

            skipped = true;
            Advance();
        }

        return skipped;
    }

    private bool IsComparisonOperator(string op)
    {
        return op == ">" || op == "<" || op == ">=" || op == "<=" || op == "==" || op == "!=";
    }

    private bool IsLogicalOperator(string op)
    {
        return op == "and" || op == "or" || op == "&&" || op == "||";
    }

    private bool HandleInvalidOperators()
    {
        if (IsAtEnd) return false;

        if (Check(Lexer.TokenType.Operator) && PeekNext != null && PeekNext.Type == Lexer.TokenType.Operator)
        {
            string firstOp = Current.Value;
            string secondOp = PeekNext.Value;
            string combined = firstOp + secondOp;

            if (!IsComparisonOperator(combined) && !IsLogicalOperator(combined))
            {
                AddError($"Недопустимый оператор '{combined}'", Current);
                Advance();
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool ParseOperand()
    {
        bool hadError = SkipErrorTokens();

        if (HandleInvalidOperators())
        {
            hadError = true;
            SkipErrorTokens();
        }

        if (Match(Lexer.TokenType.Identifier) || Match(Lexer.TokenType.Number))
        {
            UpdateLastValidToken(_tokens[_position - 1]);
            return true;
        }

        if (Match(Lexer.TokenType.LeftParen))
        {
            if (!ParseLogicalExpression())
            {
                return false;
            }
            if (!Match(Lexer.TokenType.RightParen))
            {
                AddError("Ожидалась закрывающая скобка ')'", Current);
                return false;
            }
            UpdateLastValidToken(_tokens[_position - 1]);
            return true;
        }

        return false;
    }

    private bool ParseRelation()
    {
        if (Check(Lexer.TokenType.Operator) && (Current.Value == "not" || Current.Value == "!"))
        {
            Advance();
            return ParseRelation();
        }

        SkipErrorTokens();
        HandleInvalidOperators();

        if (!ParseOperand())
        {
            return false;
        }

        if (Check(Lexer.TokenType.Operator))
        {
            string op = Current.Value;

            if (IsComparisonOperator(op))
            {
                Advance();

                SkipErrorTokens();
                HandleInvalidOperators();

                if (!ParseOperand())
                {
                    AddError("Ожидался операнд после оператора сравнения", Current);
                    return false;
                }
            }
            else if (!IsLogicalOperator(op))
            {
                HandleInvalidOperators();
                return false;
            }
        }

        return true;
    }

    private bool ParseLogicalExpression()
    {
        if (!ParseRelation())
        {
            return false;
        }

        while (Check(Lexer.TokenType.Operator) && IsLogicalOperator(Current.Value))
        {
            string logicalOp = Current.Value;
            Advance();

            SkipErrorTokens();
            HandleInvalidOperators();

            if (!ParseRelation())
            {
                AddError($"Ожидалось выражение после '{logicalOp}'", Current);
                return false;
            }
        }

        return true;
    }

    private Lexer.Token FindLastValidToken()
    {
        for (int i = _tokens.Count - 1; i >= 0; i--)
        {
            if (_tokens[i].Type != Lexer.TokenType.Error)
            {
                return _tokens[i];
            }
        }
        return null;
    }

    public bool Parse()
    {
        if (IsAtEnd)
        {
            AddError("Выражение не может быть пустым", null);
            return false;
        }

        bool hasError = false;
        _lastValidTokenBeforeSemicolon = null;
        int lastPosition = -1;

        while (!IsAtEnd)
        {
            // Защита от бесконечного цикла
            if (_position == lastPosition)
            {
                Advance();
                continue;
            }
            lastPosition = _position;

            // Пропускаем точки с запятой между выражениями
            while (Match(Lexer.TokenType.Semicolon, ";"))
            {
                lastPosition = _position;
            }

            if (IsAtEnd) break;

            // Пропускаем ошибочные токены в начале выражения
            SkipErrorTokens();

            if (IsAtEnd) break;

            // Проверяем, что текущий токен - идентификатор (начало выражения)
            if (!Check(Lexer.TokenType.Identifier))
            {
                if (Check(Lexer.TokenType.Operator))
                {
                    AddError($"Недопустимый оператор '{Current.Value}' в начале выражения", Current);
                    Advance();
                    hasError = true;
                    continue;
                }
                else if (!Check(Lexer.TokenType.Semicolon))
                {
                    AddError($"Ожидался идентификатор, найдено '{Current.Value}'", Current);
                    Advance();
                    hasError = true;
                    continue;
                }
            }

            // Парсим одно выражение
            // 1. Идентификатор
            if (!Match(Lexer.TokenType.Identifier))
            {
                Advance();
                hasError = true;
                continue;
            }

            _lastValidTokenBeforeSemicolon = _tokens[_position - 1];

            // 2. Оператор присваивания '='
            if (!Match(Lexer.TokenType.Operator, "="))
            {
                if (Check(Lexer.TokenType.Operator, "=="))
                {
                    AddError("Ожидался оператор присваивания '=', найдено '=='", Current);
                    Advance();
                }
                else if (!Check(Lexer.TokenType.Semicolon))
                {
                    AddError("Ожидался оператор присваивания '='", Current);
                }
                hasError = true;

                // Пропускаем до ';' или конца
                while (!IsAtEnd && !Check(Lexer.TokenType.Semicolon))
                {
                    Advance();
                }
                continue;
            }

            // 3. Условное выражение (value_if_true if condition else value_if_false)
            // value_if_true
            if (!ParseOperand())
            {
                AddError("Ожидался операнд (значение при true)", Current);
                hasError = true;
            }

            // if
            if (!Match(Lexer.TokenType.If))
            {
                if (Check(Lexer.TokenType.Identifier))
                {
                    AddError($"Ожидалось ключевое слово 'if', найдено '{Current.Value}'", Current);
                    Advance();
                }
                else if (!Check(Lexer.TokenType.Semicolon) && !IsAtEnd)
                {
                    AddError("Ожидалось ключевое слово 'if'", Current);
                }
                hasError = true;
            }

            // condition
            if (!ParseLogicalExpression())
            {
                // Пропускаем до 'else', ';' или конца
                while (!IsAtEnd && !Check(Lexer.TokenType.Else) && !Check(Lexer.TokenType.Semicolon))
                {
                    Advance();
                }
            }

            // else
            if (Match(Lexer.TokenType.Else))
            {
                if (!ParseOperand())
                {
                    AddError("Ожидался операнд (значение при false) после 'else'", Current);
                    hasError = true;
                }
            }
            else if (!Check(Lexer.TokenType.Semicolon) && !IsAtEnd)
            {
                if (Check(Lexer.TokenType.Identifier) && Current.Value.StartsWith("el"))
                {
                    AddError($"Ожидалось ключевое слово 'else', найдено '{Current.Value}' (опечатка)", Current);
                    Advance();
                    hasError = true;
                }
            }

            // 4. Пропускаем всё до точки с запятой
            while (!IsAtEnd && !Check(Lexer.TokenType.Semicolon))
            {
                if (Check(Lexer.TokenType.Error))
                {
                    SkipErrorTokens();
                }
                else if (Check(Lexer.TokenType.Operator))
                {
                    if (!HandleInvalidOperators())
                    {
                        Advance();
                    }
                }
                else
                {
                    Advance();
                }
            }

            // 5. Проверяем точку с запятой
            if (!Match(Lexer.TokenType.Semicolon, ";"))
            {
                if (IsAtEnd)
                {
                    Lexer.Token lastToken = _lastValidTokenBeforeSemicolon;
                    if (lastToken == null)
                    {
                        lastToken = FindLastValidToken();
                    }
                    AddError("Отсутствует точка с запятой ';' в конце выражения", lastToken);
                    hasError = true;
                }
            }
            else
            {
                _lastValidTokenBeforeSemicolon = _tokens[_position - 1];
            }
        }

        return !hasError;
    }
}