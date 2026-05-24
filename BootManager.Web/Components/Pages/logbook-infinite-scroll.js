/**
 * Infinite scroll handler voor logboek card-view
 * Detecteert wanneer de load-meer-knop in zicht komt en triggert automatisch laden
 */
export function initInfiniteScroll(dotnetHelper) {
    const container = document.getElementById('cardScrollContainer');
    if (!container) return;

    const options = {
        root: null, // viewport
        rootMargin: '100px', // Trigger 100px voordat de knop volledig zichtbaar is
        threshold: 0.01
    };

    // Vind de load-meer-knop
    const findLoadMoreButton = () => {
        // Zoek naar de "Meer laden" knop
        const buttons = container.querySelectorAll('button');
        return Array.from(buttons).find(btn => btn.textContent.includes('Meer laden'));
    };

    let loadMoreBtn = findLoadMoreButton();

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && loadMoreBtn) {
                // Check of de knop niet al disabled is
                if (!loadMoreBtn.disabled) {
                    console.log('Load more button is visible, triggering click');
                    loadMoreBtn.click();
                }
            }
        });
    }, options);

    // Observer de load-meer-knop
    if (loadMoreBtn) {
        observer.observe(loadMoreBtn);
    }

    // Expose cleanup function
    return {
        disconnect: () => observer.disconnect(),
        refresh: () => {
            loadMoreBtn = findLoadMoreButton();
            observer.disconnect();
            if (loadMoreBtn) {
                observer.observe(loadMoreBtn);
            }
        }
    };
}
