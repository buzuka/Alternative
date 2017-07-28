using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Alternative.Expression
{
    public class ArgumentMissingException : Exception
    {
        public ArgumentMissingException(string message)
            : base(message)
        {
        }
    }

    public interface IXslBinder
    {
        XElement Bind(XElement el);
    }

    public class ValueOfBinder : IXslBinder
    {
        public ValueOfBinder(IExpression exp)
        {
            _exp = exp;
        }

        public XElement Bind(XElement el)
        {
            el.Add(new XElement(Namespaces.xsl + "value-of"
                , new XAttribute("select", _exp.AsString())
                ));
            return el;
        }

        private IExpression _exp;
    }




    public interface IExpression
    {
        string AsString();
    }

    public class StringExpression : IExpression
    {
        public StringExpression(string v)
        {
            _value = v;
        }

        public string AsString()
        {
            return "'" + _value + "'";
        }

        private string _value;
    }

    public class IntegerExpression : IExpression
    {
        public IntegerExpression(int v)
        {
            _value = v;
        }

        public string AsString()
        {
            return _value.ToString();
        }

        private int _value;
    }

    public class LinkExpression : IExpression
    {
        public LinkExpression(string link)
        {
            _link = link;
        }

        public string AsString()
        {
            return _link;
        }

        private string _link;
    }

    public class ConcatExpression : IExpression
    {
        public ConcatExpression(params IExpression[] exps)
        {
            _exps = exps;
        }

        public string AsString()
        {
            if (_exps.Length < 2)
            {
                throw new ArgumentMissingException("concat function requires at least 2 arguments");
            }
            string result = "concat(";
            foreach (IExpression e in _exps)
            {
                if (!e.Equals(_exps[0]))
                {
                    result += ", ";
                }
                result += e.AsString();
            }
            result += ")";
            return result;
        }

        private IExpression[] _exps;
    }
}
