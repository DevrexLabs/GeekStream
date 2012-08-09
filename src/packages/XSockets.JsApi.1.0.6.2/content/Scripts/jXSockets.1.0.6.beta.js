/*
* XSockets.NET JavaScript Library v1.0.6.beta
* http://xsockets.net/
* Distributed in whole under the terms of the MIT
 
*
* Copyright 2012, Magnus Thor & Ulf Björklund
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
 
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
 
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
* LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 
* OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
* WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*/
(function () {
    "use strict";
    var jXSockets = {


        Events: {
            onError: "xsockets.onerror",
            open: "xsockets.xnode.open",
            close: "close",
            storage: {
                set: "xsockets.storage.set",
                get: "xsockets.storage.get",
                getAll: "xsockets.storage.getall",
                remove: "xsockets.storage.remove"
            },
            onBlob: "blob"
        },

        WebSocket: function (url, subprotocol) {
            /// <summary>
            /// XSockets.NET JavaScript API 
            /// </summary>

            ///<param name="url" type="String">
            /// The WebSocket handler (URL) to connect to. i.e ws://127.0.0.1:4502/GenericText
            ///</param>

            ///<param name="subprotocol" type="String">
            /// Subprotocol i.e Chat 
            ///</param>          


            var self = this;

            var webSocket = null;
            var subscriptions = [];
            var events = [];
            var pubSub = {
                subscribe: "xsocket.subscribe",
                unsubscribe: "xsocket.unsubscribe",
                getSubscriptions: "xsocket.getsubscriptions",
                getAllSubscriptions: "xsocket.getallsubscriptions"
            };



            this.bind = function (event, fn, options, callback) {
                /// <summary>
                ///     Attach a handler (subscription) for the current WebSocket Handler
                /// </summary>

                /// <param name="event" type="string">
                ///    Name of the event to subscribe to (bind)
                /// </param> 

                /// <param name="fn" type="function">
                ///    A function to execute each time the event (subscription) is triggered.
                /// </param>   

                /// <param name="options" type="object">
                ///   event (subscriptions) options
                /// </param>

                /// <param name="callback" type="function">
                ///    A function to execute when completed.
                /// </param>
                event = event.toLowerCase();
                events.push(event);
                var o = {

                    callback: fn,
                    options: options,
                    ready: webSocket.readyState
                };

                if (o.ready === 1) {

                    self.trigger(new this.Message(pubSub.subscribe, {
                        Event: event
                    }));
                }
                subscriptions[event] = subscriptions[event] || [];
                subscriptions[event].push(o);
                if (callback && typeof (callback) === "function") {
                    callback();
                }
            };
            this.unbind = function (event, callback) {
                /// <summary>
                ///     Remove a previously-attached event handler (subscription).
                /// </summary>
                /// <param name="event" type="String">
                ///    Name of the event (subscription) to unbind.
                /// </param>           

                /// <param name="callback" type="function">
                ///    A function to execute when completed.
                /// </param>   

                event = event.toLowerCase();
                for (var i = 0; i < subscriptions[event].length; i++) {
                    self.trigger(new self.Message(pubSub.unsubscribe, {
                        Event: event
                    }));
                    subscriptions[event].splice(i, 1);
                }
                events.removeItem(event);
                if (callback && typeof (callback) === "function") {
                    callback();
                }
            };
            this.many = function (event, count, callback, options) {
                /// <summary>
                ///     Attach a handler to an event (subscription) for the current WebSocket Handler,  unbinds when the event is triggered the specified number of (count) times.
                /// </summary>

                /// <param name="event" type="String">
                ///    Name of the event (subscription)
                /// </param>           

                /// <param name="count" type="Number">
                ///     Number of times to listen to this event (subscription)
                /// </param>           

                /// <param name="callback" type="Function">
                ///    A function to execute at the time the event is triggered the specified number of times.
                /// </param> 

                /// <param name="options" type="object">
                ///   event (subscriptions) options
                /// </param>
                event = event.toLowerCase();
                subscriptions[event] = subscriptions[event] || [];
                self.bind(event, callback, extend({

                    counter: {
                        messages: count,
                        completed: function () {
                            self.unbind(event);
                        }

                    }
                }, options));
            };
            this.one = function (event, callback, options) {
                /// <summary>
                ///    Attach a handler to an event (subscription) for the current WebSocket Handler. The handler is executed at most once.
                /// </summary>

                /// <param name="event" type="String">
                ///    Name of the event (subscription)
                /// </param>           

                /// <param name="callback" type="Function">
                ///    A function to trigger when executed once.
                /// </param>       

                /// <param name="options" type="object">
                ///   event (subscriptions) options
                /// </param>  
                event = event.toLowerCase();
                subscriptions[event] = subscriptions[event] || [];
                self.bind(event, callback, extend({

                    counter: {
                        messages: 1,
                        completed: function () {
                            self.unbind(event);
                        }

                    }
                }, options));
            };
            this.trigger = function (event, json, callback) {
                /// <summary>
                ///      Trigger (Publish)  a WebSocketMessage (event) to the current WebSocket Handler.
                /// </summary>

                /// <param name="event" type="string">
                ///     Name of the event (publish)
                /// </param>                

                /// <param name="json" type="JSON">
                ///     JSON representation of the WebSocketMessage to trigger/send (publish)
                /// </param>

                /// <param name="callback" type="function">
                ///      A function to execute when completed. 
                /// </param>


                if (typeof (event) !== "object") {
                    event = event.toLowerCase();
                    var message = self.Message(event, json);

                    send(message.toString());
                    if (callback && typeof (callback) === "function") {

                        callback();
                    }
                } else {
                    send(event.toString());
                    if (json && typeof (json) === "function") {

                        json();
                    }
                }
            };
            this.send = function (payload) {
                /// <summary>
                ///     Send a binary message to the current WebSocket Handler
                /// </summary>
                /// <param name="payload" type="Blob">
                ///     Binary object to send (Blob/ArrayBuffer).
                /// </param>  
                webSocket.send(payload);
            };
            this.Message = function (event, object) {
                /// <summary>
                ///     Create a new WebSocketMessage
                /// </summary>

                /// <param name="event" type="string">
                ///     Name of the event
                /// </param>             

                /// <param name="object" type="object">
                ///     The message payload (JSON)
                /// </param>                              
                var json = {
                    event: event,
                    data: JSON.stringify(object)

                };
                return {
                    JSON: json,
                    toString: function () {
                        return JSON.stringify(this.JSON);

                    }
                };
            };
            var dispatch = function (eventName, message) {
                eventName = eventName.toLowerCase();
                var chain = subscriptions[eventName];

                if (typeof message === "string") {
                    message = JSON.parse(message);
                }
                if (typeof chain === 'undefined') {
                    return;

                }
                for (var i = 0; i < chain.length; i++) {
                    chain[i].callback(message);
                    var opts = chain[i].options;

                    if (typeof opts === "object") {
                        if (typeof opts.counter !== "undefined") {

                            chain[i].options.counter.messages--;
                            if (chain[i].options.counter.messages === 0) {
                                if (typeof opts.counter.completed !== 'undefined') {

                                    opts.counter.completed();
                                }
                            }
                        }
                    }
                }
            };
            var extend = function (obj, extObj) {
                if (arguments.length > 2) {
                    for (var a = 1; a < arguments.length; a++) {

                        extend(obj, arguments[a]);
                    }
                } else {
                    for (var i in extObj) {

                        obj[i] = extObj[i];
                    }
                }
                return obj;
            };
            var raiseEvent = function (message) {


                var event = null;
                if (typeof message.data === "string") {

                    var msg = JSON.parse(message.data);
                    event = msg.event;
                    dispatch(event, msg.data);
                } else {
                    dispatch("blob", message.data);

                }
            };
            var send = function (payload) {
                webSocket.send(payload);
            };

            Array.prototype.removeItem = function (str) {
                for (var i = 0; i < this.length; i++) {
                    if (escape(this[i]).match(escape(str.trim()))) {
                        this.splice(i, 1);
                        break;
                    }
                }
                return this;
            };


            if ('WebSocket' in window) {
                var clientGuid = window.localStorage.getItem("XSocketsClientGuid");
                if (clientGuid !== null) {

                    webSocket = new window.WebSocket(url + "?XSocketsClientGuid=" + clientGuid, subprotocol);
                } else {
                    webSocket = new window.WebSocket(url, subprotocol);
                }
            } else {
                webSocket = null;
            }


            if (webSocket !== null) {

                self.bind(jXSockets.Events.open, function (data) {
                    window.localStorage.setItem("XSocketsClientGuid", data.Guid);
                    for (var e = 0; e < events.length; e++) {
                        for (var i = 0; i < subscriptions[events[e]].length; i++) {
                            if (subscriptions[events[e]][i].ready !== 1) {
                                self.trigger(new self.Message(pubSub.subscribe, {
                                    Event: events[e]
                                }));
                            }
                        }
                    }
                }, {
                    subscribe: false
                });

                webSocket.onclose = function (msg) {
                    dispatch('close', msg);
                };
                webSocket.onopen = function (msg) {
                    dispatch('open', msg);
                };
                webSocket.onmessage = function (message) {

                    raiseEvent(message);
                };

            }

            return self;
        }
    };
    if (!window.jXSockets) {
        window.jXSockets = jXSockets;
    }
    if (!window.XSockets) {
        window.XSockets = jXSockets;
    }
})();