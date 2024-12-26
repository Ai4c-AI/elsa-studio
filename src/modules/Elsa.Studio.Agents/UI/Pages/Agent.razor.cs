using Elsa.Studio.Agents.Client;
using Elsa.Studio.Agents.Models;
using Elsa.Studio.Agents.UI.Validators;
using Elsa.Studio.Components;
using Elsa.Studio.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace Elsa.Studio.Agents.UI.Pages;

public partial class Agent : StudioComponentBase
{
    /// The ID of the agent to edit.
    [Parameter] public string AgentId { get; set; } = default!;

    [Inject] private IBackendApiClientProvider ApiClientProvider { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private bool UseJsonResponse
    {
        get => _agent.ExecutionSettings.ResponseFormat == "json_object";
        set => _agent.ExecutionSettings.ResponseFormat = value ? "json_object" : "string";
    }

    private ICollection<object> AvailableServices { get; set; } = [];
    private IReadOnlyCollection<string> SelectedServices { get; set; } = [];

    private ICollection<object> AvailablePlugins { get; set; } = [];
    private IReadOnlyCollection<string> SelectedPlugins { get; set; } = [];

    private ICollection<string> Providers { get; set; } = [];

    private ICollection<LlmModelSetting> LlmProviderModels { get; set; } = [];

    private MudForm _form = default!;
    private AgentInputModelValidator _validator = default!;
    private AgentModel _agent = new();
    private AgentInputVariableConfig? _inputVariableBackup;
    private MudTable<AgentInputVariableConfig> _inputVariableTable;

    private AgentOutputVariableConfig? _outputVariableBackup;
    private MudTable<AgentOutputVariableConfig> _outputVariableTable;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var apiClient = await ApiClientProvider.GetApiAsync<IAgentsApi>();
        _validator = new AgentInputModelValidator(apiClient);
        var modelProviderApi = await ApiClientProvider.GetApiAsync<IModelProviderApi>();
        Providers = await modelProviderApi.GetLlmProvidersAsync();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        var apiClient = await ApiClientProvider.GetApiAsync<IAgentsApi>();
        _agent = await apiClient.GetAsync(AgentId);
        var modelProviderApi = await ApiClientProvider.GetApiAsync<IModelProviderApi>();
        var provider = _agent?.AgentLlmConfig?.Provider;
        LlmProviderModels = await modelProviderApi.GetLlmProviderModelsAsync(string.IsNullOrEmpty(provider) ? Providers.FirstOrDefault() ?? "azure-openai" : provider);
    }

    private async Task OnSaveClicked()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;
        var apiClient = await ApiClientProvider.GetApiAsync<IAgentsApi>();
        _agent = await apiClient.UpdateAsync(AgentId, _agent);
        Snackbar.Add("Agent successfully updated.", Severity.Success);
        StateHasChanged();
    }

    private void OnAddInputVariableClicked()
    {
        var newInputVariable = new AgentInputVariableConfig
        {
            Name = "Variable1",
            Type = "string"
        };

        _agent.InputVariables.Add(newInputVariable);

        // Need to do it this way, otherwise MudTable doesn't show the item in edit mode.
        _ = Task.Delay(1).ContinueWith(_ =>
        {
            InvokeAsync(() =>
            {
                _inputVariableTable.SetEditingItem(newInputVariable);
                StateHasChanged();
            });
        });
    }

    private void BackupInputVariable(object obj)
    {
        var inputVariable = (AgentInputVariableConfig)obj;
        _inputVariableBackup = new AgentInputVariableConfig
        {
            Name = inputVariable.Name,
            Type = inputVariable.Type,
            Description = inputVariable.Description
        };
    }

    private void RestoreInputVariable(object obj)
    {
        var inputVariable = (AgentInputVariableConfig)obj;
        inputVariable.Name = _inputVariableBackup!.Name;
        inputVariable.Type = _inputVariableBackup.Type;
        inputVariable.Description = _inputVariableBackup.Description;
        _inputVariableBackup = null;
    }

    private void CommitInputVariable(object obj)
    {
        _inputVariableBackup = null;
    }

    private Task DeleteInputVariable(AgentInputVariableConfig item)
    {
        _agent.InputVariables.Remove(item);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void OnAddOutputVariableClicked()
    {
        var newOutputVariable = new AgentOutputVariableConfig
        {
            Name = "Variable1",
            Type = "string"
        };

        _agent.OutputVariables.Add(newOutputVariable);

        // Need to do it this way, otherwise MudTable doesn't show the item in edit mode.
        _ = Task.Delay(1).ContinueWith(_ =>
        {
            InvokeAsync(() =>
            {
                _outputVariableTable.SetEditingItem(newOutputVariable);
                StateHasChanged();
            });
        });
    }

    private void BackupOutputVariable(object obj)
    {
        var outputVariable = (AgentOutputVariableConfig)obj;
        _outputVariableBackup = new AgentOutputVariableConfig
        {
            Name = outputVariable.Name,
            Type = outputVariable.Type,
            Description = outputVariable.Description
        };
    }

    private void RestoreOutputVariable(object obj)
    {
        var outputVariable = (AgentOutputVariableConfig)obj;
        outputVariable.Name = _outputVariableBackup!.Name;
        outputVariable.Type = _outputVariableBackup.Type;
        outputVariable.Description = _outputVariableBackup.Description;
        _outputVariableBackup = null;
    }

    private void CommitOutputVariable(object obj)
    {
        _outputVariableBackup = null;
    }

    private Task DeleteOutputVariable(AgentOutputVariableConfig item)
    {
        _agent.OutputVariables.Remove(item);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnProviderChanged(string newValue)
    {
        var modelProviderApi = await ApiClientProvider.GetApiAsync<IModelProviderApi>();
        var provider = _agent?.AgentLlmConfig?.Provider;
        LlmProviderModels = await modelProviderApi.GetLlmProviderModelsAsync(string.IsNullOrEmpty(provider)? Providers.FirstOrDefault() ?? "azure-openai" : provider);
        if (_agent != null)
        {
            _agent.AgentLlmConfig.Model = LlmProviderModels.FirstOrDefault()?.Name;
        } 
    }
}