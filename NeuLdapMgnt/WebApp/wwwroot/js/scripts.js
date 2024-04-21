window.downloadJsonFile = function (file, json) {
    let link = document.createElement("a")
    link.download = file
    link.href = "data:application/json;charset=utf-8," + encodeURIComponent(json)
    link.style.display = "none"
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
}
