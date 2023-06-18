using XD.XFinder.Lang.Base;

#region ReSharper disable
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberCanBePrivate.Global
#endregion

namespace XD.XFinder.Lang;

internal static class Units
{
    internal static readonly IMatcher Keyword = new KeyWord();

    #region MatcherDef

    internal class KeyWord : IMatcher
    {
        internal enum Info : byte
        {
            None = 0,
            From,
            Find,
            Col,
            Row,
            As,
            Group,
            DeGroup,
            Del
        }

        public int MatcherType => MatcherEnum.KeyWord.Int();
        public (IResult res, StrSlice left) Match(in StrSlice str)
        {
            var r = IMatcher.NewResultInst();
            r.Type = MatcherType;

            var sp = MatcherUtil.TrimHead(str);
            if (sp.Length == 0) return (r, str);

            var i = 0;
            while (i < sp.Length && char.IsLetter(sp[i])) i++;
            if (i == 0) return (r, str);

            var left = i == sp.Length ? default : new StrSlice(sp, i, int.MaxValue);
            var word = (sp = new StrSlice(sp, i)).ToString().ToLower();

            switch (word)
            {
                // ReSharper disable StringLiteralTypo
                case "from": SetResult(Info.From, sp); break;
                case "find": SetResult(Info.Find, sp); break;
                case "col": SetResult(Info.Col, sp); break;
                case "row": SetResult(Info.Row, sp); break;
                case "as": SetResult(Info.As, sp); break;
                case "group": SetResult(Info.Group, sp); break;
                case "degroup": SetResult(Info.DeGroup, sp); break;
                case "del": SetResult(Info.Del, sp); break;
                // ReSharper restore StringLiteralTypo
                default:
                    r.Info = null;
                    r.Content = sp;
                    r.Distance = 128;
                    break;
            }

            return (r, left);
            void SetResult(Info wt, in StrSlice strSpan)
            {
                r.Info = wt;
                r.Content = strSpan;
                r.Distance = IResult.FullDistance;
            }
        }
    }

    internal class Bracket : IMatcher
    {
        internal const char EscapeChar = '~';
        internal const char BracketCharBeg = '<';
        internal const char BracketCharEnd = '>';

        internal class Info
        {
            internal ushort BitFlag;
            internal string? Content;
        }

        public int MatcherType => MatcherEnum.Bracket.Int();
        public (IResult res, StrSlice left) Match(in StrSlice str)
        {
            var r = IMatcher.NewResultInst();
            r.Type = MatcherType;

            var sp = MatcherUtil.TrimHead(str);
            if (sp.Count < 2 || sp[0] != BracketCharBeg) return (r, str);

            var isFoundEnd = false;
            var isEscaping = false;
            var i = 1;
            for (; i < sp.Length; i++)
            {
                var c = sp[i];
                if (isEscaping)
                {
                    isEscaping = false;
                    continue;
                }
                if (c == EscapeChar)
                {
                    isEscaping = true;
                    continue;
                }

                if (c != BracketCharEnd) continue;
                isFoundEnd = true;
                break;
            }

            if (!isFoundEnd)
            {
                r.Content = sp;
                r.Distance = 200;
                return (r, str);
            }

            throw new NotImplementedException();
        }

        private ushort IsFlag(in char c)
        {
            // 0000 0000 0000 0000
            // +-*/ !?^% @#$% `&|\
            for (var i = 0; i < _flagBit.Length; i++)
                if (_flagBit[i] == c) return (ushort)(0x8000 >> i);
            return 0;
        }

        private readonly char[] _flagBit =
        {
            '+', '-', '*', '/',
            '!', '?', '^', '%',
            '@', '#', '$', '%',
            '`', '&', '|', '\\'
        };
    }

    internal class TxtContent : IMatcher
    {
        internal class Info
        {
            public int[]? EscapeCharIndex;
        }

        public int MatcherType => MatcherEnum.TxtContent.Int();
        public (IResult res, StrSlice left) Match(in StrSlice str)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}