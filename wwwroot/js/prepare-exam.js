(function () {
  const button = document.getElementById('startExamButton');
  if (!button) {
    return;
  }

  button.addEventListener('click', async () => {
    button.disabled = true;
    button.textContent = 'Preparando prova...';

    try {
      await document.documentElement.requestFullscreen?.();
    } finally {
      window.location.href = button.dataset.startUrl;
    }
  });
})();
