// Download json as a file
self.downloadFile = function (file, type, content) {
    const blob = new Blob([new TextEncoder().encode(content)], {type: type});
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = file;
    document.body.appendChild(link);
    link.click();

    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

// Clear caches
self.cc = function () {
    caches.keys().then(function (names) {
        for (let name of names)
            caches.delete(name).then(function (success) {
                if (success) {
                    console.info("Deleted " + name)
                } else {
                    console.error("Failed to delete " + name)
                }
            });
    });
}
