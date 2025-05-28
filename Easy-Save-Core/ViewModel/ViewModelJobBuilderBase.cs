using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel
{
    public abstract class ViewModelJobBuilderBase : INotifyPropertyChanged
    {
        private string _initialName = string.Empty;

        public string InitialName
        {
            get => _initialName;
            protected set
            {
                _initialName = value;
                OnPropertyChanged();
            }
        }
        
        public abstract string Name { get; set; }
        public abstract string Source { get; set; }
        public abstract string Target { get; set; }
        public abstract JobExecutionStrategy.StrategyType StrategyType { get; set; }
        public abstract bool IsEncrypted { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Clears the current state of the builder.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        ///     Copies the state of the given job into the builder.
        ///     This is useful for updating the builder with an existing job's properties.
        /// </summary>
        public abstract void GetFrom(IJob job);

        /// <summary>
        ///     Builds the job object from the current state of the builder.
        ///     Can be used to create a new job or update an existing one.
        ///     Clear() should be called after using this method to ensure a clean state.
        /// </summary>
        public abstract IJob Build(bool clear = true);

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}