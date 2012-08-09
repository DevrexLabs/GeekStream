var ws = null;
onmessage = function (evt) {
    if (evt.data.method == "open") {
        ws = new WebSocket(evt.data.settings.url);
        ws.onopen = function () {
            postMessage();
        }
        ws.onclose = function () {
            postMessage();
        }
        ws.onmessage = function (evt) {
            postMessage(evt.data);
        }
    } else if (evt.data.method == "send") {
        ws.send(JSON.stringify(evt.data.message));
    }
}