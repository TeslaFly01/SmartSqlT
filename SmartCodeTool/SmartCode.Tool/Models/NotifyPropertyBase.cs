using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Tool.Annotations;

namespace SmartCode.Tool.Models
{
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    
    /// <summary>
    /// 扩展方法
    /// 避免硬编码问题
    /// </summary>
    public static class NotifyPropertyBaseEx
    {
        public static void SetProperty<T, U>(this T tvm, Expression<Func<T, U>> expre) where T : NotifyPropertyBase, new()
        {
            string _pro = CommonFun.GetPropertyName(expre);
            tvm.OnPropertyChanged(_pro);
        }//为什么扩展方法必须是静态的
    }
    public class CommonFun
    {
        /// <summary>
        /// 返回属性名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetPropertyName<T, U>(Expression<Func<T, U>> expr)
        {
            string _propertyName = "";
            if (expr.Body is MemberExpression)
            {
                _propertyName = (expr.Body as MemberExpression).Member.Name;
            }
            else if (expr.Body is UnaryExpression)
            {
                _propertyName = ((expr.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            }
            return _propertyName;
        }
    }
}
