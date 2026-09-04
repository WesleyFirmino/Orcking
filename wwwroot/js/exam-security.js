(function () {
  const radios = document.querySelectorAll('input[type="radio"][data-attempt-id]');
  const firstRadio = radios[0];
  if (!firstRadio) {
    return;
  }

  const attemptId = Number(firstRadio.dataset.attemptId);
  let reporting = false;
  let submitting = false;
  let fullscreenWasActive = Boolean(document.fullscreenElement);

  async function postJson(url, payload) {
    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      keepalive: true
    });

    if (!response.ok) {
      return null;
    }

    return response.json();
  }

  async function reportCritical(eventType, details) {
    if (reporting || submitting) {
      return;
    }

    reporting = true;
    const result = await postJson('/Exams/ReportEvent', {
      attemptId,
      eventType,
      severity: 'critical',
      details
    });

    if (result) {
      const modalElement = document.getElementById('violationModal');
      const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
      modal.show();
      setTimeout(() => window.location.href = '/Student', 1600);
    }
  }

  radios.forEach((radio) => {
    radio.addEventListener('change', () => {
      postJson('/Exams/SaveAnswer', {
        attemptId,
        questionId: Number(radio.dataset.questionId),
        optionId: Number(radio.value)
      });
    });
  });

  const blockedKeys = new Set([
    'F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'F7', 'F8', 'F9', 'F10', 'F11', 'F12',
    'PrintScreen'
  ]);

  document.addEventListener('keydown', (event) => {
    const systemCombo = event.altKey || event.metaKey || (event.ctrlKey && ['c', 'v', 'x', 'p', 's', 'u'].includes(event.key.toLowerCase()));
    if (blockedKeys.has(event.key) || systemCombo) {
      event.preventDefault();
      reportCritical('blocked_key', `Tecla/combinacao bloqueada: ${event.key}`);
    }
  });

  document.addEventListener('contextmenu', (event) => event.preventDefault());
  document.addEventListener('copy', (event) => event.preventDefault());
  document.addEventListener('cut', (event) => event.preventDefault());
  document.addEventListener('paste', (event) => event.preventDefault());

  document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
      reportCritical('tab_hidden', 'A aba ficou oculta ou o navegador foi minimizado.');
    }
  });

  window.addEventListener('blur', () => {
    setTimeout(() => {
      if (!document.hasFocus()) {
        reportCritical('window_blur', 'A janela perdeu foco durante a prova.');
      }
    }, 250);
  });

  document.addEventListener('fullscreenchange', () => {
    const gate = document.getElementById('fullscreenGate');
    if (document.fullscreenElement) {
      fullscreenWasActive = true;
      gate?.classList.add('is-hidden');
      return;
    }

    gate?.classList.remove('is-hidden');
    if (fullscreenWasActive) {
      reportCritical('fullscreen_exit', 'O aluno saiu do modo tela cheia.');
    }
  });

  const fullscreenButton = document.getElementById('fullscreenButton');
  fullscreenButton?.addEventListener('click', () => {
    document.documentElement.requestFullscreen?.();
  });

  const gateFullscreenButton = document.getElementById('gateFullscreenButton');
  gateFullscreenButton?.addEventListener('click', () => {
    document.documentElement.requestFullscreen?.();
  });

  if (document.fullscreenElement) {
    document.getElementById('fullscreenGate')?.classList.add('is-hidden');
  }

  const timer = document.querySelector('.timer[data-ends-at]');
  const examForm = document.getElementById('examForm');
  examForm?.addEventListener('submit', () => {
    submitting = true;
  });

  function tick() {
    if (!timer) {
      return;
    }

    const endsAt = new Date(timer.dataset.endsAt).getTime();
    const remaining = Math.max(0, endsAt - Date.now());
    const minutes = Math.floor(remaining / 60000).toString().padStart(2, '0');
    const seconds = Math.floor((remaining % 60000) / 1000).toString().padStart(2, '0');
    timer.textContent = `${minutes}:${seconds}`;

    if (remaining === 0) {
      submitting = true;
      examForm?.submit();
    }
  }

  tick();
  setInterval(tick, 1000);
})();
