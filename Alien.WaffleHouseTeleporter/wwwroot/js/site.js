(() => {
    const form = document.querySelector('[data-teleport-form]');
    if (!form) return;
    const button = form.querySelector('button');
    const status = document.getElementById('portal-status');
    let inFlight = false;
    form.addEventListener('submit', async event => {
        event.preventDefault();
        if (inFlight) return;
        inFlight = true;
        const inline = form.dataset.inline === 'true';
        // Open during the click gesture to avoid popup blocking after the async request.
        const portal = inline ? null : window.open('about:blank', '_blank');
        if (portal) portal.opener = null;
        button.disabled = true;
        button.textContent = 'Aligning syrup matrix…';
        status.textContent = 'Locking onto a breakfast beacon…';
        document.body.classList.add('portal-opening');
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 10000);
        try {
            const query = new URLSearchParams({ handler: 'Random', exclude: form.elements.exclude.value });
            const response = await fetch(`/Teleport?${query}`, { cache: 'no-store', signal: controller.signal });
            if (!response.ok) throw new Error('Destination unavailable');
            const destination = await response.json();
            document.getElementById('destination-title').textContent = destination.name;
            document.getElementById('destination-address').textContent = destination.address;
            document.getElementById('destination-coordinates').textContent = `Earth coordinates: ${destination.coordinates}`;
            document.getElementById('street-view-link').href = destination.streetViewUrl;
            document.getElementById('maps-link').href = destination.mapsUrl;
            document.getElementById('details-link').href = destination.detailsUrl;
            form.elements.exclude.value = destination.id;
            document.getElementById('destination').hidden = false;
            if (inline && destination.embedUrl) {
                const frame = document.createElement('iframe');
                frame.title = 'Street View near the selected Waffle House';
                frame.src = destination.embedUrl;
                frame.allowFullscreen = true;
                frame.referrerPolicy = 'no-referrer-when-downgrade';
                const view = document.getElementById('inline-view');
                view.replaceChildren(frame);
                view.hidden = false;
                status.textContent = 'Portal open. Welcome to your next breakfast stop.';
            } else if (portal && !portal.closed) {
                portal.location.replace(destination.streetViewUrl);
                status.textContent = 'Portal open in your new tab. Another jump awaits here.';
            } else {
                status.textContent = 'Destination ready. Use Open Street View below to enter your portal.';
            }
            button.textContent = 'Teleport again ↗';
        } catch {
            if (portal && !portal.closed) portal.close();
            status.textContent = 'The portal could not connect. Please try again.';
            button.textContent = 'Retry teleport ↗';
        } finally {
            clearTimeout(timeout);
            inFlight = false;
            button.disabled = false;
            document.body.classList.remove('portal-opening');
        }
    });
})();
