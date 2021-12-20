using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartCode.Tool.Annotations;

namespace SmartCode.Tool.ViewModels
{
	class ViewModelBase : INotifyPropertyChanged
	{

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
