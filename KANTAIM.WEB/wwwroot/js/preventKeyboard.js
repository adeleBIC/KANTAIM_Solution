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
        element.removeEventListener('touchstart', preventFocus);
        element.addEventListener('touchstart', preventFocus);
    }
};

function preventFocus(event) {
    event.preventDefault();
    event.target.blur();
}