using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Compiler4CMinus
{

    /// <summary>
    /// type of token
    /// </summary>
    enum TokenType
    {
        ENDFILE, ERROR,
        ELSE, IF, INT, RETURN, VOID, WHILE,
        ID, NUM,
        PLUS, MINUS, MULTI, DIVIDE, LT, LE, GT, GE, EQ, NE, ASSIGN, SEMI, COMMA, LPAREN, RPAREN, LSPAR, RSPAR, LCPAR, RCPAR,
        //+   -      *      /       <   <=  >   >=  ==  !=  =       ;     ,      (       )       [      ]      {      }
    }
    /// <summary>
    /// type of state 
    /// </summary>
    enum StateType
    {
        START, INLE, INGE, INEQ, INUEQ, INCOMSTART, INCOMMENT, INCOMDONE, INNUM, INID, DONE
    }    

    class Program
    {
        private static string test = @"E:\Software\Visual Studio Projects\Compiler4CMinus\Compiler4CMinus\test2.txt";
        const int MAXRESERVED = 6;
       
        static void Main(string[] args)
        {
            while (GetToken() != TokenType.ENDFILE) ;
            Console.ReadKey();
        }

        public static void AddLineNumber(string path)
        {
            string[] lines = File.ReadAllLines(path);
            //char[] chars = lines[0].ToCharArray();
            //for (int i = 0; i < chars.Length; i++)
            //{
            //    Console.WriteLine(chars[i] + " ");
            //}
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(i + "\t" + lines[i] + "\t consist of " + lines[i].Length + "chars");
            }
            Console.ReadKey();
        }

        public static string[] GetStringLine(string path)
        {
            return File.ReadAllLines(path);
            
            
        }

        public static void PrintToken(TokenType token, string tokenString)
        {
            switch (token)
            {
                case TokenType.ENDFILE:
                    Console.WriteLine("EOF");
                    break;
                case TokenType.ERROR:
                    Console.WriteLine("Error: {0}", tokenString);
                    break;
                case TokenType.ELSE:
                case TokenType.IF:
                case TokenType.INT:
                case TokenType.RETURN:
                case TokenType.VOID:
                case TokenType.WHILE:
                    Console.WriteLine("reserved word: {0}", tokenString);
                    break;
                case TokenType.ID:
                    Console.WriteLine("ID, name= {0}", tokenString);
                    break;
                case TokenType.NUM:
                    Console.WriteLine("NUM, val= {0}", tokenString);
                    break;
                case TokenType.PLUS:
                    Console.WriteLine("+");
                    break;
                case TokenType.MINUS:
                    Console.WriteLine("-");
                    break;
                case TokenType.MULTI:
                    Console.WriteLine("*");
                    break;
                case TokenType.DIVIDE:
                    Console.WriteLine("/");
                    break;
                case TokenType.LT:
                    Console.WriteLine("<");
                    break;
                case TokenType.LE:
                    Console.WriteLine("<=");
                    break;
                case TokenType.GT:
                    Console.WriteLine(">");
                    break;
                case TokenType.GE:
                    Console.WriteLine(">=");
                    break;
                case TokenType.EQ:
                    Console.WriteLine("==");
                    break;
                case TokenType.NE:
                    Console.WriteLine("!=");
                    break;
                case TokenType.ASSIGN:
                    Console.WriteLine("=");
                    break;
                case TokenType.SEMI:
                    Console.WriteLine(";");
                    break;
                case TokenType.COMMA:
                    Console.WriteLine(",");
                    break;
                case TokenType.LPAREN:
                    Console.WriteLine("(");
                    break;
                case TokenType.RPAREN:
                    Console.WriteLine(")");
                    break;
                case TokenType.LSPAR:
                    Console.WriteLine("[");
                    break;
                case TokenType.RSPAR:
                    Console.WriteLine("]");
                    break;
                case TokenType.LCPAR:
                    Console.WriteLine("{");
                    break;
                case TokenType.RCPAR:
                    Console.WriteLine("}");
                    break;
                default:
                    Console.WriteLine("Unknown token: {0}", token);
                    break;
            }
        }
        static string[] lines = GetStringLine(test);
        static int linepos = 0;
        static int bufsize = 0;
        static int lineno = -1;
        const int EOF = -1;
        public static int GetNextChar()
        {
            if(!(linepos < bufsize))
            {
                lineno++;
                if(lineno < lines.Length)
                {
                    Console.WriteLine("{0}: {1}", lineno+1, lines[lineno]);
                    bufsize = lines[lineno].Length;
                    linepos = 0;
                    if (bufsize == 0) return '\n';
                    return lines[lineno].ElementAt(linepos++);
                }
                else
                {
                    Console.WriteLine();
                    return EOF;
                }
            }
            else return lines[lineno].ElementAt(linepos++);
        } 
        public static void UngetNextChar()
        {
            linepos--;
        }
        static StringBuilder tokenString = new StringBuilder(256);
        public static TokenType GetToken()
        {
            int tokenStringIndex = 0;
            TokenType currentToken = TokenType.ERROR;
            StateType state = StateType.START;
            bool save = false;
            while(state != StateType.DONE)
            {
                int c = GetNextChar();
                save = true;
                switch (state)
                {
                    #region start state
                    case StateType.START:
                        if (IsDigit(c)) state = StateType.INNUM;
                        else if (IsAlpha(c)) state = StateType.INID;
                        else if (c == ' ' || c == '\t' || c == '\n') save = false;
                        else if (c == '/')
                        {
                            save = false;
                            state = StateType.INCOMSTART;
                        }
                        else if (c == '<') state = StateType.INLE;
                        else if (c == '>') state = StateType.INGE;
                        else if (c == '=') state = StateType.INEQ;
                        else if (c == '!') state = StateType.INUEQ;
                        else
                        {
                            state = StateType.DONE;
                            switch (c)
                            {
                                case EOF: currentToken = TokenType.ENDFILE; save = false; break;
                                case '+': currentToken = TokenType.PLUS; break;
                                case '-': currentToken = TokenType.MINUS; break;
                                case '*': currentToken = TokenType.MULTI; break;
                                case ';': currentToken = TokenType.SEMI; break;
                                case ',': currentToken = TokenType.COMMA; break;
                                case '(': currentToken = TokenType.LPAREN; break;
                                case ')': currentToken = TokenType.RPAREN; break;
                                case '[': currentToken = TokenType.LSPAR; break;
                                case ']': currentToken = TokenType.RSPAR; break;
                                case '{': currentToken = TokenType.LCPAR; break;
                                case '}': currentToken = TokenType.RCPAR; break;
                                default: currentToken = TokenType.ERROR; break;
                            }
                        }
                        break;
                    #endregion
                    #region incomstart state
                    case StateType.INCOMSTART:
                        if(c == '*')
                        {
                            save = false;
                            state = StateType.INCOMMENT;
                        }
                        else
                        {
                            save = true;
                            state = StateType.DONE;
                            currentToken = TokenType.DIVIDE;
                            UngetNextChar();
                        } 
                        break;
                    #endregion
                    #region incomment state
                    case StateType.INCOMMENT:
                        save = false;
                        if (c == '*') state = StateType.INCOMDONE;
                        break;
                    #endregion
                    #region incomdone state
                    case StateType.INCOMDONE:
                        save = false;
                        if (c == '/') state = StateType.START;
                        else state = StateType.INCOMMENT;
                        break;
                    #endregion
                    #region inle start
                    case StateType.INLE:
                        state = StateType.DONE;
                        if (c == '=') currentToken = TokenType.LE;
                        else
                        {
                            UngetNextChar();
                            currentToken = TokenType.LT;
                        }
                        break;
                    #endregion
                    #region inge state
                    case StateType.INGE:
                        state = StateType.DONE;
                        if (c == '=') currentToken = TokenType.GE;
                        else
                        {
                            UngetNextChar();
                            currentToken = TokenType.GT;
                        }
                        break;
                    #endregion
                    #region ineq
                    case StateType.INEQ:
                        state = StateType.DONE;
                        if (c == '=') currentToken = TokenType.EQ;
                        else
                        {
                            UngetNextChar();
                            currentToken = TokenType.ASSIGN;
                        }
                        break;
                    #endregion
                    #region inueq
                    case StateType.INUEQ:
                        state = StateType.DONE;
                        if (c == '=') currentToken = TokenType.NE;
                        else
                        {
                            UngetNextChar();
                            save = false;
                            currentToken = TokenType.ERROR;
                        }
                        break;
                    #endregion
                    #region innum
                    case StateType.INNUM:
                        if (!IsDigit(c))
                        {
                            UngetNextChar();
                            save = false;
                            state = StateType.DONE;
                            currentToken = TokenType.NUM;
                        }
                        break;
                    #endregion
                    #region inid
                    case StateType.INID:
                        if (!IsAlpha(c))
                        {
                            UngetNextChar();
                            save = false;
                            state = StateType.DONE;
                            currentToken = TokenType.ID;
                        }
                        break;
                    #endregion
                    #region done
                    case StateType.DONE:
                    default:
                        Console.WriteLine("Scanner Bug: state= {0}", state);
                        state = StateType.DONE;
                        currentToken = TokenType.ERROR;
                        break;
                    #endregion
                }
                if (save) tokenString.Append((char)c);
                if(state == StateType.DONE)
                {
                    if (currentToken == TokenType.ID)
                        currentToken = reservedLookup(tokenString.ToString());
                }
            }
            Console.Write("\t" + (lineno+1) + ": ");
            PrintToken(currentToken, tokenString.ToString());
            tokenString.Clear();
            return currentToken;
        }
        static TokenType reservedLookup(string tokenString)
        {
            if (tokenString.Equals("if")) return TokenType.IF;
            else if (tokenString.Equals("else")) return TokenType.ELSE;
            else if (tokenString.Equals("int")) return TokenType.INT;
            else if (tokenString.Equals("void")) return TokenType.VOID;
            else if (tokenString.Equals("while")) return TokenType.WHILE;
            else if (tokenString.Equals("return")) return TokenType.RETURN;
            else return TokenType.ID;
        }
        static bool IsDigit(int c)
        {
            if (c >= '0' && c <= '9')
                return true;
            else
                return false;
        }

        static bool IsAlpha(int c)
        {
            if (c >= 'a' && c <= 'z')
                return true;
            else if (c >= 'A' && c <= 'Z')
                return true;
            else
                return false;
        }
    }
}
