namespace StargateAPI.Business.Services
{
    public interface ILogService
    {
        Task InfoAsync(
            string source,
            string message,
            string? details = null,
            CancellationToken cancellationToken = default);

        Task ErrorAsync(
            string source,
            string message,
            string? details = null,
            CancellationToken cancellationToken = default);
    }
}