using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Framework.SqliteModel;

namespace SmartCode.Tool.ViewModels
{
    class GroupManageViewModel : ViewModelBase
    {
        #region file
        private string _connectionKey = null;
        private string _title = null;
        private List<ObjectGroup> _groupList = null;

        public string ConnectionKey
        {
            get => _connectionKey;
            private set
            {
                if (_connectionKey != null)
                {
                    _connectionKey = value;
                    RaisePropertyChanged(nameof(ConnectionKey));
                }
            }
        }
        public string Title
        {
            get => _title;
            private set
            {
                if (_title != null)
                {
                    _title = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }
        public List<ObjectGroup> GroupList
        {
            get => _groupList;
            private set
            {
                if (_groupList != null)
                {
                    _groupList = value;
                    RaisePropertyChanged(nameof(GroupList));
                }
            }
        } 
        #endregion
    }
}
