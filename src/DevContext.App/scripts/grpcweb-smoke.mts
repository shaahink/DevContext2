// Live smoke for the gRPC-Web transport: drives the real server with the same connect-web client the
// Angular app uses. Proves Ping + server-streaming Analyze + Map/Entries/Trace over gRPC-Web.
// Run with the server up:  node --experimental-strip-types scripts/grpcweb-smoke.mts
import { resolve } from 'node:path';

import { createClient } from '@connectrpc/connect';
import { createGrpcWebTransport } from '@connectrpc/connect-web';

import { DevContextService } from '../src/app/core/grpc/gen/devcontext/v1/devcontext_pb.ts';

const baseUrl = process.env.DEVCONTEXT_SERVER ?? 'http://127.0.0.1:5179';
const fixture = resolve('../../tests/fixtures/ControllerApp');

const client = createClient(DevContextService, createGrpcWebTransport({ baseUrl }));

const pong = await client.ping({});
console.log(`PING ready=${pong.ready} version=${pong.version}`);
if (!pong.ready) throw new Error('server not ready');

let handle: string | undefined;
let stages = 0;
for await (const evt of client.analyze({ path: fixture, noRoslyn: false })) {
  if (evt.event.case === 'progress') stages++;
  if (evt.event.case === 'result') handle = evt.event.value.handle;
  if (evt.event.case === 'error') throw new Error(`analyze error: ${evt.event.value.message}`);
}
if (!handle) throw new Error('no handle from analyze');
console.log(`ANALYZE handle=${handle.slice(0, 8)}… streamedStages=${stages}`);

const map = await client.getMap({ handle });
console.log(`MAP style=${map.style} projects=${map.projectCount} chars=${map.markdown.length}`);

const entries = await client.listEntryPoints({ handle });
console.log(`ENTRIES count=${entries.entryPoints.length}`);

const trace = await client.getTrace({ handle, focus: 'GET /api/Products', depth: 4 });
const titles: string[] = [];
const walk = (n: { title: string; children: { title: string; children: unknown[] }[] }): void => {
  titles.push(n.title);
  for (const c of n.children) walk(c as never);
};
if (trace.found && trace.root) walk(trace.root as never);
console.log(`TRACE found=${trace.found} nodes=${titles.length} -> ${titles.join(' / ')}`);
if (!trace.found || !titles.some((t) => t.includes('ProductService'))) {
  throw new Error('trace did not reach ProductService');
}

console.log('GRPC-WEB SMOKE OK');
