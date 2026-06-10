namespace DevContext.Core.Tests;

public sealed class DiscoveryContextBuilder
{
    private FakeFileSystem _fs = new();
    private FakeAnalysisCache? _cache;
    private SharedAnalysisContext _analysis = new();
    private ExtractionOptions _options = new();
    private Scenario _scenario = ScenarioRegistry.BuiltIn["architecture"];
    private IDiscoveryObserver _observer = new NullDiscoveryObserver();
    private ILogger _logger = new NullLogger<DiscoveryContext>();
    private IRoslynWorkspaceProvider _roslyn = new MockRoslynProvider();
    private string _rootPath = "";
    private readonly List<FeatureSignal> _signals = [];
    private bool _sealSignals;

    public DiscoveryContextBuilder WithFileSystem(FakeFileSystem fs) { _fs = fs; return this; }
    public DiscoveryContextBuilder WithRootPath(string path) { _rootPath = path; return this; }
    public DiscoveryContextBuilder WithOptions(ExtractionOptions opts) { _options = opts; return this; }
    public DiscoveryContextBuilder WithScenario(Scenario scenario) { _scenario = scenario; return this; }
    public DiscoveryContextBuilder WithObserver(IDiscoveryObserver observer) { _observer = observer; return this; }
    public DiscoveryContextBuilder WithSignal(FeatureSignal signal) { _signals.Add(signal); return this; }
    public DiscoveryContextBuilder WithSealedSignals() { _sealSignals = true; return this; }

    public DiscoveryContext Build()
    {
        _cache ??= new FakeAnalysisCache(_fs);

        var ctx = new DiscoveryContext
        {
            RootPath = _rootPath,
            Options = _options,
            ActiveScenario = _scenario,
            Observer = _observer,
            FileSystem = _fs,
            Cache = _cache,
            Analysis = _analysis,
            Logger = _logger,
            RoslynWorkspace = _roslyn
        };

        return ctx;
    }

    public (DiscoveryContext Context, RecordingDiscoveryObserver Recording) BuildWithRecording()
    {
        var recording = new RecordingDiscoveryObserver();
        _observer = recording;
        return (Build(), recording);
    }

    public IReadOnlyList<FeatureSignal> Signals => _signals;
    public bool SealSignals => _sealSignals;
}
