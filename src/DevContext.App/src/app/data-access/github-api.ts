export interface GitHubRepo {
  readonly id: number;
  readonly fullName: string;
  readonly htmlUrl: string;
  readonly cloneUrl: string;
  readonly description: string | null;
  readonly stargazersCount: number;
  readonly language: string | null;
  readonly topics: readonly string[];
  readonly pushedAt: string | null;
  readonly ownerAvatar: string;
  readonly ownerLogin: string;
}

export interface GitHubSearchResult {
  readonly totalCount: number;
  readonly incompleteResults: boolean;
  readonly items: readonly GitHubRepo[];
}

interface GitHubApiItem {
  id: number;
  full_name: string;
  html_url: string;
  clone_url: string;
  description: string | null;
  stargazers_count: number;
  language: string | null;
  topics: string[];
  pushed_at: string | null;
  owner: { avatar_url: string; login: string };
}

const GITHUB_API = 'https://api.github.com';

export function searchRepos(query: string, page: number, perPage: number, signal?: AbortSignal): Promise<GitHubSearchResult> {
  const q = encodeURIComponent(query);
  const url = `${GITHUB_API}/search/repositories?q=${q}&sort=stars&order=desc&per_page=${perPage}&page=${page}`;
  return fetch(url, {
    headers: { Accept: 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28' },
    signal,
  }).then((r) => {
    if (!r.ok) throw new Error(r.status === 403 ? 'GitHub rate limit reached. Wait a moment.' : `GitHub API error: ${r.status}`);
    return r.json() as Promise<{ total_count: number; incomplete_results: boolean; items: GitHubApiItem[] }>;
  }).then((data) => ({
    totalCount: data.total_count,
    incompleteResults: data.incomplete_results,
    items: data.items.map(itemToRepo),
  }));
}

function itemToRepo(item: GitHubApiItem): GitHubRepo {
  return {
    id: item.id,
    fullName: item.full_name,
    htmlUrl: item.html_url,
    cloneUrl: item.clone_url,
    description: item.description,
    stargazersCount: item.stargazers_count,
    language: item.language,
    topics: item.topics ?? [],
    pushedAt: item.pushed_at,
    ownerAvatar: item.owner.avatar_url,
    ownerLogin: item.owner.login,
  };
}
