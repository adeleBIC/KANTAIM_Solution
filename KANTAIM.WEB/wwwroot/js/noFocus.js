// noFocus.js — version robuste
(function () {
    // Blur immédiat + avec setTimeout pour contrer les focus programmatiques
    function forceBlur(el) {
        try {
            el.blur && el.blur();
        } catch (e) { /* ignore */ }
        // some scanners might re-focus milliseconds later, so blur again
        setTimeout(function () {
            try { el.blur && el.blur(); } catch (e) { }
        }, 10);
        setTimeout(function () {
            try { el.blur && el.blur(); } catch (e) { }
        }, 50);
    }

    function processButton(btn) {
        if (!btn) return;
        // assure tabindex -1
        btn.setAttribute('tabindex', '-1');

        // remove existing listeners we might have added previously (defensive)
        if (btn._nofocus_attached) return;
        btn._nofocus_attached = true;

        // écoute capture 'focus' pour intercepter avant d'autres handlers
        btn.addEventListener('focus', function (e) {
            // si le bouton a la classe nofocus, on blur
            if (e.target && e.target.classList && e.target.classList.contains('nofocus')) {
                forceBlur(e.target);
                e.preventDefault();
            }
        }, true); // capture = true

        // écoute 'mousedown' pour autoriser le click tactile (on ne bloque pas pointer-events)
        // on ne fait rien ici - exist for future tweaks
    }

    function scanAndAttach() {
        document.querySelectorAll('button.nofocus').forEach(processButton);
    }

    // Observe DOM changes (MudBlazor peut recréer le bouton)
    var mo = new MutationObserver(function (mutations) {
        // simple throttle
        scanAndAttach();
    });

    // start observer
    mo.observe(document.documentElement || document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });

    // handler global capture: si un élément reçoit focus et possède la classe nofocus -> blur
    document.addEventListener('focusin', function (e) {
        var t = e.target;
        if (t && t.classList && t.classList.contains('nofocus')) {
            forceBlur(t);
            e.preventDefault();
        }
    }, true); // capture

    // expose init for explicit call
    window.initNoFocus = function () {
        scanAndAttach();
    };

    // auto init on load
    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        setTimeout(scanAndAttach, 0);
    } else {
        window.addEventListener('DOMContentLoaded', scanAndAttach);
    }
})();
