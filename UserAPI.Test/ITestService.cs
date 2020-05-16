using System;
using System.Threading.Tasks;

namespace UserAPI.Test
{
    public interface ITestService
    {
        Task RunAsync(Uri serviceUrl, string host);
    }
}