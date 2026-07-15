namespace Api.Ui;

/// <summary>T1.7 negative case — a plain MVVM base (NOT the nested <c>Service.ServiceBase</c> form
/// protoc generates). Its <c>public override</c> method must NEVER be catalogued as a gRPC RPC: the
/// old "any *Base" heuristic wrongly turned every eShop ClientApp ViewModel into a gRPC service.</summary>
public abstract class ViewModelBase
{
    public virtual Task RefreshAsync() => Task.CompletedTask;
}

public sealed class WeatherViewModel : ViewModelBase
{
    public override Task RefreshAsync() => Task.CompletedTask;
}
