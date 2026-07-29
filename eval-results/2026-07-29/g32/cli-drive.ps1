# G3.2 (R4 §1 item 9) — kind-filtered neighbours, driven over the REAL CLI on a REAL repo.
#
# The point of driving the CLI as well as the MCP: the CLI is a second, independent client of the
# same GraphQuery.NeighborsView. If the two disagree, one of them is rendering its own idea of the
# answer — which is the class of defect this whole strand keeps finding.
#
# SUBJECTS ARE CHOSEN FROM THE GRAPH, NOT FROM MEMORY. The first run of this script picked
# OrderItem and OrderingContext-out and got totalEdges=0 from all of them — not a bug, just badly
# chosen nodes (OrderingContext is a DbContext: outDegree 0, inDegree 43). The subjects below come
# from `query graphdump` (graphdump.json), which says where the 57 ReadsWrites edges actually are.
#
# OP IS POSITIONAL (`query neighbors`, never `query --op neighbors`) and every path is ABSOLUTE.
$ErrorActionPreference = 'Continue'
$cli  = 'C:/code/DevContext2/src/DevContext.Cli/bin/Debug/net10.0/DevContext.Cli.exe'
$repo = 'C:/code/DevContext2/eval-repos/eShop'
$out  = 'C:/code/DevContext2/eval-results/2026-07-29/g32'

function Run($name, $cliArgs) {
  Write-Host "=== $name ==="
  Write-Host "    devcontext $($cliArgs -join ' ')"
  # -Encoding utf8 rather than `>`: PowerShell 5.1's redirect writes UTF-16, which every JSON
  #  reader downstream then chokes on.
  & $cli @cliArgs 2>$null | Out-File -FilePath "$out/$name.json" -Encoding utf8
  Write-Host "    exit=$LASTEXITCODE  ->  $name.json"
}

# 1. THE ROLL-UP, on real data. CatalogApi is a Type whose ReadsWrites edges ALL hang off its
#    members (GetAllItems, GetItemById, ...). A filter applied to the Type node's own edges answers
#    "this API touches no model" — see RED-direct-edges.txt for that exact failure in the unit suite.
Run 'cli-01-catalogapi-out-all'    @('query','neighbors','--path',$repo,'--focus','eShop.Catalog.API.CatalogApi','--direction','out')
Run 'cli-02-catalogapi-out-writes' @('query','neighbors','--path',$repo,'--focus','eShop.Catalog.API.CatalogApi','--direction','out','--kind','ReadsWrites')

# 2. The headline question of item 9, in the IN direction: WHO WRITES THIS ENTITY.
Run 'cli-03-order-in-all'    @('query','neighbors','--path',$repo,'--focus','eShop.ClientApp.Models.Orders.Order','--direction','in')
Run 'cli-04-order-in-writes' @('query','neighbors','--path',$repo,'--focus','eShop.ClientApp.Models.Orders.Order','--direction','in','--kind','ReadsWrites')

# 3. The two dead ends, which must NOT read alike:
#    a valid kind that matched nothing — the rows go empty, kindsPresent still says what to ask
Run 'cli-05-catalogapi-out-nomatch' @('query','neighbors','--path',$repo,'--focus','eShop.Catalog.API.CatalogApi','--direction','out','--kind','Consumes')
#    ...and a kind that is not a kind at all — refused WITH the vocabulary, never silently unfiltered
Run 'cli-06-catalogapi-out-unknown' @('query','neighbors','--path',$repo,'--focus','eShop.Catalog.API.CatalogApi','--direction','out','--kind','writes')

# 4. A node with genuinely nothing in this direction reads differently again (0 of 0, no kinds).
Run 'cli-07-orderingcontext-out-empty' @('query','neighbors','--path',$repo,'--focus','eShop.Ordering.Infrastructure.OrderingContext','--direction','out')

Write-Host "DONE"
