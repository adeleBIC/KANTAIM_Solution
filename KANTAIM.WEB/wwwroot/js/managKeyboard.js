// wwwroot/js/preventKeyboard.js
//window.preventKeyboard = function (elementId) {
//    var element = document.getElementById(elementId);
//    if (element) {
//        element.addEventListener('touchstart', function (event) {
//            event.preventDefault();
//            element.blur();
//        });
//    }
//};


// wwwroot/js/preventKeyboard.js
window.preventKeyboardOnTouch = function (elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        // Remove existing event listener if it exists to avoid multiple listeners
        element.removeEventListener('focus', preventFocus);
        element.addEventListener('focus', preventFocus);
    }
};

function preventFocus(event) {
    event.preventDefault();
    event.target.blur();
}

//document.addEventListener('keydown', function (event) {
//    if (event.key === '§') {
//        DotNet.invokeMethodAsync('KANTAIM.WEB', 'OnSpecialKeyPressed');
//    }
//});

//window.initializeKeyListener = function () {
//    document.addEventListener('keydown', function (event) {
//        if (event.key === '§') {
//            DotNet.invokeMethodAsync('KANTAIM.WEB', 'OnSpecialKeyPressed');
//        }
//    });
//};

window.initializeKeyListener = function () {
    let capturing = false;
    document.addEventListener('keydown', function (event) {

        if (event.key === '$') {
            capturing = false;
        } else if (event.key === '!') {
            DotNet.invokeMethodAsync('KANTAIM.WEB', 'OnSpecialKeyPressed');
            capturing = true;
            event.preventDefault();
        } else if (capturing) {
            DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInput', event.key);
        } 

    });
};