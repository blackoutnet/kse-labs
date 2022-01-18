using System.Globalization;
using System.Text;

namespace mathexp_parser;

internal static class Token
{
    public const string Add = "+";
    public const string Subtract = "-";
    public const string Multiply = "*";
    public const string Divide = "/";
    public const string Power = "^";
    public const string LeftParenthesis = "(";
    public const string RightParenthesis = ")";
    public const string Decimal = ".";
    public const string Zero = "0";
}

internal static class OperatorAssociativity
{
    public const int Left = 0;
    public const int Right = 1;
}

public class ExpressionParser
{
    private readonly string _input;
    private int _index;

    public ExpressionParser(string input)
    {
        _input = input;
        _index = 0;
    }

    private IEnumerable<string> Tokenize()
    {

        var tokens = new List<string>();
        var negative = false;
        
        SkipWhiteSpaces();
        if (_index == _input.Length)
        {
            throw new ExpressionParserException("Expression is empty!");
        }
        while (_index < _input.Length)
        {
            var s = _input[_index].ToString();
            switch (s)
            {
                case Token.Subtract:
                    negative = true;
                    if (_index == 0)
                    {
                        tokens.Add(Token.Zero);
                    }

                    tokens.Add(Token.Add);
                    _index += 1;
                    break;
                case Token.Add:
                case Token.Multiply:
                case Token.Divide:
                case Token.Power:
                case Token.LeftParenthesis:
                case Token.RightParenthesis:
                    tokens.Add(s);
                    _index += 1;
                    break;
                default:
                    if (char.IsDigit(s[0]))
                    {
                        var numberBuffer = new StringBuilder();
                        while (_index < _input.Length && (char.IsDigit(s[0]) || s == Token.Decimal))
                        {
                            numberBuffer.Append(s);
                            _index += 1;
                            if (_index < _input.Length)
                                s = _input[_index].ToString();
                        }

                        if (negative)
                        {
                            negative = false;
                            numberBuffer.Insert(0, "-");
                        }

                        var number = numberBuffer.ToString();
                        if (numberBuffer[^1].ToString() == Token.Decimal)
                        {
                            throw new ExpressionParserException($"Invalid number '{number}'");
                        }

                        tokens.Add(number);
                    }
                    else
                    {
                        throw new ExpressionParserException($"Unexpected token '{s}'");
                    }

                    break;
            }

            
            SkipWhiteSpaces();
        }

        return tokens;
    }

    private Queue<string> Parse()
    {
        var tokens = Tokenize();
        var output = new Queue<string>();
        var operatorStack = new CustomStack<string>();

        foreach (var token in tokens)
        {
            if (TokenIsNumber(token))
            {
                output.Enqueue(token);
            }
            else
                switch (token)
                {
                    case Token.LeftParenthesis:
                        operatorStack.Push(token);
                        break;
                    case Token.RightParenthesis:
                    {
                        string? topOperator;

                        while (operatorStack.TryPeek(out topOperator) && topOperator != Token.LeftParenthesis)
                        {
                            output.Enqueue(operatorStack.Pop());
                        }

                        if (topOperator == Token.LeftParenthesis)
                        {
                            operatorStack.Pop();
                        }
                        else
                        {
                            throw new ExpressionParserException("Mismatched parentheses ')'");
                        }

                        break;
                    }
                    default:
                    {
                        while (operatorStack.TryPeek(out var topOperator) &&
                               topOperator != Token.LeftParenthesis &&
                               (GetOperatorPrecedence(topOperator) > GetOperatorPrecedence(token) ||
                                GetOperatorPrecedence(topOperator) == GetOperatorPrecedence(token) &&
                                GetOperatorAssociativity(token) == OperatorAssociativity.Left))
                        {
                            output.Enqueue(operatorStack.Pop());
                        }

                        operatorStack.Push(token);
                        break;
                    }
                }
        }
        
        while (operatorStack.Count > 0)
        {
            var top = operatorStack.Pop();
            if (top == Token.LeftParenthesis)
            {
                throw new ExpressionParserException("Mismatched parentheses '('");
            }

            output.Enqueue(top);
        }

        return output;
    }


    public string Evaluate()
    {
        var representation = Parse();
        var evaluationStack = new Stack<string>();

        foreach (var token in representation)
        {
            if (TokenIsNumber(token))
            {
                evaluationStack.Push(token);
            }
            else
            {
                try
                {
                    var secondOperand = double.Parse(evaluationStack.Pop());
                    var firstOperand = double.Parse(evaluationStack.Pop());
                    
                    var result = token switch
                    {
                        Token.Add => firstOperand + secondOperand,
                        Token.Subtract => firstOperand - secondOperand,
                        Token.Multiply => firstOperand * secondOperand,
                        Token.Divide => secondOperand != 0 ? firstOperand / secondOperand : double.PositiveInfinity,
                        Token.Power => Math.Pow(firstOperand, secondOperand),
                        _ => double.PositiveInfinity
                    };
                    if (double.IsInfinity(result))
                    {
                        throw new ExpressionParserException("Division by zero!");
                    }

                    evaluationStack.Push(result.ToString(CultureInfo.InvariantCulture));
                }
                catch (InvalidOperationException)
                {
                    throw new ExpressionParserException("Invalid expression!");
                }

            }
        }

        if (evaluationStack.Count != 1)
        {
            throw new ExpressionParserException("Invalid expression!");
        }
        return evaluationStack.Pop();
    }

    private void SkipWhiteSpaces()
    {
        while (_index < _input.Length && char.IsWhiteSpace(_input[_index]))
        {
            _index += 1;
        }
    }

    private static int GetOperatorPrecedence(string @operator)
    {
        var precedence = @operator switch
        {
            Token.Add or Token.Subtract => 1,
            Token.Multiply or Token.Divide => 2,
            Token.Power => 3,
            _ => 4
        };

        return precedence;
    }

    private static int GetOperatorAssociativity(string @operator)
    {
        return Convert.ToInt32(@operator == Token.Power);
    }

    private static bool TokenIsNumber(string token)
    {
        return token.All(c => char.IsDigit(c) || c.ToString() == Token.Decimal || c.ToString() == Token.Subtract);
    }
}