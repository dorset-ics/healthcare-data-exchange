namespace Infrastructure.Common.Authentication;

public interface ITokenFactory
{
    public Task<string> GetAccessToken();
}