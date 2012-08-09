﻿// Copyright: Hiroshi Ichikawa <http://gimite.net/en/>
// License: New BSD License
// Reference: http://dev.w3.org/html5/websockets/
// Reference: http://tools.ietf.org/html/rfc6455
(function () {
    if (window.WebSocket) {
        return;
    } else if (window.MozWebSocket) {
        window.WebSocket = MozWebSocket;
        return;
    }
    var logger;
    if (window.WEB_SOCKET_LOGGER) {
        logger = WEB_SOCKET_LOGGER;
    } else if (window.console && window.console.log && window.console.error) {
        logger = window.console;
    } else {
        logger = {
            log: function () { },
            error: function () { }
        };
    }
    // swfobject.hasFlashPlayerVersion("10.0.0") doesn't work with Gnash.
    if (swfobject.getFlashPlayerVersion().major < 10) {
        logger.error("Flash Player >= 10.0.0 is required.");
        return;
    }
    window.WebSocket = function (url, protocols, proxyHost, proxyPort, headers) {
        var self = this;
        self.__id = WebSocket.__nextId++;
        WebSocket.__instances[self.__id] = self;
        self.readyState = WebSocket.CONNECTING;
        self.bufferedAmount = 0;
        self.__events = {};
        if (!protocols) {
            protocols = [];
        } else if (typeof protocols == "string") {
            protocols = [protocols];
        }
        self.__createTask = setTimeout(function () {
            WebSocket.__addTask(function () {
                self.__createTask = null;
                WebSocket.__flash.create(
				self.__id, url, protocols, proxyHost || null, proxyPort || 0, headers || null);
            });
        }, 0);
    };
    WebSocket.prototype.send = function (data) {
        if (this.readyState == WebSocket.CONNECTING) {
            throw "INVALID_STATE_ERR: Web Socket connection has not been established";
        }
        var result = WebSocket.__flash.send(this.__id, encodeURIComponent(data));
        if (result < 0) {
            return true;
        } else {
            this.bufferedAmount += result;
            return false;
        }
    };
    WebSocket.prototype.close = function () {
        if (this.__createTask) {
            clearTimeout(this.__createTask);
            this.__createTask = null;
            this.readyState = WebSocket.CLOSED;
            return;
        }
        if (this.readyState == WebSocket.CLOSED || this.readyState == WebSocket.CLOSING) {
            return;
        }
        this.readyState = WebSocket.CLOSING;
        WebSocket.__flash.close(this.__id);
    };
    WebSocket.prototype.addEventListener = function (type, listener, useCapture) {
        if (!(type in this.__events)) {
            this.__events[type] = [];
        }
        this.__events[type].push(listener);
    };
    WebSocket.prototype.removeEventListener = function (type, listener, useCapture) {
        if (!(type in this.__events)) return;
        var events = this.__events[type];
        for (var i = events.length - 1; i >= 0; --i) {
            if (events[i] === listener) {
                events.splice(i, 1);
                break;
            }
        }
    };
    WebSocket.prototype.dispatchEvent = function (event) {
        var events = this.__events[event.type] || [];
        for (var i = 0; i < events.length; ++i) {
            events[i](event);
        }
        var handler = this["on" + event.type];
        if (handler) handler.apply(this, [event]);
    };
    WebSocket.prototype.__handleEvent = function (flashEvent) {
        if ("readyState" in flashEvent) {
            this.readyState = flashEvent.readyState;
        }
        if ("protocol" in flashEvent) {
            this.protocol = flashEvent.protocol;
        }
        var jsEvent;
        if (flashEvent.type == "open" || flashEvent.type == "error") {
            jsEvent = this.__createSimpleEvent(flashEvent.type);
        } else if (flashEvent.type == "close") {
            jsEvent = this.__createSimpleEvent("close");
            jsEvent.wasClean = flashEvent.wasClean ? true : false;
            jsEvent.code = flashEvent.code;
            jsEvent.reason = flashEvent.reason;
        } else if (flashEvent.type == "message") {
            var data = decodeURIComponent(flashEvent.message);
            jsEvent = this.__createMessageEvent("message", data);
        } else {
            throw "unknown event type: " + flashEvent.type;
        }
        this.dispatchEvent(jsEvent);
    };
    WebSocket.prototype.__createSimpleEvent = function (type) {
        if (document.createEvent && window.Event) {
            var event = document.createEvent("Event");
            event.initEvent(type, false, false);
            return event;
        } else {
            return {
                type: type,
                bubbles: false,
                cancelable: false
            };
        }
    };
    WebSocket.prototype.__createMessageEvent = function (type, data) {
        if (document.createEvent && window.MessageEvent && !window.opera) {
            var event = document.createEvent("MessageEvent");
            event.initMessageEvent("message", false, false, data, null, null, window, null);
            return event;
        } else {
            return {
                type: type,
                data: data,
                bubbles: false,
                cancelable: false
            };
        }
    };
    WebSocket.CONNECTING = 0;
    WebSocket.OPEN = 1;
    WebSocket.CLOSING = 2;
    WebSocket.CLOSED = 3;
    WebSocket.__initialized = false;
    WebSocket.__flash = null;
    WebSocket.__instances = {};
    WebSocket.__tasks = [];
    WebSocket.__nextId = 0;
    WebSocket.loadFlashPolicyFile = function (url) {
        WebSocket.__addTask(function () {
            WebSocket.__flash.loadManualPolicyFile(url);
        });
    };
    WebSocket.__initialize = function () {
        if (WebSocket.__initialized) return;
        WebSocket.__initialized = true;
        var container = document.createElement("div");
        container.id = "webSocketContainer";
        container.style.position = "absolute";
        if (WebSocket.__isFlashLite()) {
            container.style.left = "0px";
            container.style.top = "0px";
        } else {
            container.style.left = "-100px";
            container.style.top = "-100px";
        }
        var holder = document.createElement("div");
        holder.id = "webSocketFlash";
        container.appendChild(holder);
        document.body.appendChild(container);
        swfobject.embedSWF("/scripts/FlashWebSocket.swf", "webSocketFlash", "1", "1", "10.0.0", null, null, {
            hasPriority: true,
            swliveconnect: true,
            allowScriptAccess: "always"
        }, null);
    };
    WebSocket.__onFlashInitialized = function () {
        setTimeout(function () {
            WebSocket.__flash = document.getElementById("webSocketFlash");
            WebSocket.__flash.setCallerUrl(location.href);
            WebSocket.__flash.setDebug(!!window.WEB_SOCKET_DEBUG);
            for (var i = 0; i < WebSocket.__tasks.length; ++i) {
                WebSocket.__tasks[i]();
            }
            WebSocket.__tasks = [];
        }, 0);
    };
    WebSocket.__onFlashEvent = function () {
        setTimeout(function () {
            try {
                var events = WebSocket.__flash.receiveEvents();
                for (var i = 0; i < events.length; ++i) {
                    WebSocket.__instances[events[i].webSocketId].__handleEvent(events[i]);
                }
            } catch (e) {
                logger.error(e);
            }
        }, 0);
        return true;
    };
    WebSocket.__log = function (message) {
        logger.log(decodeURIComponent(message));
    };
    WebSocket.__error = function (message) {
        logger.error(decodeURIComponent(message));
    };
    WebSocket.__addTask = function (task) {
        if (WebSocket.__flash) {
            task();
        } else {
            WebSocket.__tasks.push(task);
        }
    };
    WebSocket.__isFlashLite = function () {
        if (!window.navigator || !window.navigator.mimeTypes) {
            return false;
        }
        var mimeType = window.navigator.mimeTypes["application/x-shockwave-flash"];
        if (!mimeType || !mimeType.enabledPlugin || !mimeType.enabledPlugin.filename) {
            return false;
        }
        return mimeType.enabledPlugin.filename.match(/flashlite/i) ? true : false;
    };
    
    swfobject.addDomLoadEvent(function () {
            WebSocket.__initialize();
    });
    
})();