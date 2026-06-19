// Download-helper voor Blazor
window.downloadFileFromStream = async (filename, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// SVG markup to PNG converter for QR tag download
// C# passes SVG markup directly instead of using DOM selectors
window.downloadSvgMarkupAsPng = (svgMarkup, filename) => {
    if (!svgMarkup || !filename) {
        console.error('downloadSvgMarkupAsPng: Invalid svgMarkup or filename');
        return;
    }

    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const img = new Image();

    img.onload = () => {
        canvas.width = img.width;
        canvas.height = img.height;
        ctx.drawImage(img, 0, 0);
        canvas.toBlob((blob) => {
            if (!blob) {
                console.error('downloadSvgMarkupAsPng: Failed to create blob from canvas');
                return;
            }
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }, 'image/png');
    };

    img.onerror = () => {
        console.error('downloadSvgMarkupAsPng: Failed to load SVG as image');
    };

    try {
        img.src = 'data:image/svg+xml;base64,' + btoa(svgMarkup);
    } catch (e) {
        console.error('downloadSvgMarkupAsPng: Failed to encode SVG markup:', e.message);
    }
};

// PNG-download helper voor canvas
window.downloadCanvasAsPng = (canvasRef, filename) => {
    if (!canvasRef || !canvasRef.toBlob) {
        console.error('Canvas reference invalid');
        return;
    }
    canvasRef.toBlob((blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }, 'image/png');
};
