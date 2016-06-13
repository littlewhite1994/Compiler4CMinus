using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler4CMinus2
{
    enum TokenType
    {
        ENDFILE, ERROR,
        ELSE, IF, INT, RETURN, VOID, WHILE,
        ID, NUM,
        PLUS, MINUS, MULTI, DIVIDE, LT, LE, GT, GE, EQ, NE, ASSIGN, SEMI, COMMA, LPAREN, RPAREN, LSPAR, RSPAR, LCPAR, RCPAR,
        //+   -      *      /       <   <=  >   >=  ==  !=  =       ;     ,      (       )       [      ]      {      }
    }

    enum StateType
    {
        START, INLE, INGE, INEQ, INUEQ, INCOMSTART, INCOMMENT, INCOMDONE, INNUM, INID, DONE
    }

    enum NodeKind
    {
        StmtK, ExpK
    }

    enum StmtKind
    {
        ExpressionK, CompoundK, SelectionK, IteratonK, ReturnK,
        VarDecK, FunDecK, CallK, ArgsK, ArgK, ParamsK, ParamK,
        DecK, LocDecK, StatemsK
    }

    enum ExpKind
    {
        OpK, ConstK, IdK
    }

    enum ExpType
    {
        Void, Integer, UnDefined
    }
    
    static class Globals
    {
        public static int lineno = -1;
        public static string[] lines = null;

        public static string source;
        public static string listing;
        public static string code;

        public static bool EchoSource = true;
        public static bool TraceScan = true;
        public static bool TraceParse = true;
        public static bool TraceAnalyze = true;
        public static bool TraceCode = true;

        public static bool Error = false;

        public const int EOF = -1;
        public const int MAXCHILDREN = 3;

        public static string tokenString = "";
        public static string[] EXPTYPE = {"Void", "Integer", "Undefined" };
    }

    class Util
    {
        /// <summary>
        /// print the information of token.
        /// </summary>
        /// <param name="token">the type of token</param>
        /// <param name="tokenString">the string of token</param>
        public static void PrintToken(TokenType token, string tokenString)
        {
            switch (token)
            {
                case TokenType.ENDFILE:
                    Console.WriteLine("EOF");
                    break;
                case TokenType.ERROR:
                    Console.WriteLine("Lexical Error: {0}", tokenString);
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

        public static TreeNode NewStmtNode(StmtKind kind)
        {
            TreeNode t = new TreeNode();
            int i;
            if(t == null)
            {
                Console.WriteLine("Out of memory error at line {0}", Globals.lineno);
            }
            else
            {
                for (i = 0; i < Globals.MAXCHILDREN; i++)
                {
                    t.child[i] = null;
                }
                t.sibling = null;
                t.nodekind = NodeKind.StmtK;
                t.kind.stmt = kind;
                t.lineno = Globals.lineno;
            }
            return t;
        }

        public static TreeNode NewExpNode(ExpKind kind)
        {
            TreeNode t = new TreeNode();
            int i;
            if(t == null)
            {
                Console.WriteLine("Out of memory error at line {0}", Globals.lineno);
            }
            else
            {
                for (i = 0; i < Globals.MAXCHILDREN; i++) t.child[i] = null;
                t.sibling = null;
                t.nodekind = NodeKind.ExpK;
                t.kind.exp = kind;
                t.lineno = Globals.lineno;
                t.type = ExpType.Void;
            }
            return t;
        }

        public static int indentno = 0;
        static void Indent()
        {
            indentno += 2;
        }
        static void Unindent()
        {
            indentno -= 2;
        }

        static void PrintSpaces()
        {
            for (int i = 0; i < indentno; i++)
            {
                Console.Write(" ");
            }
        }

        public static void PrintTree(TreeNode tree)
        {
            int i;
            Indent();
            while(tree != null)
            {
                PrintSpaces();
                if (tree.nodekind == NodeKind.StmtK)
                {
                    switch (tree.kind.stmt)
                    {
                        case StmtKind.ExpressionK: Console.WriteLine("Expression"); break;
                        case StmtKind.CompoundK: Console.WriteLine("Compound"); break;
                        case StmtKind.SelectionK: Console.WriteLine("Selection"); break;
                        case StmtKind.IteratonK: Console.WriteLine("Iteration"); break;
                        case StmtKind.ReturnK: Console.WriteLine("Return"); break;
                        case StmtKind.VarDecK: Console.WriteLine("VarDeclaration: {0}", Globals.EXPTYPE[(int)tree.type]); break;
                        case StmtKind.FunDecK: Console.WriteLine("FunDeclaration: {0}", Globals.EXPTYPE[(int)tree.type]); break;
                        case StmtKind.CallK: Console.WriteLine("Call"); break;
                        case StmtKind.ArgK: Console.WriteLine("Argument"); break;
                        case StmtKind.ArgsK: Console.WriteLine("Arguments"); break;
                        case StmtKind.ParamK: Console.WriteLine("Parameter: {0}", Globals.EXPTYPE[(int)tree.type]); break;
                        case StmtKind.ParamsK: Console.WriteLine("Parameters"); break;
                        case StmtKind.DecK: Console.WriteLine("Declaration: {0}", Globals.EXPTYPE[(int)tree.type]); break;
                        case StmtKind.LocDecK: Console.WriteLine("LocalDeclaration"); break;
                        case StmtKind.StatemsK: Console.WriteLine("Statement"); break;
                        default: Console.WriteLine("Unknown ExpNode kind"); break;
                    }
                }
                else if (tree.nodekind == NodeKind.ExpK)
                {
                    switch (tree.kind.exp)
                    {
                        case ExpKind.OpK:
                            Console.Write("Op: ");
                            Util.PrintToken(tree.attr.op, "\0");
                            break;
                        case ExpKind.ConstK:
                            Console.WriteLine("Const: {0}", tree.attr.val);
                            break;
                        case ExpKind.IdK:
                            Console.WriteLine("Id: {0}", tree.attr.name);
                            break;
                        default:
                            Console.WriteLine("Unknown ExpNode kind");
                            break;
                    }
                }
                else Console.WriteLine("Unknown node kind");
                for(i = 0; i < Globals.MAXCHILDREN; i++)
                {
                    PrintTree(tree.child[i]);
                }
                tree = tree.sibling;
            }
            Unindent();
        }
    }

    class TreeNode
    {
        public TreeNode[] child = new TreeNode[Globals.MAXCHILDREN];
        public TreeNode sibling;
        public int lineno;
        public NodeKind nodekind;
        public KindSelect kind;
        public TokenSelect attr;
        public ExpType type;
        public TreeNode()
        {
            kind = new KindSelect();
            attr = new TokenSelect();
        }
    }
    class KindSelect
    {
        public StmtKind stmt;
        public ExpKind exp;
    }
    class TokenSelect
    {
        public TokenType op;
        public int val;
        public string name;
    }

    class CMinus
    {
        
        static void Main(string[] args)
        {
            TreeNode syntaxTree;
            if (args.Length != 0)
            {
                Globals.source = args[0];
            }
            else
            {
                Globals.source = @"E:\Software\Visual Studio Projects\Compiler4CMinus\Compiler4CMinus\test1.txt";
            }
            if (Globals.source != null)
                Globals.lines = GetStringLine(Globals.source);
            else
            {
                Console.WriteLine("Error: source file not found!");
                Console.ReadKey();
            }
            Console.WriteLine(">>> CMinus Compilation <<<");
            #region scanning process
            //while (Scanner.GetToken() != TokenType.ENDFILE) ;
            #endregion
            #region parsing process
            //Console.WriteLine("---lineno is {0}---", Globals.lineno);
            if (Globals.lineno != -1)
                Globals.lineno = -1;
            syntaxTree = Parser.Parse();
            if (Globals.TraceParse && !Globals.Error)
            {
                Console.WriteLine("\n>>> Syntax tree <<<");
                Util.PrintTree(syntaxTree);
            }
            #endregion

            Console.ReadKey();
        }

        public static string[] GetStringLine(string path)
        {
            return File.ReadAllLines(path);
        }
    }

    class Scanner
    {
        static int linepos = 0;
        static int bufsize = 0;
        public static StringBuilder tokenString = new StringBuilder(256);

        /// <summary>
        /// get next character
        /// </summary>
        /// <returns></returns>
        public static int GetNextChar()
        {
            if (!(linepos < bufsize))
            {
                Globals.lineno++;
                if (Globals.lineno < Globals.lines.Length)
                {
                    Console.WriteLine("{0}: {1}", Globals.lineno + 1, Globals.lines[Globals.lineno]);
                    bufsize = Globals.lines[Globals.lineno].Length;
                    linepos = 0;
                    if (bufsize == 0) return '\n';
                    return Globals.lines[Globals.lineno].ElementAt(linepos++);
                }
                else
                {
                    Console.WriteLine();
                    return Globals.EOF;
                }
            }
            else return Globals.lines[Globals.lineno].ElementAt(linepos++);
        }
        /// <summary>
        /// get last character
        /// </summary>
        public static void UngetNextChar()
        {
            linepos--;
        }
        /// <summary>
        /// get a token
        /// </summary>
        /// <returns></returns>
        public static TokenType GetToken()
        {
            //int tokenStringIndex = 0;
            TokenType currentToken = TokenType.ERROR;
            StateType state = StateType.START;
            bool save = false;
            while (state != StateType.DONE)
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
                                case Globals.EOF: currentToken = TokenType.ENDFILE; save = false; break;
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
                        if (c == '*')
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
                if (state == StateType.DONE)
                {
                    if (currentToken == TokenType.ID)
                        currentToken = reservedLookup(tokenString.ToString());
                }
            }
            Console.Write("\t" + (Globals.lineno + 1) + ": ");
            Globals.tokenString = tokenString.ToString();
            Util.PrintToken(currentToken, Globals.tokenString);
            tokenString.Clear();
            return currentToken;
        }
        /// <summary>
        /// is reserved word
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
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
        /// <summary>
        /// is digital
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        static bool IsDigit(int c)
        {
            if (c >= '0' && c <= '9')
                return true;
            else
                return false;
        }
        /// <summary>
        /// is alpha
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
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
    class Parser
    {
        static TokenType token;
        static TokenType lastToken;

        /// <summary>
        /// print error message
        /// </summary>
        /// <param name="message"></param>
        static void SyntaxError(string message)
        {
            Console.WriteLine(">>>");
            Console.WriteLine("Syntax error at line {0}: {1}", Globals.lineno, message);
            Globals.Error = true;
        }
        /// <summary>
        /// match the expected token
        /// </summary>
        /// <param name="expected"></param>
        static void Match(TokenType expected)
        {
            if (token == expected)
            {
                lastToken = token;
                token = Scanner.GetToken();
            }
                
            else
            {
                SyntaxError("unexpected token -> ");
                Util.PrintToken(token, Globals.tokenString);
                Console.WriteLine();
            }
        }
        #region BNF
        static TreeNode program()
        {
            return declaration_list();
        }
        static TreeNode declaration_list()
        {
            TreeNode t = declaration();
            TreeNode p = t;
            while ((token != TokenType.INT) && (token != TokenType.VOID) && (token != TokenType.ENDFILE))
            {
                if (token == TokenType.ENDFILE)
                {
                    return null;
                }
                SyntaxError("Unexpected token -> ");
                Util.PrintToken(token, Globals.tokenString);
                token = Scanner.GetToken();
            }
            while ((token == TokenType.INT) || (token == TokenType.VOID))
            {
                TreeNode q;
                q = declaration();
                if(q != null)
                {
                    if (t == null)
                        t = p = q;
                    else
                    {
                        p.sibling = q;
                        p = q;
                    }
                }
            }
            return t;
        }
        static TreeNode declaration()
        {
            TreeNode t = fun_declaration();
            return t;
        }
        static TreeNode var_declaration()
        {
            TreeNode t = null;
            if(token == TokenType.INT || token == TokenType.VOID)
            {
                t = Util.NewStmtNode(StmtKind.VarDecK);
                if (token == TokenType.INT)
                    t.type = ExpType.Integer;
                else
                    t.type = ExpType.Void;
                Match(token);
                TreeNode p = Util.NewExpNode(ExpKind.IdK);
                if(p != null && token == TokenType.ID)
                {
                    p.attr.name = Globals.tokenString;
                    t.child[0] = p;
                    Match(TokenType.ID);
                }
                else
                {
                    SyntaxError("Unexpected token -> ");
                    Util.PrintToken(token, Globals.tokenString);
                    token = Scanner.GetToken();
                }
                if(token == TokenType.LSPAR)
                {
                    Match(TokenType.LSPAR);
                    TreeNode q = Util.NewExpNode(ExpKind.ConstK);
                    if (q != null && token == TokenType.NUM)
                        q.attr.val = Int32.Parse(Globals.tokenString);
                    p.child[1] = q;
                    Match(TokenType.NUM);
                    Match(TokenType.RSPAR);
                }
                Match(TokenType.SEMI);
            }
            return t;
        }
        static TreeNode fun_declaration()
        {
            TreeNode t = null;
            if((token == TokenType.INT) || (token == TokenType.VOID))
            {
                t = Util.NewStmtNode(StmtKind.VarDecK);
                if (token == TokenType.INT)
                    t.type = ExpType.Integer;
                else
                    t.type = ExpType.Void;
                Match(token);
                TreeNode p = Util.NewExpNode(ExpKind.IdK);
                if(p != null && token == TokenType.ID)
                {
                    p.attr.name = Globals.tokenString;
                    t.child[0] = p;
                    Match(TokenType.ID);
                }
                else
                {
                    SyntaxError("Unexpected token -> ");
                    Util.PrintToken(token, Globals.tokenString);
                    token = Scanner.GetToken();
                }
                if(token == TokenType.LPAREN)
                {
                    TreeNode func = Util.NewStmtNode(StmtKind.FunDecK);
                    func.type = t.type;
                    func.child[0] = t.child[0];
                    if (token == TokenType.LPAREN)
                    {
                        Match(TokenType.LPAREN);
                        func.child[1] = paramas();
                        Match(TokenType.RPAREN);
                        func.child[2] = compound_stmt();
                    }
                    return func;
                }
                else
                {
                    if(token == TokenType.LSPAR)
                    {
                        Match(TokenType.LSPAR);
                        TreeNode q = Util.NewExpNode(ExpKind.ConstK);
                        if (q != null && token == TokenType.NUM)
                            q.attr.val = Int32.Parse(Globals.tokenString);
                        p.child[1] = q;
                        Match(TokenType.NUM);
                        Match(TokenType.RSPAR);
                    }
                    Match(TokenType.SEMI);
                }
            }
            return t;
        }
        static TreeNode paramas()
        {
            TreeNode t = params_list();
            return t;
        }
        static TreeNode params_list()
        {
            TreeNode t = param();
            if(t == null)
            {
                t = Util.NewStmtNode(StmtKind.ParamK);
                t.type = ExpType.Void;
                return t;
            }
            TreeNode p = t;
            while(token == TokenType.COMMA)
            {
                TreeNode q;
                Match(TokenType.COMMA);
                q = param();
                if(q != null)
                {
                    if (t == null) t = p = q;
                    else
                    {
                        p.sibling = q;
                        p = q;
                    }
                }
            }
            return t;
        }
        static TreeNode param()
        {
            TreeNode t = null;
            if(token == TokenType.INT || token == TokenType.VOID)
            {
                t = Util.NewStmtNode(StmtKind.ParamK);
                if (token == TokenType.INT)
                    t.type = ExpType.Integer;
                else
                    t.type = ExpType.Void;
                if (lastToken == TokenType.LPAREN && token == TokenType.VOID)
                {
                    Match(token);
                    if (lastToken == TokenType.VOID && token == TokenType.RPAREN)
                        return null;
                }
                else
                    Match(token);
                TreeNode p = Util.NewExpNode(ExpKind.IdK);
                if(p != null && token == TokenType.ID)
                {
                    p.attr.name = Globals.tokenString;
                    t.child[0] = p;
                }
                Match(TokenType.ID);
                if(token == TokenType.LSPAR)
                {
                    Match(TokenType.LSPAR);
                    p = Util.NewExpNode(ExpKind.ConstK);
                    if(p != null)
                    {
                        p.attr.val = 0;
                        t.child[1] = p;
                    }
                    Match(TokenType.RSPAR);
                }
            }
            else
            {
                SyntaxError("Unexpected token -> ");
                Util.PrintToken(token, Globals.tokenString);
                token = Scanner.GetToken();
            }
            return t;
        }
        static TreeNode compound_stmt()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.CompoundK);
            Match(TokenType.LCPAR);
            if(t != null)
            {
                t.child[0] = local_declarations();
                t.child[1] = statement_list();
            }
            Match(TokenType.RCPAR);
            return t;
        }
        static TreeNode local_declarations()
        {
            TreeNode t = var_declaration();
            TreeNode p = t;
            while(token == TokenType.INT || token == TokenType.VOID)
            {
                TreeNode q = var_declaration();
                if(q != null)
                {
                    if (t == null) t = p = q;
                    else
                    {
                        p.sibling = q;
                        p = q;
                    }
                }
            }
            return t;
        }
        static TreeNode statement_list()
        {
            TreeNode t = statement();
            TreeNode p = t;
            while(token != TokenType.RCPAR)
            {
                if(token == TokenType.ENDFILE)
                {
                    SyntaxError("Unexpected token -> ");
                    Util.PrintToken(token, Globals.tokenString);
                    break;
                }
                TreeNode q;
                q = statement();
                if(q != null)
                {
                    if (t == null) t = p = q;
                    else
                    {
                        p.sibling = q;
                        p = q;
                    }
                }
            }
            return t;
        }
        static TreeNode statement()
        {
            TreeNode t = null;
            switch (token)
            {
                case TokenType.IF:
                    t = selection_stmt(); break;
                case TokenType.LCPAR:
                    t = compound_stmt(); break;
                case TokenType.WHILE:
                    t = iteration_stmt(); break;
                case TokenType.RETURN:
                    t = return_stmt(); break;
                case TokenType.ID:
                case TokenType.LPAREN:
                case TokenType.NUM:
                    t = expression_stmt(); break;
                default:
                    SyntaxError("Unexpected token -> ");
                    Util.PrintToken(token, Globals.tokenString);
                    token = Scanner.GetToken();
                    break;
            }
            return t;
        }
        static TreeNode expression_stmt()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.ExpressionK);
            if (token != TokenType.SEMI && t != null)
                t.child[0] = expression();
            Match(TokenType.SEMI);
            return t;
        }
        static TreeNode selection_stmt()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.SelectionK);
            Match(TokenType.IF);
            Match(TokenType.LPAREN);
            if (t != null)
                t.child[0] = expression();
            Match(TokenType.RPAREN);
            if (t != null)
                t.child[1] = statement();
            if(token == TokenType.ELSE)
            {
                Match(TokenType.ELSE);
                if (t != null)
                    t.child[2] = statement();
            }
            return t;
        }
        static TreeNode iteration_stmt()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.IteratonK);
            Match(TokenType.WHILE);
            Match(TokenType.LPAREN);
            if (t != null)
                t.child[0] = expression();
            Match(TokenType.RPAREN);
            if (t != null)
                t.child[1] = statement();
            return t;
        }
        static TreeNode return_stmt()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.ReturnK);
            Match(TokenType.RETURN);
            if(token != TokenType.SEMI)
            {
                if (t != null)
                    t.child[0] = expression();
            }
            Match(TokenType.SEMI);
            return t;
        }
        static TreeNode expression()
        {
            TreeNode t = simple_expression();
            TreeNode p = Util.NewExpNode(ExpKind.OpK);
            if(token == TokenType.ASSIGN)
            {
                p.attr.op = token;
                Match(TokenType.ASSIGN);
                p.child[0] = t;
                p.child[1] = expression();
                return p;
            }
            return t;
        }
        static TreeNode var()
        {
            TreeNode t = null;
            if(token == TokenType.ID)
            {
                t = Util.NewExpNode(ExpKind.IdK);
                if (t != null && token == TokenType.ID)
                    t.attr.name = Globals.tokenString;
                Match(TokenType.ID);
                if(token == TokenType.LSPAR)
                {
                    Match(TokenType.LSPAR);
                    t.child[0] = expression();
                    Match(TokenType.RSPAR);
                }
            }
            return t;
        }
        static TreeNode simple_expression()
        {
            TreeNode t = additive_expression();
            if(token == TokenType.LE || token == TokenType.LT 
                || token == TokenType.GE || token == TokenType.GT
                || token == TokenType.EQ || token == TokenType.NE)
            {
                TreeNode p = Util.NewExpNode(ExpKind.OpK);
                if(p != null)
                {
                    p.child[0] = t;
                    p.attr.op = token;
                    t = p;
                }
                Match(token);
                if (t != null)
                    t.child[1] = additive_expression();
            }
            return t;
        }
        static TreeNode additive_expression()
        {
            TreeNode t = term();
            while(token == TokenType.PLUS || token == TokenType.MINUS)
            {
                TreeNode p = Util.NewExpNode(ExpKind.OpK);
                if(p != null)
                {
                    p.child[0] = t;
                    p.attr.op = token;
                    t = p;
                    Match(token);
                    t.child[1] = term();
                }
            }
            return t;
        }
        static TreeNode term()
        {
            TreeNode t = factor();
            while(token == TokenType.MULTI || token == TokenType.DIVIDE)
            {
                TreeNode p = Util.NewExpNode(ExpKind.OpK);
                if(p != null){
                    p.child[0] = t;
                    p.attr.op = token;
                    t = p;
                    Match(token);
                    p.child[1] = factor();
                }
            }
            return t;
        }
        static TreeNode factor()
        {
            TreeNode t = null;
            switch (token)
            {
                case TokenType.NUM:
                    t = Util.NewExpNode(ExpKind.ConstK);
                    if (t != null && token == TokenType.NUM)
                        t.attr.val = Int32.Parse(Globals.tokenString);
                    Match(TokenType.NUM);
                    break;
                case TokenType.LPAREN:
                    Match(TokenType.LPAREN);
                    t = expression();
                    Match(TokenType.RPAREN);
                    break;
                case TokenType.ID:
                    t = Util.NewExpNode(ExpKind.IdK);
                    if (t != null && token == TokenType.ID)
                        t.attr.name = Globals.tokenString;
                    Match(TokenType.ID);
                    if(token == TokenType.LSPAR)
                    {
                        Match(TokenType.LSPAR);
                        t.child[0] = expression();
                        Match(TokenType.RSPAR);
                    }
                    else if(token == TokenType.LPAREN)
                    {
                        Match(TokenType.LPAREN);
                        t.child[0] = args();
                        Match(TokenType.RPAREN);
                    }
                    break;
                default:
                    SyntaxError("Unexpected token -> ");
                    Util.PrintToken(token, Globals.tokenString);
                    token = Scanner.GetToken();
                    break;
            }
            return t;
        }
        
        static TreeNode args()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.ArgsK);
            if (token != TokenType.RPAREN)
                t = arg_list();
            return t;
        }
        static TreeNode arg_list()
        {
            TreeNode t = Util.NewStmtNode(StmtKind.ArgsK);
            TreeNode p = t;
            TreeNode q = expression();
            while(token == TokenType.COMMA)
            {
                Match(TokenType.COMMA);
                if(q != null)
                {
                    if (t != null) t = p = q;
                    else
                    {
                        p.sibling = q;
                        p = q;
                    }
                }
                q = expression();
            }
            if(q != null)
            {
                if (t == null) t = p = q;
                else
                {
                    p.sibling = q;
                    p = q;
                }
            }
            return t;
        }
        public static TreeNode Parse()
        {
            TreeNode t;
            token = Scanner.GetToken();
            t = program();
            if (token != TokenType.ENDFILE)
                SyntaxError("Code ends before file");
            return t;
        }
        #endregion

    }
}
