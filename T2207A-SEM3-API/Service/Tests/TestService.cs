using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Service.Tests
{
    public class TestService : ITestService
    {
        private readonly ExamonimyContext _context;

        public TestService(ExamonimyContext context)
        {
            _context = context;
        }
        public async Task<Test> TestExists(string test_slug)
        {
            return await _context.Tests.SingleOrDefaultAsync(t => t.Slug == test_slug);
        }
    }
}
