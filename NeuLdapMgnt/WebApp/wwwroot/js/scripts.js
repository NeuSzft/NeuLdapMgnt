self.downloadJsonFile = function (file, json) {
    let link = document.createElement("a")
    link.download = file
    link.href = "data:application/json;charset=utf-8," + encodeURIComponent(json)
    link.style.display = "none"
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
}

self.eswc = function () {
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
