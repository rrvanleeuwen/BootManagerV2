/**
 * Infinite scroll handler voor logboek card-view.
 * Detecteert wanneer de load-meer-knop in zicht komt en triggert automatisch laden.
 */
let observer;
let container;
let loadMoreBtn;

export function initInfiniteScroll() {
    container = document.getElementById('cardScrollContainer');
    if (!container) return;

    observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && loadMoreBtn && !loadMoreBtn.disabled) {
                loadMoreBtn.click();
            }
        });
    }, {
        root: null,
        rootMargin: '100px',
        threshold: 0.01
    });

    refreshInfiniteScroll();
}

export function refreshInfiniteScroll() {
    if (!observer || !container) return;

    if (loadMoreBtn) {
        observer.unobserve(loadMoreBtn);
    }

    loadMoreBtn = findLoadMoreButton();
    if (loadMoreBtn) {
        observer.observe(loadMoreBtn);
    }
}

export function disconnectInfiniteScroll() {
    if (observer) {
        observer.disconnect();
    }

    observer = undefined;
    container = undefined;
    loadMoreBtn = undefined;
}

function findLoadMoreButton() {
    const buttons = container.querySelectorAll('button');
    return Array.from(buttons).find(btn => btn.textContent.includes('Meer laden'));
}
