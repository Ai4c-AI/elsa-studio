using Elsa.Studio.Agents.Models;
using Refit;

namespace Elsa.Studio.Agents.Client;

/// <summary>
/// 
/// </summary>
public interface IModelProviderApi
{
    /// Lists all providers.
    [Get("/ai/llm-providers")]
    Task<ICollection<string>> GetLlmProvidersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/ai/llm-provider/{provider}/models")]
    Task<ICollection<LlmModelSetting>> GetLlmProviderModelsAsync(string provider, CancellationToken cancellationToken = default);

}
