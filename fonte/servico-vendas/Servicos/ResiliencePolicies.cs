using System.Net;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace ServicoVendas.Servicos;

public static class ResiliencePolicies
{
    // Retry exponencial com jitter para falhas transitórias (5xx, timeouts, etc.) e 429
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1))
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100))
            );

    // Circuit breaker simples para evitar tempestade de requisições quando o serviço-alvo está instável
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(10)
            );
}
