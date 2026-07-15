MAP  unknown     (1 project)

STACK  net10.0 · Minimal APIs

STYLE  MinimalApi  (confidence high)
       evidence: Minimal APIs + 1 project(s); no MediatR

       per service:
         SignalRApp: Unknown

TOPOLOGY (depends-on)
   SignalRApp

ENTRY POINTS
   SignalR (1)
      ChatHub (2 methods: SendMessage, JoinGroup)  → ChatHub  (Hubs/ChatHub.cs:5)

PACKAGES
   Web/API:  Microsoft.AspNetCore.SignalR 1.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
