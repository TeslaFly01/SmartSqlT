using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Models
{
    public class DbObjectDTO : NotifyPropertyBase
    {
        public string ObjectId { get; set; }

        public string Name { get; set; }

        public int ObjectType { get; set; }

        public string Comment { get; set; }

        public bool IsEnable { get; set; }

        public bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    //折叠状态改变
                    _IsChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

    }
}
