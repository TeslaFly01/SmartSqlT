using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Util
{
    public class SingletonConstructor<T> where T : class, new()
    {
        private static T _Instance = null;
        private readonly static object _lockObj = new object();

        /// <summary>
        /// 获取单例对象的实例
        /// </summary>
        /// <returns></returns>
        public static T GetInstance()
        {
            if (_Instance != null) return _Instance;
            lock (_lockObj)
            {
                if (_Instance == null)
                {
                    var item = Activator.CreateInstance<T>();
                    System.Threading.Interlocked.Exchange(ref _Instance, item);
                }
            }
            return _Instance;
        }

    }
}
