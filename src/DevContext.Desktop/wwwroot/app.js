// DevContext Desktop — Bridge Interop + UI Logic

let state = { recent: [], analysisRunning: false, currentContent: '' };

// === Bridge API ===

function call(type, data = {}) {
  return new Promise((resolve, reject) => {
    const id = Date.now().toString(36) + Math.random().toString(36).slice(2, 6);
    const handler = (e) => {
      try {
        const msg = typeof e === 'string' ? JSON.parse(e) : JSON.parse(e.data || e);
        if (msg.type === 'reply' && msg.data?.id === id) { window.removeEventListener('message', handler); resolve(msg.data.data); }
        if (msg.type === 'error' && msg.data?.id === id) { window.removeEventListener('message', handler); reject(msg.data.error); }
        if (msg.type === 'error' && !msg.data?.id) { showToast(msg.data?.error || 'Unknown error', 'error'); reject(msg.data?.error); }
        if (msg.type === 'progress') handleProgress(msg.data);
        if (msg.type === 'done') handleDone(msg.data);
      } catch {}
    };
    window.addEventListener('message', handler);
    const msg = JSON.stringify({ id, type, data });
    if (window.external && typeof window.external.sendMessage === 'function')
      window.external.sendMessage(msg);
    else if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage)
      window.chrome.webview.postMessage(msg);
    else
      console.warn('No bridge API available for message:', type);
  });
}

// === Core Actions ===

function pickFolder() { call('pick-folder').then(p => { if (p) document.getElementById('path').value = p; checkPath(); }).catch(()=>{}); }
function pickFile() { call('pick-file').then(p => { if (p) document.getElementById('path').value = p; checkPath(); }).catch(()=>{}); }

function checkPath() {
  const el = document.getElementById('path-status');
  const v = document.getElementById('path').value.trim();
  if (!v) { el.textContent = ''; return; }
  el.textContent = v.endsWith('.sln') || v.endsWith('.slnx') || v.endsWith('.csproj') || v.includes('\\src\\') || v.includes('/src/')
    ? '✓ Valid project path' : '⚠ Not in a .NET project directory';
  el.className = 'text-xs mt-1 h-4 ' + (el.textContent.startsWith('✓') ? 'text-success' : 'text-warn');
}

function setProfile(p) {
  document.querySelectorAll('#profile-group button').forEach(b => {
    b.classList.toggle('bg-accent', b.dataset.p === p);
    b.classList.toggle('text-gray-900', b.dataset.p === p);
    b.classList.toggle('bg-gray-700', b.dataset.p !== p);
    b.classList.toggle('text-gray-300', b.dataset.p !== p);
  });
  state.profile = p;
}

function setFormat(f) {
  document.querySelectorAll('#format-group button').forEach(b => {
    b.classList.toggle('bg-accent', b.dataset.f === f);
    b.classList.toggle('text-gray-900', b.dataset.f === f);
    b.classList.toggle('bg-gray-700', b.dataset.f !== f);
    b.classList.toggle('text-gray-300', b.dataset.f !== f);
  });
  state.format = f;
}

function toggleTheme() {
  const html = document.documentElement;
  const dark = html.classList.toggle('dark');
  document.getElementById('theme-btn').textContent = dark ? '🌙' : '☀️';
}

// === Execute ===

async function execute() {
  if (state.analysisRunning) return;
  const path = document.getElementById('path').value.trim();
  if (!path) { showToast('Select a project path first', 'error'); return; }

  resetProgress();
  state.analysisRunning = true;
  const btn = document.getElementById('execute-btn');
  btn.disabled = true;
  document.getElementById('btn-text').textContent = '⟳ Analyzing…';
  document.getElementById('progress-area').classList.remove('hidden');

  try {
    await call('analyze', {
      path,
      scenario: document.getElementById('scenario').value,
      profile: document.querySelector('#profile-group .bg-accent')?.dataset?.p || 'focused',
      around: document.getElementById('around').value.trim(),
      maxTokens: parseInt(document.getElementById('tokens').value),
      format: state.format || 'markdown',
      includeProvenance: document.getElementById('opt-provenance').checked,
      includeDiagnostics: document.getElementById('opt-diagnostics').checked,
      noRoslyn: document.getElementById('opt-noroslyn').checked,
      dryRun: document.getElementById('opt-dryrun').checked,
    });
  } catch (e) {
    showToast(String(e), 'error');
  } finally {
    state.analysisRunning = false;
    btn.disabled = false;
    document.getElementById('btn-text').textContent = '▶ Analyze';
  }
}

// === Progress ===

function resetProgress() {
  document.getElementById('stages').innerHTML = '';
  document.getElementById('progress-bar').classList.add('hidden');
  document.getElementById('progress-fill').style.width = '0%';
  document.getElementById('output').innerHTML = '';
  document.getElementById('placeholder').classList.add('hidden');
  document.getElementById('output-bar').classList.remove('hidden');
  document.getElementById('stats').innerHTML = '';
  document.getElementById('progress-extras').textContent = '';
  state.currentContent = '';
}

function handleProgress(data) {
  const stagesEl = document.getElementById('stages');
  const extrasEl = document.getElementById('progress-extras');
  const steps = ['Discovery', 'Extraction', 'Signals', 'Pruning', 'Compression', 'Render'];

  if (data.stage === 'pipeline-started') {
    stagesEl.innerHTML = steps.map(s => `<div class="flex items-center"><span class="stage-dot pending"></span>${s}</div>`).join('');
  }
  if (data.stage === 'extractor') {
    const pending = stagesEl.querySelector('.stage-dot.pending');
    if (pending) pending.className = 'stage-dot done';
    extrasEl.textContent = `${data.name} (${Math.round(data.elapsed)}ms)`;
  }
  if (data.stage === 'signals') {
    const pending = stagesEl.querySelectorAll('.stage-dot.pending');
    if (pending.length) pending[0].className = 'stage-dot done';
    extrasEl.textContent = `Signals: ${(data.signals || []).join(' · ')}`;
  }
  if (data.stage === 'pruner') {
    extrasEl.textContent = `${data.name}: ${data.before} → ${data.after} types`;
  }
  if (data.stage === 'stage-started') {
    const pending = stagesEl.querySelector('.stage-dot.pending');
    if (pending) pending.className = 'stage-dot active';
  }
  if (data.stage === 'stage-completed') {
    const active = stagesEl.querySelector('.stage-dot.active');
    if (active) active.className = 'stage-dot done';
    extrasEl.textContent = `${data.name || ''} — ${Math.round(data.elapsed)}ms`;
  }
  if (data.stage === 'render') {
    stagesEl.querySelectorAll('.stage-dot.pending, .stage-dot.active').forEach(d => d.className = 'stage-dot done');
    document.getElementById('progress-bar').classList.add('hidden');
  }

  // Update progress bar
  const total = stagesEl.querySelectorAll('.stage-dot').length;
  const done = stagesEl.querySelectorAll('.stage-dot.done').length;
  if (total > 0) {
    document.getElementById('progress-bar').classList.remove('hidden');
    document.getElementById('progress-fill').style.width = (done / total * 100) + '%';
  }
}

// === Results ===

function handleDone(data) {
  const outputEl = document.getElementById('output');
  const content = data.content || '';

  if (content.trim().startsWith('{') || content.trim().startsWith('[')) {
    try {
      outputEl.innerHTML = `<pre class="bg-gray-900 rounded-lg p-4 overflow-x-auto text-xs"><code>${escapeHtml(JSON.stringify(JSON.parse(content), null, 2))}</code></pre>`;
    } catch {
      outputEl.innerHTML = `<pre>${escapeHtml(content)}</pre>`;
    }
  } else {
    outputEl.innerHTML = marked.parse(content, { breaks: true });
    outputEl.querySelectorAll('pre code').forEach(b => {
      try { hljs.highlightElement(b); } catch {}
    });
  }

  state.currentContent = content;

  const statsEl = document.getElementById('stats');
  const tokens = data.tokens || 0;
  const time = data.timeMs ? (data.timeMs / 1000).toFixed(1) + 's' : '—';
  statsEl.innerHTML = `<span>📝 ~${tokens.toLocaleString()} tokens</span><span>⏱ ${time}</span>`;
}

// === Utility ===

function copyOutput() {
  if (!state.currentContent) return;
  navigator.clipboard.writeText(state.currentContent)
    .then(() => showToast('Copied to clipboard!', 'success'))
    .catch(() => showToast('Failed to copy', 'error'));
}

function saveOutput() {
  if (!state.currentContent) return;
  call('save-file', { content: state.currentContent, ext: state.format === 'json' ? 'json' : 'md' });
}

function showToast(msg, type = 'info') {
  const el = document.getElementById('toast');
  el.textContent = msg;
  el.className = `fixed bottom-4 left-1/2 -translate-x-1/2 px-4 py-2 rounded-lg text-sm shadow-lg z-50 transition-all ${type === 'error' ? 'bg-danger text-gray-900' : type === 'success' ? 'bg-success text-gray-900' : 'bg-gray-700 text-gray-200'}`;
  el.classList.remove('hidden');
  setTimeout(() => el.classList.add('hidden'), 3000);
}

function escapeHtml(s) {
  return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

// === Init ===

window.addEventListener('DOMContentLoaded', async () => {
  try { document.getElementById('version').textContent = await call('get-version') || 'v2.0.0'; } catch {}
  try {
    const r = await call('get-recent');
    state.recent = Array.isArray(r) ? r : [];
    if (state.recent.length) {
      const el = document.getElementById('recent');
      el.classList.remove('hidden');
      el.innerHTML = '<div class="flex flex-wrap gap-1 mt-1">' +
        state.recent.slice(0, 5).map(p =>
          `<button onclick="document.getElementById('path').value='${escapeHtml(p)}';checkPath()" class="text-xs bg-gray-700 hover:bg-gray-600 text-gray-300 px-2 py-0.5 rounded truncate max-w-48 transition">📁 ${escapeHtml(p.split('\\').pop()?.split('/').pop() || p)}</button>`
        ).join('') + '</div>';
    }
  } catch {}
  try {
    const s = await call('get-settings');
    if (s) {
      if (s.lastScenario) document.getElementById('scenario').value = s.lastScenario;
      if (s.lastProfile) setProfile(s.lastProfile);
      if (s.lastFormat) setFormat(s.lastFormat);
      if (s.lastTokens) { document.getElementById('tokens').value = s.lastTokens; document.getElementById('token-val').textContent = s.lastTokens; }
      if (s.lastAround) document.getElementById('around').value = s.lastAround;
      if (s.includeProvenance) document.getElementById('opt-provenance').checked = true;
      if (s.includeDiagnostics) document.getElementById('opt-diagnostics').checked = true;
      if (s.noRoslyn) document.getElementById('opt-noroslyn').checked = true;
      if (s.theme === 'light') { document.documentElement.classList.remove('dark'); document.getElementById('theme-btn').textContent = '☀️'; }
    }
  } catch {}
});

// === Keyboard shortcuts ===
document.addEventListener('keydown', e => {
  if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') { e.preventDefault(); execute(); }
  if ((e.ctrlKey || e.metaKey) && e.key === 'o') { e.preventDefault(); pickFolder(); }
  if ((e.ctrlKey || e.metaKey) && e.key === 's') { e.preventDefault(); saveOutput(); }
  if ((e.ctrlKey || e.metaKey) && e.key === 'c' && e.shiftKey) { e.preventDefault(); copyOutput(); }
});
