let state={analysisRunning:false,content:'',profile:'focused',format:'markdown',
  steps:['Discovery','Extraction','Signals','Pruning','Compression','Render']};
let pending={},_counter=0;

// === Bridge ===
// C# sends messages via window.SendWebMessage(str) → received as 'message' event
// JS sends messages via window.external.sendMessage(str) → received by C# RegisterWebMessageReceivedHandler

window.addEventListener('message',function(e){
  var m; try{m=typeof e.data==='string'?JSON.parse(e.data):e.data;}catch(ex){return;}
  if(!m||!m.type)return;
  var entry,mId=m.data&&m.data.id;
  if(m.type==='reply'){entry=pending[mId];delete pending[mId];if(entry)entry.resolve(m.data);return;}
  if(m.type==='done'){entry=pending[mId];delete pending[mId];if(entry)entry.resolve(m.data);handleDone(m.data);return;}
  if(m.type==='error'){entry=pending[mId];delete pending[mId];if(entry)entry.reject(m.data.error);else toast(m.data?m.data.error:'Error','error');return;}
  if(m.type==='progress'){handleProgress(m.data);return;}
});

function bridge(type,data){return new Promise(function(resolve,reject){
  var id='m'+(++_counter).toString(36);
  pending[id]={resolve:resolve,reject:reject};
  setTimeout(function(){if(pending[id]){delete pending[id];reject('timeout');}},300000);
  try{window.external.sendMessage(JSON.stringify({id:id,type:type,data:data||{}}));}catch(e){delete pending[id];reject(e);}
});}

// === Minimal Markdown Renderer ===
function renderMD(md){
  var lines=md.split('\n'),out=[],inT=0,inC=0;
  for(var i=0;i<lines.length;i++){var l=lines[i];
    if(l.startsWith('```')){inC=!inC;out.push(inC?'<pre><code>':'</code></pre>');continue;}
    if(inC){out.push(esc(l));continue;}
    if(l.startsWith('#')){var d=l.match(/^(#{1,3})\s+(.+)/);out.push(d?'<h'+(d[1].length+1)+'>'+d[2]+'</h'+(d[1].length+1)+'>':'');continue;}
    if(l.startsWith('|')&&l.endsWith('|')){
      var c=l.split('|').filter(function(x){return x.trim();});
      if(c.every(function(x){return /^[-: ]+$/.test(x);}))continue;
      if(!inT){out.push('<table>');inT=1;}
      var tag=out.length<2||out[out.length-1]==='<table>'||out[out.length-1].endsWith('</tr>')?'th':'td';
      out.push('<tr>'+c.map(function(x){return'<'+tag+'>'+inline(x)+'</'+tag+'>';}).join('')+'</tr>');continue;
    }else if(inT){out.push('</table>');inT=0;}
    if(l.startsWith('- ')){out.push('<li>'+inline(l.slice(2))+'</li>');continue;}
    if(l==='---'||l==='***'){out.push('<hr>');continue;}
    if(!l.trim())continue;
    out.push('<p>'+inline(l)+'</p>');
  }
  if(inT)out.push('</table>');if(inC)out.push('</code></pre>');
  return out.join('');
}
function inline(s){return esc(s).replace(/\*\*(.+?)\*\*/g,'<strong>$1</strong>').replace(/`([^`]+)`/g,'<code>$1</code>').replace(/\*(.+?)\*/g,'<em>$1</em>');}
function esc(s){return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');}

// === UI Actions ===
function pickFolder(){bridge('pick-folder').then(function(p){if(p){$('path').value=p;checkPath();}}).catch(function(){});}
function pickFile(){bridge('pick-file').then(function(p){if(p){$('path').value=p;checkPath();}}).catch(function(){});}

function checkPath(){
  var el=$('path-status'),v=$('path').value.trim();
  if(!v){el.textContent='';return;}
  var ok=v.endsWith('.sln')||v.endsWith('.slnx')||v.endsWith('.csproj')||v.indexOf('\\src\\')>=0||v.indexOf('/src/')>=0;
  el.textContent=ok?'Valid':'Not found';el.style.color=ok?'var(--success)':'var(--warn)';
}

function setProfile(p){state.profile=p;$$('#profile-group .pill').forEach(function(b){b.classList.toggle('active',b.dataset.p===p);});}
function setFormat(f){state.format=f;$$('#format-group .pill').forEach(function(b){b.classList.toggle('active',b.dataset.f===f);});}

async function execute(){
  if(state.analysisRunning)return;
  var path=$('path').value.trim();
  if(!path){toast('Select a project path','error');return;}
  resetOutput();state.analysisRunning=true;
  var btn=$('execute-btn');btn.disabled=true;btn.textContent='\u27F3 Analyzing…';
  $('progress').classList.add('show');$('header-bar').classList.remove('hidden');
  var sh=state.steps.map(function(s){return'<span class="progress-step">'+s+'</span>';}).join('');
  $('psteps').innerHTML=sh;$('pfill').style.width='0%';progressDone=0;
  try{await bridge('analyze',{
    path:path,scenario:$('scenario').value,profile:state.profile,
    around:$('around').value.trim(),maxTokens:parseInt($('tokens').value),
    format:state.format,includeProvenance:$('opt-provenance').checked,
    includeDiagnostics:$('opt-diagnostics').checked,noRoslyn:$('opt-noroslyn').checked,
    dryRun:$('opt-dryrun').checked
  });}catch(e){toast(String(e),'error');}
  finally{state.analysisRunning=false;btn.disabled=false;btn.textContent='\u25B6 Analyze';}
}

function resetOutput(){
  $('output').innerHTML='<div id="placeholder" style="display:flex;flex-direction:column;align-items:center;justify-content:center;height:100%"><div style="font-size:48px;margin-bottom:12px">\u27F3</div><p style="color:var(--muted)">Analyzing\u2026</p></div>';
  $('stats').innerHTML='';
}

var progressDone=0;
function handleProgress(data){
  if(data.stage==='extractor'){
    var steps=$('psteps').querySelectorAll('.progress-step');
    if(progressDone<steps.length)steps[progressDone].classList.add('done');
    progressDone=Math.min(progressDone+1,steps.length);
    $('pfill').style.width=Math.min(90,progressDone/state.steps.length*90+10)+'%';
    $('pinfo').textContent=data.name+' ('+Math.round(data.elapsed)+'ms)';
    $('header-bar').classList.remove('hidden');
  }
  if(data.stage==='launched'){$('pinfo').textContent='Starting…';$('pfill').style.width='5%';}
  if(data.stage==='render'||data.stage==='pipeline-completed'){
    var steps=$('psteps').querySelectorAll('.progress-step');
    steps.forEach(function(s){s.classList.add('done');});
    $('pfill').style.width='100%';$('pinfo').textContent='Done';
  }
}

function handleDone(data){
  var c=data.data||data.content||'';
  state.content=c;
  if(c.trim().startsWith('{')||c.trim().startsWith('[')){
    try{$('output').innerHTML='<pre><code>'+esc(JSON.stringify(JSON.parse(c),null,2))+'</code></pre>';}
    catch(e){$('output').innerHTML='<pre>'+esc(c)+'</pre>';}
  }else{$('output').innerHTML=renderMD(c);}
  var tok=data.tokens||Math.round(c.length/4),time=data.timeMs?(data.timeMs/1000).toFixed(1)+'s':'—';
  $('stats').innerHTML='<span>\uD83D\uDCCF ~'+tok.toLocaleString()+' tokens</span><span>\u23F1 '+time+'</span>';
  $('header-bar').classList.remove('hidden');
}

function copyOutput(){if(state.content)navigator.clipboard.writeText(state.content).then(function(){toast('Copied!','success');}).catch(function(){toast('Copy failed','error');});}
function saveOutput(){if(state.content)bridge('save-file',{content:state.content,ext:state.format==='json'?'json':'md'});}
function toast(msg,type){var el=$('toast');el.textContent=msg;el.className='toast show '+(type||'');setTimeout(function(){el.classList.remove('show');},3000);}
function $(id){return document.getElementById(id);}
function $$(s){return document.querySelectorAll(s);}

// === Init ===
window.addEventListener('DOMContentLoaded',function(){
  bridge('get-version').then(function(v){var el=document.querySelector('#sidebar h1 span');if(el)el.textContent='v'+v;}).catch(function(){});
  bridge('get-recent').then(function(r){if(Array.isArray(r)&&r.length){
    $('recent').innerHTML=r.slice(0,5).map(function(p){var n=p.split('\\').pop()||p.split('/').pop()||p;
      return'<span class="chip" title="'+esc(p)+'" onclick="$(\'path\').value=\''+esc(p).replace(/'/g,"\\'")+'\';checkPath()">\uD83D\uDCC1 '+esc(n)+'</span>';}).join('');
  }}).catch(function(){});
  bridge('get-settings').then(function(s){if(s){
    if(s.lastScenario)$('scenario').value=s.lastScenario;
    if(s.lastProfile)setProfile(s.lastProfile);if(s.lastFormat)setFormat(s.lastFormat);
    if(s.lastTokens){$('tokens').value=s.lastTokens;$('token-val').textContent=s.lastTokens;}
    if(s.lastAround)$('around').value=s.lastAround;
    if(s.includeProvenance)$('opt-provenance').checked=true;
    if(s.includeDiagnostics)$('opt-diagnostics').checked=true;
    if(s.noRoslyn)$('opt-noroslyn').checked=true;
  }}).catch(function(){});
});

document.addEventListener('keydown',function(e){
  if((e.ctrlKey||e.metaKey)&&e.key==='Enter'){e.preventDefault();execute();}
  if((e.ctrlKey||e.metaKey)&&e.key==='o'){e.preventDefault();pickFolder();}
  if((e.ctrlKey||e.metaKey)&&e.key==='s'){e.preventDefault();saveOutput();}
});
