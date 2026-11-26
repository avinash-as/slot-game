(() => {
  const canvas = document.getElementById('game');
  const ctx = canvas.getContext('2d');
  const W = canvas.width, H = canvas.height;

  const SYMBOLS = [
    { name: 'Cherry', emoji: '🍒', weight: 28, p3: 5, p4: 10, p5: 25, wild: false, scatter: false, color: '#ff6b6b' },
    { name: 'Lemon', emoji: '🍋', weight: 24, p3: 4, p4: 8, p5: 20, wild: false, scatter: false, color: '#ffd93d' },
    { name: 'Orange', emoji: '🍊', weight: 20, p3: 6, p4: 12, p5: 30, wild: false, scatter: false, color: '#ff9f43' },
    { name: 'Plum', emoji: '🍇', weight: 18, p3: 8, p4: 16, p5: 40, wild: false, scatter: false, color: '#a29bfe' },
    { name: 'Bell', emoji: '🔔', weight: 12, p3: 15, p4: 30, p5: 75, wild: false, scatter: false, color: '#feca57' },
    { name: 'Bar', emoji: '▬', weight: 8, p3: 25, p4: 50, p5: 125, wild: false, scatter: false, color: '#c8d6e5' },
    { name: 'Seven', emoji: '7️⃣', weight: 5, p3: 50, p4: 100, p5: 250, wild: false, scatter: false, color: '#ff6b6b' },
    { name: 'Wild', emoji: '⭐', weight: 6, p3: 0, p4: 0, p5: 0, wild: true, scatter: false, color: '#ffeaa7' },
    { name: 'Scatter', emoji: '💎', weight: 4, p3: 0, p4: 0, p5: 0, wild: false, scatter: true, color: '#74b9ff' }
  ];

  const REELS = 5, ROWS = 3, VISIBLE = 3, STRIP = 26;
  const CELL = 132, GAP = 10;
  const GRID_W = REELS * (CELL + GAP) - GAP;
  const GRID_X = (W - GRID_W) / 2;
  const GRID_Y = 60;

  let balance = 1000, bet = 10, freeSpins = 0, multiplier = 1;
  let spinning = false, muted = false;
  let message = 'Place your bet and spin!', messageType = '', lastWin = 0;

  const reels = [];
  for (let r = 0; r < REELS; r++) reels.push({ strip: [], pos: 0, start: 0, duration: 1, targets: null });

  function weightedIndex() {
    const weights = SYMBOLS.map(s => s.weight);
    const total = weights.reduce((a, b) => a + b, 0);
    let roll = Math.random() * total, cum = 0;
    for (let i = 0; i < weights.length; i++) {
      cum += weights[i];
      if (roll < cum) return i;
    }
    return weights.length - 1;
  }

  function makeRandomStrip() {
    const s = [];
    for (let i = 0; i < STRIP; i++) s.push(weightedIndex());
    return s;
  }

  function makeStripForTargets(targets) {
    const s = [];
    for (let i = 0; i < STRIP - VISIBLE; i++) s.push(weightedIndex());
    for (let i = 0; i < VISIBLE; i++) s.push(targets[i]);
    return s;
  }

  let audioCtx = null;
  function ensureAudio() {
    if (!audioCtx) audioCtx = new (window.AudioContext || window.webkitAudioContext)();
  }
  function beep(freq, dur, type = 'sine', vol = 0.15) {
    if (muted) return;
    ensureAudio();
    const o = audioCtx.createOscillator();
    const g = audioCtx.createGain();
    o.type = type; o.frequency.value = freq; g.gain.value = vol;
    o.connect(g); g.connect(audioCtx.destination);
    o.start();
    g.gain.exponentialRampToValueAtTime(0.0001, audioCtx.currentTime + dur);
    o.stop(audioCtx.currentTime + dur);
  }
  const playSpin = () => beep(220, 0.08, 'sawtooth', 0.08);
  const playStop = () => beep(440, 0.06, 'square', 0.1);
  const playWin = () => [523, 659, 784].forEach((f, i) => setTimeout(() => beep(f, 0.12, 'triangle', 0.15), i * 100));
  const playBigWin = () => [523, 659, 784, 1046].forEach((f, i) => setTimeout(() => beep(f, 0.18, 'triangle', 0.2), i * 120));
  const playBonus = () => [392, 523, 659, 784, 988].forEach((f, i) => setTimeout(() => beep(f, 0.14, 'sine', 0.15), i * 90));

  function generateOutcome() {
    const grid = [];
    for (let r = 0; r < REELS; r++) {
      const col = [];
      for (let row = 0; row < ROWS; row++) col.push(weightedIndex());
      grid.push(col);
    }
    return grid;
  }

  function evaluate(grid) {
    let scatters = 0;
    for (let r = 0; r < REELS; r++)
      for (let row = 0; row < ROWS; row++)
        if (SYMBOLS[grid[r][row]].scatter) scatters++;

    let firstNonWild = null, allSame = true;
    for (let r = 0; r < REELS && allSame; r++) {
      for (let row = 0; row < ROWS; row++) {
        const idx = grid[r][row];
        if (SYMBOLS[idx].wild) continue;
        if (firstNonWild === null) firstNonWild = idx;
        else if (idx !== firstNonWild) { allSame = false; break; }
      }
    }
    if (allSame) {
      const sym = firstNonWild !== null ? firstNonWild : grid[0][0];
      return { win: true, jackpot: true, symbol: sym, count: REELS * ROWS, payout: bet * 50 * multiplier, scatters };
    }

    const line = [];
    for (let r = 0; r < REELS; r++) line.push(grid[r][1]);
    let lineSym = null;
    for (const s of line) { if (!SYMBOLS[s].wild) { lineSym = s; break; } }
    if (lineSym === null) lineSym = line[0];

    let match = 0;
    for (const s of line) {
      if (SYMBOLS[s].wild || s === lineSym) match++;
      else break;
    }

    let payout = 0, win = false;
    if (match >= 3) {
      win = true;
      const sym = SYMBOLS[lineSym];
      const p = match === 3 ? sym.p3 : match === 4 ? sym.p4 : sym.p5;
      payout = bet * p * multiplier;
    }
    return { win, jackpot: false, symbol: lineSym, count: match, payout, scatters };
  }

  const spinBtn = document.getElementById('spin');
  function startSpin() {
    if (spinning) return;
    const usingFree = freeSpins > 0;
    if (!usingFree && balance < bet) {
      message = 'Insufficient balance'; messageType = '';
      updateUI(); return;
    }
    if (usingFree) freeSpins--; else balance -= bet;

    spinning = true;
    spinBtn.disabled = true;
    message = 'Spinning...'; messageType = '';
    playSpin();

    const grid = generateOutcome();
    const baseDur = 1.5;
    reels.forEach((reel, r) => {
      reel.strip = makeStripForTargets(grid[r].slice());
      reel.targets = grid[r].slice();
      reel.pos = 0;
      reel.start = performance.now() + r * 80;
      reel.duration = baseDur + r * 0.22;
    });

    const check = setInterval(() => {
      const now = performance.now();
      let allDone = true;
      for (const reel of reels) {
        const t = Math.min(1, Math.max(0, (now - reel.start) / (reel.duration * 1000)));
        if (t < 1) allDone = false;
      }
      if (allDone) {
        clearInterval(check);
        onFinish(grid);
      }
    }, 50);
  }

  function easeInOutCubic(t) {
    return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
  }

  function onFinish(grid) {
    const result = evaluate(grid);
    if (result.win) {
      balance += result.payout;
      lastWin = result.payout;
      if (result.jackpot) {
        message = `JACKPOT! +${result.payout}`; messageType = 'jackpot';
        playBigWin();
      } else {
        message = `WIN +${result.payout} (${result.count} of a kind)`; messageType = 'win';
        playWin();
      }
    } else {
      message = 'Try again'; messageType = '';
      lastWin = 0;
    }
    if (result.scatters >= 3) {
      freeSpins += 10;
      multiplier = 2;
      message += '  🎉 10 Free Spins (2x)!';
      playBonus();
    }
    spinning = false;
    spinBtn.disabled = false;
    updateUI();
  }

  function roundRect(x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
  }

  function draw() {
    ctx.clearRect(0, 0, W, H);
    const g = ctx.createLinearGradient(0, 0, 0, H);
    g.addColorStop(0, '#16213e'); g.addColorStop(1, '#0f0f1a');
    ctx.fillStyle = g; ctx.fillRect(0, 0, W, H);

    roundRect(GRID_X - 16, 44, GRID_W + 32, ROWS * CELL + 32, 18);
    ctx.fillStyle = '#0a0a14'; ctx.fill();
    ctx.strokeStyle = '#ffd700'; ctx.lineWidth = 3; ctx.stroke();

    const now = performance.now();
    for (let r = 0; r < REELS; r++) {
      const reel = reels[r];
      const t = Math.min(1, Math.max(0, (now - reel.start) / (reel.duration * 1000)));
      const eased = easeInOutCubic(t);
      const travel = STRIP - VISIBLE;
      reel.pos = eased * travel;

      const rx = GRID_X + r * (CELL + GAP);
      ctx.save();
      ctx.beginPath(); ctx.rect(rx, GRID_Y, CELL, ROWS * CELL); ctx.clip();

      const baseIndex = Math.floor(reel.pos);
      const frac = reel.pos - baseIndex;
      for (let i = -1; i <= VISIBLE; i++) {
        const idx = baseIndex + i;
        if (idx < 0 || idx >= reel.strip.length) continue;
        const sym = SYMBOLS[reel.strip[idx]];
        const y = GRID_Y + (i - frac) * CELL;
        const cg = ctx.createLinearGradient(rx, y, rx, y + CELL);
        cg.addColorStop(0, sym.color + '55'); cg.addColorStop(1, '#00000088');
        ctx.fillStyle = cg;
        roundRect(rx + 4, y + 4, CELL - 8, CELL - 8, 12); ctx.fill();
        ctx.font = '64px serif'; ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
        ctx.fillStyle = '#fff'; ctx.fillText(sym.emoji, rx + CELL / 2, y + CELL / 2);
        ctx.font = '11px sans-serif'; ctx.fillStyle = 'rgba(255,255,255,0.7)';
        ctx.fillText(sym.name, rx + CELL / 2, y + CELL - 12);
      }
      ctx.restore();
    }

    ctx.strokeStyle = 'rgba(255,215,0,0.35)'; ctx.lineWidth = 2;
    ctx.setLineDash([6, 6]);
    ctx.beginPath();
    ctx.moveTo(GRID_X, GRID_Y + CELL + CELL / 2);
    ctx.lineTo(GRID_X + GRID_W, GRID_Y + CELL + CELL / 2);
    ctx.stroke(); ctx.setLineDash([]);

    requestAnimationFrame(draw);
  }

  function updateUI() {
    document.getElementById('balance').textContent = balance;
    document.getElementById('bet').textContent = bet;
    document.getElementById('win').textContent = lastWin;
    document.getElementById('freespins').textContent = freeSpins;
    const msg = document.getElementById('message');
    msg.textContent = message;
    msg.className = 'message ' + (messageType || '');
  }

  document.getElementById('betDown').addEventListener('click', () => {
    if (spinning) return;
    bet = Math.max(10, bet - 10); updateUI();
  });
  document.getElementById('betUp').addEventListener('click', () => {
    if (spinning) return;
    bet = Math.min(100, bet + 10); updateUI();
  });
  spinBtn.addEventListener('click', startSpin);
  document.getElementById('mute').addEventListener('click', (e) => {
    muted = !muted; e.target.textContent = muted ? '🔇' : '🔊';
  });

  reels.forEach(r => r.strip = makeRandomStrip());
  updateUI();
  requestAnimationFrame(draw);
})();