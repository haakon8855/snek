window.JsFunctions = {
    addKeyboardListenerEvent: function (foo) {
        let serializeEvent = function (e) {
            if (e) {
                return {
                    key: e.key,
                    code: e.keyCode.toString(),
                    location: e.location,
                    repeat: e.repeat,
                    ctrlKey: e.ctrlKey,
                    shiftKey: e.shiftKey,
                    altKey: e.altKey,
                    metaKey: e.metaKey,
                    type: e.type
                };
            }
        };
        window.document.addEventListener('keydown', function (e) {
            DotNet.invokeMethodAsync('Web.Client', 'JsKeyDown', serializeEvent(e));
        });
        window.document.addEventListener('keyup', function (e) {
            DotNet.invokeMethodAsync('Web.Client', 'JsKeyUp', serializeEvent(e));
        });
    }
};

