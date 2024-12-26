using Elsa.Studio.Branding;
using Elsa.Studio.Components;
using Elsa.Studio.Contracts;
using Elsa.Studio.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Elsa.Studio.Layouts;

/// <summary>
/// The main layout for the application.
/// </summary>
public partial class MainLayout : IDisposable
{
    private bool _drawerOpen = true;
    private ErrorBoundary? _errorBoundary;

    [Inject] private IThemeService ThemeService { get; set; } = default!;
    [Inject] private IAppBarService AppBarService { get; set; } = default!;
    [Inject] private IUnauthorizedComponentProvider UnauthorizedComponentProvider { get; set; } = default!;
    [Inject] private IFeatureService FeatureService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IBrandingProvider BrandingProvider { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }
    private MudTheme CurrentTheme => ThemeService.CurrentTheme;
    private bool IsDarkMode => ThemeService.IsDarkMode;
    private bool IsLogin { get; set; }
    private bool IsUserLoggedIn { get; set; }
    private string UserName { get; set; } = string.Empty;
    private RenderFragment UnauthorizedComponent => UnauthorizedComponentProvider.GetUnauthorizedComponent();
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        ThemeService.CurrentThemeChanged += OnThemeChanged;
        AppBarService.AppBarItemsChanged += OnAppBarItemsChanged;
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState != null)
        {
            var authState = await AuthenticationState;
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                UserName = authState.User?.Identity?.Name ?? "";
                IsLogin = true;
            }
            if (authState.User?.Identity?.IsAuthenticated == true && !authState.User.Claims.IsExpired())
            {

                await FeatureService.InitializeFeaturesAsync();
                StateHasChanged();
            }
        }
        else
        {
            await FeatureService.InitializeFeaturesAsync();
            StateHasChanged();
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        _errorBoundary?.Recover();
    }

    private void OnThemeChanged() => StateHasChanged();
    private void OnAppBarItemsChanged() => InvokeAsync(StateHasChanged);

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void ToggleDarkMode()
    {
        ThemeService.IsDarkMode = !ThemeService.IsDarkMode;
    }

    private async Task ShowProductInfo()
    {
        await DialogService.ShowAsync<ProductInfoDialog>($"Elsa Studio {ToolVersion.GetDisplayVersion()}", new DialogOptions
        {
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraSmall,
            CloseButton = true,
            CloseOnEscapeKey = true
        });
    }

    private void Login()
    {
        Navigation.NavigateTo("Signin", true);
        
    }

    private void Logout()
    {
        Navigation.NavigateTo("Signout", true);
    }

    void IDisposable.Dispose()
    {
        ThemeService.CurrentThemeChanged -= OnThemeChanged;
    }
}