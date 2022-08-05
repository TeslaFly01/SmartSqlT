using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using FontAwesome.WPF;
using SmartSQL.Annotations;

namespace SmartSQL.Views
{
    public partial class Fontawesome : INotifyPropertyChanged
    {
        public static readonly DependencyProperty IconListProperty = DependencyProperty.Register(
            "IconList", typeof(List<FIcon>), typeof(Fontawesome), new PropertyMetadata(default(List<FIcon>)));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<FIcon> IconList
        {
            get => (List<FIcon>)GetValue(IconListProperty);
            set
            {
                SetValue(IconListProperty, value);
                OnPropertyChanged(nameof(IconList));
            }
        }
        public Fontawesome()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Fontawesome_OnLoaded(object sender, RoutedEventArgs e)
        {
            IconList = new List<FIcon>();
            IconList.Clear();
            foreach (var value in Enum.GetValues(typeof(FontAwesomeIcon)))
            {
                if (value.ToString().Equals("None"))
                {
                    continue;
                }
                //var memInfo = typeof(FontAwesomeIcon).GetMember(value.ToString());
                //var attributes = memInfo[0].GetCustomAttributes(typeof(IconIdAttribute), false);

                //if (attributes.Length == 0) continue; // alias

                //var id = ((IconIdAttribute)attributes[0]).Id;
                IconList.Add(new FIcon
                {
                    Name = value.ToString()
                });

            }
        }
    }

    public class FIcon
    {
        public string Name { get; set; }
    }
}
