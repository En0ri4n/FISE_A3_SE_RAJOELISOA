using System.ComponentModel;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel;

public interface IViewModelObjectBuilder<TJob> : INotifyPropertyChanged where TJob : IJob
{
    /// <summary>
    /// Clears the current state of the builder.
    /// </summary>
    void Clear();

    /// <summary>
    /// Copies the state of the given job into the builder.
    /// This is useful for updating the builder with an existing job's properties.
    /// </summary>
    void GetFrom(TJob job);
    
    /// <summary>
    /// Builds the job object from the current state of the builder.
    /// Can be used to create a new job or update an existing one.
    /// Clear() should be called after using this method to ensure a clean state.
    /// </summary>
    TJob Build();
}