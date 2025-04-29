namespace NashTech_TCG_API.Utilities.Interfaces
{
    public interface IIdGenerator
    {
        Task<string> GenerateId(string prefix);
    }
}
