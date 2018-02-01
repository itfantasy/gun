using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun
{
    public class error
    {
        private string text;

        public error(string text)
        {
            this.text = text;
        }

        public string Error()
        {
            return this.text;
        }
    }

    public class errors
    {
        public static error nil = new error("nil");

        public static error New(string text)
        {
            return new error(text);
        }
    }
}

