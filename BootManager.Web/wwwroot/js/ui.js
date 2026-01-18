window.toggleCollapse = function (id) {
    try {
        var el = document.getElementById(id);
        if (!el) return;
        el.classList.toggle('show');
    } catch (e) {
        console && console.error(e);
    }
};

window.closeCollapse = function (id) {
    try {
        var el = document.getElementById(id);
        if (!el) return;
        el.classList.remove('show');
    } catch (e) {
        console && console.error(e);
    }
};

window.setupAutoCollapse = function (id, togglerSelector) {
    try {
        var collapseEl = document.getElementById(id);
        if (!collapseEl) return;

        // Use provided selector or fallback to data-target matching
        var toggler = null;
        if (togglerSelector) toggler = document.querySelector(togglerSelector);
        if (!toggler) toggler = document.querySelector('[data-target="#' + id + '"]');

        document.addEventListener('click', function (e) {
            if (!collapseEl.classList.contains('show')) return;

            var clickedInside = collapseEl.contains(e.target);
            var clickedToggler = toggler && toggler.contains(e.target);

            if (!clickedInside && !clickedToggler) {
                collapseEl.classList.remove('show');
            }

            // If a nav link inside the collapse was clicked, close the collapse
            if (clickedInside) {
                var anchor = e.target.closest('a');
                if (anchor && anchor.classList.contains('nav-link')) {
                    collapseEl.classList.remove('show');
                }
            }
        });
    } catch (e) {
        console && console.error(e);
    }
};
