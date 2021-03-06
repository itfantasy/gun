﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun
{
    public class error
    {
        private string text;
        public int errcode { get; set; }

        public bool nil
        {
            get
            {
                return text == String.Empty;
            }
        }

        private object token { get; set; }

        public error(string text)
        {
            this.text = text;
            if(this.text == String.Empty)
            {
                errcode = 0;
            }
            else
            {
                errcode = 1;
            }
        }

        public string Error()
        {
            return this.text;
        }

        public T Result<T>()
        {
            return (T)this.token;
        }

        public error AttachResult(object result)
        {
            this.token = result;
            return this;
        }
    }

    public class errors
    {
        public static error nil
        {
            get
            {
                return new error(String.Empty);
            }
        }

        public static error New(string text)
        {
            return new error(text);
        }
    }
}

