export async function postJson(url, payload) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload),
        credentials: 'include'
    });

    const body = await response.text();
    return {
        ok: response.ok,
        status: response.status,
        body: body ?? null
    };
}
