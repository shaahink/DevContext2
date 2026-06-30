import type { VibeDefinition } from '../vibe-definition';
import { hacker } from './hacker';
import { modern } from './modern';
import { terminal } from './terminal';

export const VIBES: readonly VibeDefinition[] = [modern, terminal, hacker] as const;

export function getVibe(id: string): VibeDefinition | undefined {
  return VIBES.find((v) => v.id === id);
}
