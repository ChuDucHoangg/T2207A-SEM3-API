using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Service.Tests
{
    public interface ITestService
    {
        Task<Test> TestExists(string test_slug);
    }
}
