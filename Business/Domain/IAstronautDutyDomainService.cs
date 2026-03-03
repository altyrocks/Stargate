using StargateAPI.Business.Data;

public interface IAstronautDutyDomainService
{
    Task<AstronautDuty> CreateDutyAsync(
        int personId,
        string rank,
        string title,
        DateTime startDate,
        CancellationToken cancellationToken);
}