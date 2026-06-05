namespace FeeloryBackend.Services.Interfaces;

public interface IDayShareAccessService
{
    /// <summary>
    /// Checks whether the user has permission to view the DayShare.
    /// </summary>
    Task<bool> CanViewDayShareAsync(Guid dayShareId, Guid requesterId);

    /// <summary>
    /// Checks whether the specified user is the owner of the DayShare.
    /// </summary>
    Task<bool> IsDayShareOwnerAsync(Guid dayShareId, Guid userId);
}