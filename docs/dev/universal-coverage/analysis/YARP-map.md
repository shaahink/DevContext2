MAP  YARP     (27 projects)

STACK  net472, net9.0 · Minimal APIs · Controllers

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.8)

TOPOLOGY (depends-on)
   Yarp.ReverseProxy
   Yarp.Telemetry.Consumption ── Yarp.ReverseProxy
   Yarp.Kubernetes.Controller ── Yarp.ReverseProxy
   backend
   BasicYarpSample ── Yarp.ReverseProxy
   BenchmarkApp ── Yarp.ReverseProxy
   HttpLoadApp
   ReverseProxy.Auth.Sample ── Yarp.ReverseProxy
   ReverseProxy.Code ── Yarp.ReverseProxy, Yarp.Telemetry.Consumption
   ReverseProxy.Code.Sample ── Yarp.ReverseProxy
   ReverseProxy.Config ── Yarp.ReverseProxy
   ReverseProxy.Config.Sample ── Yarp.ReverseProxy
   ReverseProxy.ConfigFilter.Sample ── Yarp.ReverseProxy
   ReverseProxy.Direct ── Yarp.ReverseProxy
   ReverseProxy.Direct.Sample ── Yarp.ReverseProxy
   ReverseProxy.HttpSysDelegation.Sample ── Yarp.ReverseProxy
   ReverseProxy.LetsEncrypt.Sample ── Yarp.ReverseProxy
   ReverseProxy.Metrics.Prometheus.Sample ── Yarp.Telemetry.Consumption
   ReverseProxy.Metrics.Sample ── Yarp.Telemetry.Consumption
   ReverseProxy.Transforms.Sample ── Yarp.ReverseProxy
   SampleHttpSysServer
   SampleServer
   TestClient
   TestServer
   Yarp.Application
   Yarp.Kubernetes.Ingress ── Yarp.Kubernetes.Controller
   Yarp.Kubernetes.Monitor ── Yarp.Kubernetes.Controller

ENTRY POINTS
   HTTP (1)
      GET /api/dispatch  → DispatchController  (src/Kubernetes.Controller/Protocol/DispatchController.cs:25)

PACKAGES
   Web/API:  AspNetCore.HealthChecks.ApplicationStatus, Microsoft.AspNetCore.Mvc.Testing, Microsoft.AspNetCore.TestHost, OpenTelemetry.Instrumentation.AspNetCore, prometheus-net.AspNetCore
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime, Serilog.Extensions.Logging, Serilog.Formatting.Compact, Serilog.Sinks.Console
   Testing:  Autofac.Extras.Moq, coverlet.collector, Microsoft.DotNet.XUnitV3Extensions, Moq, xunit.v3.assert, xunit.v3.extensibility.core
   Utilities:  Newtonsoft.Json, Polly
   Other:  Autofac, Drop.App, JsonSchema.Net, KubernetesClient, LettuceEncrypt, Microsoft.Crank.EventSources, Microsoft.DotNet.IBCMerge, Microsoft.Extensions.ServiceDiscovery … (15 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
