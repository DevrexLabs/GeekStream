/*
* XSockets.NET JavaScript Library v1.0.2.beta
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
var jXSocketsWorker = function (url) {
		/// <summary>
		/// XSockets.NET JavaScript API using WebWorkers 
		/// </summary>
		///<param name="url" type="String">
		/// The WebSocket handler (URL) to connect to. i.e ws://127.0.0.1:4502/GenericText
		///</param>
		///<param name="subprotocol" type="String">
		///     Subprotocol i.e Chat 
		///</param>                            
		var subscriptions = {
			list: {}
		}
		var worker = new Worker("/scripts/jXSocketsWorker.WebSocket-1.0.4.js");
		worker.postMessage({
			method: "open",
			settings: {
				url: url
			}
		});
        worker.onmessage = function (evt) {
            
			raiseEvent(evt)
		}
		this.trigger = function (event, json) {
			/// <summary>
			///      Trigger (Publish)  a WebSocketMessage (event) to the current WebSocket Handler.
			/// </summary>
			/// <param name="event" type="string">
			///     Name of the event (publish)
			/// </param>                
			/// <param name="json" type="JSON">
			///     JSON representation of the WebSocketMessage to trigger/send (publish)
			/// </param>                                
			var o = {
				method: "send",
				message: {
					event: event,
					data: json
				}
			}
			worker.postMessage(o);
		}
		this.bind = function (event, fn) {
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
			///    N/A
			/// </param>  
			subscriptions.list[event] = subscriptions.list[event] || [];
			subscriptions.list[event].push({
				callback: fn,
				options: null
			});
		}
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
		    for (var i = 0; i < subscriptions.list[eventName].length; i++) {
		        subscriptions.list[eventName].splice(i, 1);
		    }
		    if (callback && typeof (callback) === "function") {
		        callback();
		    }
		}
		this.many = function (event, count, callback) {
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
			subscriptions.list[event] = subscriptions.list[event] || [];
			subscriptions.list[event].push({
				callback: callback,
				options: {
					Counter: {
						Messages: count,
						Completed: function () {
							for(var i = 0; i < subscriptions.list[event].length; i++) {
								subscriptions.list[event].splice(i, 1);
								break;
							}
						}
					}
				}
			});
		}
		this.one = function (event, callback) {
			/// <summary>
			///    Attach a handler to an event (subscription) for the current WebSocket Handler. The handler is executed at most once.
			/// </summary>
			/// <param name="event" type="String">
			///    Name of the event (subscription)
			/// </param>           
			/// <param name="callback" type="Function">
			///    A function to trigger when executed once.
			/// </param>           
			subscriptions.list[event] = subscriptions.list[event] || [];
			subscriptions.list[event].push({
				callback: callback,
				options: {
					Counter: {
						Messages: 1,
						Completed: function () {
							for(var i = 0; i < subscriptions.list[event].length; i++) {
								subscriptions.list[event].splice(i, 1);
							}
						}
					}
				}
			});
		}
		var raiseEvent = function (message) {
				var event_name;
				if(typeof message.data === "string") {
					var msg = JSON.parse(message.data);
					event_name = msg.event;
					dispatch(event_name, msg.data);
				} else {
					dispatch("blob", message.data);
				}
			};
		var dispatch = function (event_name, message) {
				var chain = subscriptions.list[event_name];
				if(typeof chain == 'undefined') return;
				for (var i = 0; i < chain.length; i++) {
					chain[i].callback(message);
					var opts = chain[i].options;
					if(opts != null) {
						if(opts.Counter !== undefined) {
							chain[i].options.Counter.Messages--;
							if(chain[i].options.Counter.Messages == 0) {
								if(typeof opts.Counter.Completed !== 'undefined') {
									opts.Counter.Completed();
								}
							}
						}
					}
				}
			}
		
	}