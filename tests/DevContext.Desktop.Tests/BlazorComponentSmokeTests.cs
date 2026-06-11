using DevContext.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using DevContext.Desktop.Services;

namespace DevContext.Desktop.Tests;

public class BlazorComponentSmokeTests
{
    [Fact]
    public void App_component_type_resolves()
    {
        // Verify the App component type can be created
        var type = typeof(App);
        Assert.NotNull(type);
        Assert.True(type.IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase)));
    }

    [Fact]
    public void All_blazor_components_exist()
    {
        Assert.NotNull(typeof(Components.ConfigPanel));
        Assert.NotNull(typeof(Components.OutputPanel));
        Assert.NotNull(typeof(Components.StatusBar));
    }

    [Fact]
    public void MainViewModel_registers_in_di()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<MainViewModel>();
        var sp = services.BuildServiceProvider();
        var vm = sp.GetRequiredService<MainViewModel>();
        Assert.NotNull(vm);
        Assert.NotNull(vm.Scenarios);
        Assert.NotEmpty(vm.Scenarios);
    }
}
