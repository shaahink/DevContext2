import { InjectionToken } from '@angular/core';
import { createClient, type Client } from '@connectrpc/connect';
import { createGrpcWebTransport } from '@connectrpc/connect-web';

import { serverBaseUrl } from '../config';
import { DevContextService } from './gen/devcontext/v1/devcontext_pb';

/** Strongly-typed connect client for the DevContext gRPC service. */
export type DevContextClient = Client<typeof DevContextService>;

/**
 * The single connect-web client, talking gRPC-Web to the local server. Provided as a root token so
 * data-access services inject it (and tests can override it) — no client construction leaks into the
 * data-access or feature layers.
 */
export const DEVCONTEXT_CLIENT = new InjectionToken<DevContextClient>('DEVCONTEXT_CLIENT', {
  providedIn: 'root',
  factory: () => createClient(DevContextService, createGrpcWebTransport({ baseUrl: serverBaseUrl() })),
});
