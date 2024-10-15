using GULP.Entities;

namespace GULP.Graphics.Interface;

public interface IInterface : IEntity
{
    public bool IsOpen { get; }
    public bool Open();
    public bool Close();
}