(() => {
    const guardForms = Array.from(document.querySelectorAll('form'))
        .filter(form => form.dataset.unsavedGuard !== 'false');
    if (guardForms.length === 0) {
        return;
    }

    const message = 'B?n có thay ğ?i chıa lıu. B?n có ch?c mu?n r?i trang?';
    let isDirty = false;
    let isSubmitting = false;

    const markDirty = () => {
        if (isSubmitting) {
            return;
        }
        isDirty = true;
    };

    guardForms.forEach(form => {
        if (form.dataset.unsavedAlways === 'true') {
            isDirty = true;
        }

        form.addEventListener('input', markDirty);
        form.addEventListener('change', markDirty);
        form.addEventListener('submit', () => {
            isSubmitting = true;
            isDirty = false;
        });
    });

    window.addEventListener('beforeunload', event => {
        if (!isDirty) {
            return;
        }
        event.preventDefault();
        event.returnValue = message;
        return message;
    });

    document.addEventListener('click', event => {
        const link = event.target.closest('a');
        if (!link || !isDirty) {
            return;
        }

        if (link.dataset.skipUnsavedGuard === 'true') {
            return;
        }

        if (link.target && link.target !== '_self') {
            return;
        }

        const href = link.getAttribute('href');
        if (!href || href.startsWith('#') || href.startsWith('javascript:') || href.startsWith('mailto:') || href.startsWith('tel:')) {
            return;
        }

        if (!window.confirm(message)) {
            event.preventDefault();
            event.stopPropagation();
        }
    });
})();
