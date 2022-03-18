using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.Lang;

namespace SmartCode.Framework
{
    public class LangFactory
    {
        public static Lang.Lang CreateInstance(LangType type)
        {
            switch (type)
            {
                case LangType.Csharp: return new CsharpLang();
                case LangType.Java: return new JavaLang();
                default: return new CsharpLang();
            }
        }
    }
}
