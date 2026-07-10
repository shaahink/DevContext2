import Prism from 'prismjs';
import 'prismjs/components/prism-csharp';

export function highlightCSharp(code: string): string {
  try {
    return Prism.highlight(code, Prism.languages['csharp'], 'csharp');
  } catch {
    return code;
  }
}
