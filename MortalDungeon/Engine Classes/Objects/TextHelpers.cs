using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public enum Character
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z,
        Period, QuestionMark, ExclamationPoint, Comma, QuotationMark, Apostrophe, LeftBracket, RightBracket, Plus, Minus, Equals, Underscore, Colon, Semicolon,
        LessThan, GreaterThan, At, DollarSign, Modulo, Caret, Asterisk, LeftParenthesis, RightParenthesis, And, Pipe, ForwardSlash, BackSlash, Hash, Tilde, BackTick,
        One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero, Space, NewLine, Return
    }
    static class CharacterConstants
    {
        public static Dictionary<char, Character> _characterMap = new Dictionary<char, Character>()
        {
            {'A', Character.A }, {'B', Character.B },{'C', Character.C },{'D', Character.D },{'E', Character.E },{'F', Character.F },{'G', Character.G },{'H', Character.H },{'I', Character.I },{'J', Character.J },
            {'K', Character.K },{'L', Character.L },{'M', Character.M },{'N', Character.N },{'O', Character.O },{'P', Character.P },{'Q', Character.Q },{'R', Character.R },{'S', Character.S },{'T', Character.T },
            {'U', Character.U },{'V', Character.V },{'W', Character.W },{'X', Character.X },{'Y', Character.Y },{'Z', Character.Z },{'a', Character.a },{'b', Character.b },{'c', Character.c },{'d', Character.d },
            {'e', Character.e },{'f', Character.f },{'g', Character.g },{'h', Character.h },{'i', Character.i },{'j', Character.j },{'k', Character.k },{'l', Character.l },{'m', Character.m },{'n', Character.n },
            {'o', Character.o },{'p', Character.p },{'q', Character.q },{'r', Character.r },{'s', Character.s },{'t', Character.t },{'u', Character.u },{'v', Character.v },{'w', Character.w },{'x', Character.x },
            {'y', Character.y },{'z', Character.z },{'.', Character.Period },{'?', Character.QuestionMark },{'!', Character.ExclamationPoint },{',', Character.Comma },{'"', Character.QuotationMark },{'\'', Character.Apostrophe },
            {']', Character.RightBracket },{'[', Character.LeftBracket },{'+', Character.Plus },{'-', Character.Minus },{'=', Character.Equals },{'_', Character.Underscore },{':', Character.Colon },{';', Character.Semicolon },
            {'<', Character.LessThan },{'>', Character.GreaterThan },{'@', Character.At },{'$', Character.DollarSign },{'%', Character.Modulo },{'^', Character.Caret },{'*', Character.Asterisk },{'(', Character.LeftParenthesis },
            {')', Character.RightParenthesis },{'&', Character.And },{'|', Character.Pipe },{'\\', Character.ForwardSlash },{'/', Character.BackSlash },{'#', Character.Hash },{'~', Character.Tilde },{'`', Character.BackTick },
            {'1', Character.One },{'2', Character.Two },{'3', Character.Three },{'4', Character.Four },{'5', Character.Five },{'6', Character.Six },{'7', Character.Seven },{'8', Character.Eight },{'9', Character.Nine },
            {'0', Character.Zero }, {' ', Character.Space }, {'\n', Character.NewLine }, {'\r', Character.Return }
        };

        public static Dictionary<Character, char> _characterMapToChar = new Dictionary<Character, char>()
        {
            {Character.A, 'A' }, {Character.B, 'B' },{Character.C,'C' },{ Character.D, 'D' },{Character.E, 'E' },{Character.F, 'F' },{Character.G, 'G' },{Character.H, 'H' },{Character.I,'I' },{Character.J,'J' },
            {Character.K, 'K' },{Character.L, 'L' },{Character.M, 'M' },{Character.N, 'N' },{Character.O, 'O' },{Character.P, 'P' },{Character.Q, 'Q' },{Character.R, 'R' },{Character.S, 'S' },{Character.T, 'T' },
            {Character.U, 'U' },{Character.V, 'V' },{Character.W, 'W' },{Character.X, 'X' },{Character.Y, 'Y' },{Character.Z, 'Z' },{Character.a, 'a' },{Character.b, 'b' },{ Character.c, 'c' },{Character.d, 'd' },
            {Character.e, 'e' },{Character.f, 'f' },{Character.g, 'g' },{Character.h, 'h' },{Character.i, 'i' },{Character.j, 'j' },{Character.k, 'k' },{Character.l, 'l' },{Character.m, 'm' },{Character.n, 'n' },
            {Character.o, 'o' },{Character.p, 'p' },{Character.q, 'q' },{Character.r, 'r' },{Character.s, 's' },{Character.t, 't' },{Character.u, 'u' },{Character.v, 'v' },{Character.w, 'w' },{Character.x, 'x' },
            {Character.y, 'y' },{Character.z, 'z' },{Character.Period, '.' },{Character.QuestionMark, '?' },{Character.ExclamationPoint, '!' },{Character.Comma, ',' },{Character.QuotationMark, '"' },{Character.Apostrophe, '\'' },
            {Character.RightBracket, ']' },{Character.LeftBracket, '[' },{Character.Plus, '+' },{Character.Minus, '=' },{Character.Equals, '=' },{Character.Underscore, '_' },{Character.Colon, ':' },{Character.Semicolon, ';'},
            {Character.LessThan, '<' },{Character.GreaterThan, '>' },{Character.At, '@' },{Character.DollarSign, '$' },{Character.Modulo, '%' },{Character.Caret, '^' },{Character.Asterisk, '*' },{Character.LeftParenthesis, '(' },
            {Character.RightParenthesis, ')' },{Character.And, '&' },{Character.Pipe, '|' },{Character.ForwardSlash, '\\' },{Character.BackSlash, '/' },{Character.Hash, '#' },{Character.Tilde, '~' },{Character.BackTick, '`' },
            {Character.One, '1' },{Character.Two, '2' },{Character.Three, '3' },{Character.Four, '4' },{Character.Five, '5' },{Character.Six, '6' },{Character.Seven, '7' },{Character.Eight, '8'},{Character.Nine, '9' },
            {Character.Zero, '0' }, {Character.Space, ' ' }, {Character.NewLine, '\n' }, {Character.Return, '\r' }
        };
    }

    static class TextHelper 
    {
        public static string KeyStrokeToString(KeyboardKeyEventArgs e) 
        {
            string outStr;
            bool shift = e.Shift;

            switch (e.Key) 
            {
                case Keys.A:
                    outStr = shift ? "A" : "a";
                    break;
                case Keys.B:
                    outStr = shift ? "B" : "b";
                    break;
                case Keys.C:
                    outStr = shift ? "C" : "c";
                    break;
                case Keys.D:
                    outStr = shift ? "D" : "d";
                    break;
                case Keys.E:
                    outStr = shift ? "E" : "e";
                    break;
                case Keys.F:
                    outStr = shift ? "F" : "f";
                    break;
                case Keys.G:
                    outStr = shift ? "G" : "g";
                    break;
                case Keys.H:
                    outStr = shift ? "H" : "h";
                    break;
                case Keys.I:
                    outStr = shift ? "I" : "i";
                    break;
                case Keys.J:
                    outStr = shift ? "J" : "j";
                    break;
                case Keys.K:
                    outStr = shift ? "K" : "k";
                    break;
                case Keys.L:
                    outStr = shift ? "L" : "l";
                    break;
                case Keys.M:
                    outStr = shift ? "M" : "m";
                    break;
                case Keys.N:
                    outStr = shift ? "N" : "n";
                    break;
                case Keys.O:
                    outStr = shift ? "O" : "o";
                    break;
                case Keys.P:
                    outStr = shift ? "P" : "p";
                    break;
                case Keys.Q:
                    outStr = shift ? "Q" : "q";
                    break;
                case Keys.R:
                    outStr = shift ? "R" : "r";
                    break;
                case Keys.S:
                    outStr = shift ? "S" : "s";
                    break;
                case Keys.T:
                    outStr = shift ? "T" : "t";
                    break;
                case Keys.U:
                    outStr = shift ? "U" : "u";
                    break;
                case Keys.V:
                    outStr = shift ? "V" : "v";
                    break;
                case Keys.W:
                    outStr = shift ? "W" : "w";
                    break;
                case Keys.X:
                    outStr = shift ? "X" : "x";
                    break;
                case Keys.Y:
                    outStr = shift ? "Y" : "y";
                    break;
                case Keys.Z:
                    outStr = shift ? "Z" : "z";
                    break;
                case Keys.D1:
                    outStr = shift ? "!" : "1";
                    break;
                case Keys.D2:
                    outStr = shift ? "@" : "2";
                    break;
                case Keys.D3:
                    outStr = shift ? "#" : "3";
                    break;
                case Keys.D4:
                    outStr = shift ? "$" : "4";
                    break;
                case Keys.D5:
                    outStr = shift ? "%" : "5";
                    break;
                case Keys.D6:
                    outStr = shift ? "^" : "6";
                    break;
                case Keys.D7:
                    outStr = shift ? "&" : "7";
                    break;
                case Keys.D8:
                    outStr = shift ? "*" : "8";
                    break;
                case Keys.D9:
                    outStr = shift ? "(" : "9";
                    break;
                case Keys.D0:
                    outStr = shift ? ")" : "0";
                    break;
                case Keys.GraveAccent:
                    outStr = shift ? "~" : "`";
                    break;
                case Keys.Minus:
                    outStr = shift ? "_" : "-";
                    break;
                case Keys.Equal:
                    outStr = shift ? "+" : "=";
                    break;
                case Keys.LeftBracket:
                    outStr = shift ? "{" : "[";
                    break;
                case Keys.RightBracket:
                    outStr = shift ? "}" : "]";
                    break;
                case Keys.Backslash:
                    outStr = shift ? "|" : "\\";
                    break;
                case Keys.Semicolon:
                    outStr = shift ? ":" : ";";
                    break;
                case Keys.Apostrophe:
                    outStr = shift ? "\"" : "'";
                    break;
                case Keys.Comma:
                    outStr = shift ? "<" : ",";
                    break;
                case Keys.Period:
                    outStr = shift ? ">" : ".";
                    break;
                case Keys.Slash:
                    outStr = shift ? "?" : "/";
                    break;
                case Keys.Tab:
                    outStr = shift ? "  " : "  ";
                    break;
                case Keys.Enter:
                    outStr = "\n";
                    break;
                case Keys.KeyPadEnter:
                    outStr = "\n";
                    break;
                case Keys.KeyPad1:
                    outStr = "1";
                    break;
                case Keys.KeyPad2:
                    outStr = "2";
                    break;
                case Keys.KeyPad3:
                    outStr = "3";
                    break;
                case Keys.KeyPad4:
                    outStr = "4";
                    break;
                case Keys.KeyPad5:
                    outStr = "5";
                    break;
                case Keys.KeyPad6:
                    outStr = "6";
                    break;
                case Keys.KeyPad7:
                    outStr = "7";
                    break;
                case Keys.KeyPad8:
                    outStr = "8";
                    break;
                case Keys.KeyPad9:
                    outStr = "9";
                    break;
                case Keys.KeyPad0:
                    outStr = "0";
                    break;
                case Keys.KeyPadDecimal:
                    outStr = ".";
                    break;
                case Keys.KeyPadAdd:
                    outStr = "+";
                    break;
                case Keys.KeyPadDivide:
                    outStr = "/";
                    break;
                case Keys.KeyPadMultiply:
                    outStr = "*";
                    break;
                case Keys.KeyPadSubtract:
                    outStr = "-";
                    break;
                case Keys.Space:
                    outStr = " ";
                    break;
                default:
                    return "";
            }

            return outStr;
        }
    }
}
