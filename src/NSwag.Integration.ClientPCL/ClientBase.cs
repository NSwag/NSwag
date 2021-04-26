using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NSwag.Integration.ClientPCL
{
    public class ClientBase
    {
        protected Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpClient());
        }

        protected Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpRequestMessage());
        }

        protected void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {

        }
    }
}
