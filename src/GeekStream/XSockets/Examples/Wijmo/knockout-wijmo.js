/*
*
* Wijmo KnockoutJS Binding Library 2.0.8
* http://wijmo.com/
*
* Copyright(c) ComponentOne, LLC.  All rights reserved.
* 
* Dual licensed under the Wijmo Commercial or GNU GPL Version 3 licenses.
* licensing@wijmo.com
* http://wijmo.com/license
*
*
* * Wijmo KnockoutJS Binding Factory.
*
* Depends:
*  knockoutjs.js
*
*/
(function ($, ko) {

    //extend ko.numericObservable
    ko.numericObservable = function (initialValue) {
        var _actual = ko.observable(initialValue);

        var result = ko.dependentObservable({
            read: function () {
                return _actual();
            },
            write: function (newValue) {
                var parsedValue = parseFloat(newValue);
                _actual(isNaN(parsedValue) ? newValue : parsedValue);
            }
        });

        return result;
    };

    ko.wijmo = ko.wijmo || {};

    ko.wijmo.customBindingFactory = function () {
        var self = this;

        self.customBinding = function (options) {
            var binding = {},
				widgetName = options.widgetName,
				widget,
				updatingFromEvents = false,
				updatingFromOtherObservables = false;

            binding.init = function (element, valueAccessor, allBindingAccessor, viewModel) {
                //element: The DOM element involved in this binding
                //valueAccessor: A JavaScript function that you can call to get the current model property 
                //	that is involved in this binding. Call this without passing any parameters 
                //	(i.e., call valueAccessor()) to get the current model property value.
                //allBindingsAccessor: A JavaScript function that you can call to get all the model properties 
                //	bound to this DOM element. Like valueAccessor, call it without any parameters to get the 
                //	current bound model properties.
                //viewModel: The view model object that was passed to ko.applyBindings. 
                //	Inside a nested binding context, this parameter will be set to the current data item 
                //	(e.g., inside a with: person binding, viewModel will be set to person).
                var va = ko.utils.unwrapObservable(valueAccessor()),
					opts;
                //init widget
                var opts = ko.toJS(va);
                widget = $(element)[widgetName](opts).data(widgetName);

                $.each(va, function (key, value) {
                    if (!options.observableOptions || !options.observableOptions[key]) {
                        return true;
                    }
                    var observableOption = options.observableOptions[key],
						optType = observableOption.type;
                    /*
                    * ko.computed can't observe the value like "value: percent() * 100",
                    * So it should be removed and observe values in custombindings.update now. 
                    ko.computed({
                    read: function () {
                    var val = ko.toJS(ko.utils.unwrapObservable(value));
                    if (updatingFromEvents) {
                    return;
                    }
                    if (optType && optType === 'numeric') {
                    var parsedVal = parseFloat(val);
                    val = isNaN(parsedVal) ? val : parsedVal;
                    }
                    updatingFromOtherObservables = true;
                    $(element)[widgetName]("option", key, val);
                    updatingFromOtherObservables = false;
                    },
                    disposeWhenNodeIsRemoved: element
                    });
                    */
                    if (!ko.isObservable(value)) {
                        return true;
                    }
                    //attach event.
                    var attachEvents = observableOption.attachEvents;
                    if (attachEvents) {
                        $.each(attachEvents, function (idx, ev) {
                            ko.utils.registerEventHandler(element, widgetName + ev, function () {
                                if (updatingFromOtherObservables) {
                                    return;
                                }
                                updatingFromEvents = true;
                                var newVal = $(element)[widgetName]("option", key);
                                //if (optType && optType === 'array' && value.removeAll) {
                                //	//value.removeAll();
                                //	//ko.utils.arrayPushAll(value, newVal);
                                //	//////take advantage of push accepting variable arguments
                                //	////value.push.apply(value, newItems);   
                                //	value(newVal);
                                //} else {
                                //	value(newVal);
                                //}
                                value(newVal);
                                updatingFromEvents = false;
                            });
                        });
                    }
                    //if (ko.isObservable(value) && value.subscribe) {
                    //	value.subscribe(function (newValue) {
                    //	});
                    //}
                });
            };

            binding.update = function (element, valueAccessor, allBindingAccessor, viewModel) {
                //element: The DOM element involved in this binding
                //valueAccessor: A JavaScript function that you can call to get the current model property 
                //	that is involved in this binding. Call this without passing any parameters 
                //	(i.e., call valueAccessor()) to get the current model property value.
                //allBindingsAccessor: A JavaScript function that you can call to get all the model properties 
                //	bound to this DOM element. Like valueAccessor, call it without any parameters to get the 
                //	current bound model properties.
                //viewModel: The view model object that was passed to ko.applyBindings. 
                //	Inside a nested binding context, this parameter will be set to the current data item 
                //	(e.g., inside a with: person binding, viewModel will be set to person).
                if (updatingFromEvents) {
                    return;
                }
                var valueUnwrapped = ko.utils.unwrapObservable(valueAccessor());
                $.each(valueUnwrapped, function (key, value) {
                    //The observable can be used like following: style: { width: percentMax() * 100 + '%' },
                    //the style.width is not an observable value and cannot be observed in ko.computed.
                    //So we need to check if the value is updated in binding.update.
                    var observableOption = options.observableOptions[key];
                    if (observableOption) {
                        var optType = observableOption.type;
                        val = ko.toJS(ko.utils.unwrapObservable(value)),
                    	widgetVal = $(element)[widgetName]("option", key);

                        if (optType && optType === 'numeric') {
                            var parsedVal = parseFloat(val);
                            val = isNaN(parsedVal) ? val : parsedVal;
                        }
                        if (!equals(val, widgetVal)) {
                            updatingFromOtherObservables = true;
                            $(element)[widgetName]("option", key, val);
                            updatingFromOtherObservables = false;
                        }
                    }

                });
            };

            equals = function (sourceValue, targetValue) {
                var equal = false;
                if (sourceValue === null) {
                    return false;
                }
                if (sourceValue === targetValue) {
                    return true;
                }
                if ((targetValue === null) || (sourceValue.constructor !== targetValue.constructor)) {
                    return false;
                }
                if ($.isPlainObject(sourceValue)) {
                    equal = true;
                    $.each(sourceValue, function (key, val) {
                        if (typeof targetValue[key] === 'undefined') {
                            equal = false;
                            return false;
                        }
                        if (!equals(val, targetValue[key])) {
                            equal = false;
                            return false;
                        }
                    });
                } else if ($.isArray(sourceValue)) {
                    if (sourceValue.length !== targetValue.length) {
                        return false;
                    }
                    equal = true;
                    $.each(sourceValue, function (idx, val) {
                        if (!equals(val, targetValue[idx])) {
                            equal = false;
                            return false;
                        }
                    });
                } else if (isDate(sourceValue)) {
                    return sourceValue == targetValue;
                }
                return equal;
            };

            isDate = function (obj) {
                if (!obj) {
                    return false;
                }
                return (typeof obj === 'object') && obj.constructor === Date;
            };

            ko.bindingHandlers[options.widgetName] = binding;
        };
    };

    ko.wijmo.customBindingFactory = new ko.wijmo.customBindingFactory();

    var createCustomBinding = ko.wijmo.customBindingFactory.customBinding.bind(ko.wijmo.customBindingFactory);
    createCustomBinding({
        widgetName: "wijbarchart",
        observableOptions: {
            disabled: {},
            stacked: {},
            header: {
            },
            seriesList: {
                type: 'array',
                attachEvents: ['serieschanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijbubblechart",
        observableOptions: {
            disabled: {},
            seriesList: {
                type: 'array',
                attachEvents: ['serieschanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijcompositechart",
        observableOptions: {
            disabled: {},
            seriesList: {
                type: 'array',
                attachEvents: ['serieschanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijlinechart",
        observableOptions: {
            disabled: {},
            seriesList: {
                type: 'array',
                attachEvents: ['serieschanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijpiechart",
        observableOptions: {
            disabled: {},
            seriesList: {
                type: 'array',
                attachEvents: ['serieschanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijscatterchart",
        observableOptions: {
            disabled: {},
            seriesList: {
                type: 'array',
                attachEvents: ['serieschanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijlineargauge",
        observableOptions: {
            disabled: {},
            min: {
                type: 'numeric'
            },
            max: {
                type: 'numeric'
            },
            value: {
                type: 'numeric'
            },
            ranges: {
                type: 'array'
            }
        }
    });

    createCustomBinding({
        widgetName: "wijradialgauge",
        observableOptions: {
            disabled: {},
            min: {
                type: 'numeric'
            },
            max: {
                type: 'numeric'
            },
            value: {
                type: 'numeric'
            },
            ranges: {
                type: 'array'
            }
        }
    });

    createCustomBinding({
        widgetName: "wijslider",
        observableOptions: {
            disabled: {},
            animate: {},
            max: {
                type: 'numeric'
            },
            min: {
                type: 'numeric'
            },
            orientation: {},
            range: {},
            step: {
                type: 'numeric'
            },
            value: {
                type: 'numeric',
                attachEvents: ['change', 'slide']
            },
            values: {
                type: 'array',
                attachEvents: ['change', 'slide']
            },
            dragFill: {},
            minRange: {
                type: 'numeric'
            }
        }
    });

    createCustomBinding({
        widgetName: "wijprogressbar",
        observableOptions: {
            disabled: {},
            value: {
                type: 'numeric',
                attachEvents: ['change']
            },
            labelAlign: {},
            maxValue: {
                type: 'numeric'
            },
            minValue: {
                type: 'numeric'
            },
            fillDirection: {},
            orientation: {},
            labelFormatString: {},
            toolTipFormatString: {},
            indicatorIncrement: {
                type: 'numeric'
            },
            indicatorImage: {},
            animationDelay: {
                type: 'numeric'
            },
            animationOptions: {}
        }
    });

    createCustomBinding({
        widgetName: "wijrating",
        observableOptions: {
            disabled: {},
            min: {
                type: 'numeric'
            },
            max: {
                type: 'numeric'
            },
            value: {
                type: 'numeric',
                attachEvents: ['rated', 'reset']
            },
            count: {
                type: 'numeric'
            },
            totalValue: {
                type: 'numeric'
            },
            split: {
                type: 'numeric'
            }
        }
    });

    createCustomBinding({
        widgetName: "wijgallery",
        observableOptions: {
            disabled: {},
            autoPlay: {},
            showTimer: {},
            interval: {
                type: 'numeric'
            },
            showCaption: {},
            //data: {
            //	type: 'array'
            //},
            showCounter: {},
            showPager: {},
            thumbnails: {},
            thumbsDisplay: {
                type: 'numeric'
            }
        }
    });

    createCustomBinding({
        widgetName: "wijcarousel",
        observableOptions: {
            disabled: {},
            auto: {},
            showTimer: {},
            interval: {
                type: 'numeric'
            },
            loop: {},
            //data: {
            //	type: 'array'
            //},
            showPager: {},
            showCaption: {},
            display: {
                type: 'numeric'
            },
            preview: {},
            step: {
                type: 'numeric'
            }
        }
    });

    createCustomBinding({
        widgetName: "wijsplitter",
        observableOptions: {
            disabled: {},
            showExpander: {},
            splitterDistance: {
                type: 'numeric',
                attachEvents: ['sized']
            },
            fullSplit: {}
        }
    });

    createCustomBinding({
        widgetName: "wijsuperpanel",
        observableOptions: {
            disabled: {},
            allowResize: {},
            autoRefresh: {},
            mouseWheelSupport: {},
            showRounder: {}
        }
    });

    createCustomBinding({
        widgetName: "wijtooltip",
        observableOptions: {
            disabled: {},
            closeBehavior: {},
            mouseTrailing: {},
            showCallout: {},
            showDelay: {
                type: 'numeric'
            },
            hideDelay: {
                type: 'numeric'
            },
            calloutFilled: {},
            modal: {},
            triggers: {}
        }
    });

    createCustomBinding({
        widgetName: "wijvideo",
        observableOptions: {
            disabled: {},
            fullScreenButtonVisible: {},
            showControlsOnHover: {}
        }
    });

    createCustomBinding({
        widgetName: "wijtabs",
        observableOptions: {
            disabled: {},
            collapsible: {}
        }
    });

    createCustomBinding({
        widgetName: "wijexpander",
        observableOptions: {
            disabled: {},
            allowExpand: {},
            expanded: {
                attachEvents: ['aftercollapse', 'afterexpand']
            },
            expandDirection: {}
        }
    });

    createCustomBinding({
        widgetName: "wijdialog",
        observableOptions: {
            disabled: {},
            autoOpen: {},
            draggable: {},
            modal: {},
            resizable: {}
        }
    });

    createCustomBinding({
        widgetName: "wijcalendar",
        observableOptions: {
            disabled: {},
            showTitle: {},
            showWeekDays: {},
            showWeekNumbers: {},
            showOtherMonthDays: {},
            showDayPadding: {},
            allowPreview: {},
            allowQuciPick: {},
            popupMode: {}
        }
    });

    createCustomBinding({
        widgetName: "wijaccordion",
        observableOptions: {
            disabled: {},
            requireOpenedPane: {},
            selectedIndex: {
                attachEvents: ['selectedindexchanged']
            }
        }
    });

    createCustomBinding({
        widgetName: "wijtree",
        observableOptions: {
            disabled: {},
            allowTriState: {},
            autoCheckNodes: {},
            autoCollapse: {},
            showCheckBoxes: {},
            showExpandCollapse: {}
        }
    });

    createCustomBinding({
        widgetName: "wijgrid",
        observableOptions: {
            disabled: {},
            data: {
                type: 'array'
            }
        }
    });
} (jQuery, ko));