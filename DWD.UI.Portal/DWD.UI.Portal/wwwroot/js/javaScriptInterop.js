window.interop = {
  loadScript: function (scriptUrl) {
    return new Promise((resolve, reject) => {
      const scriptElement = document.createElement('script');
      scriptElement.src = scriptUrl;
      scriptElement.onload = resolve;
      scriptElement.onerror = reject;
      document.body.appendChild(scriptElement);
    });
  },

  initPreline: function () {
    try {
      if (window.preline && typeof window.preline.init === 'function') {
        window.preline.init();
      }
    } catch (e) {
      console.error('initPreline failed', e);
    }
  }
};