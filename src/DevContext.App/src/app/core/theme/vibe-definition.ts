export interface VibeDefinition {
  readonly id: string;
  readonly name: string;
  readonly themes: readonly string[];
  readonly defaultTheme: string;
}
