//window.preventKeyboardOnTouch = function (elementId) {
//    var element = document.getElementById(elementId);
//    if (element) {
//        // Remove existing event listener if it exists to avoid multiple listeners
//        element.removeEventListener('focus', preventFocus);
//        element.addEventListener('focus', preventFocus);
//    }
//};

//function preventFocus(event) {
//    event.preventDefault();
//    event.target.blur();
//}

window.initializeKeyListener = function () {
    let capturing = false;
    document.addEventListener('keydown', function (event) {

        if (event.key === 'Enter') {
            DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInput', event.key);
            DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputInit', event.key);
            capturing = false;
        } else if (event.key === '!') {
            //DotNet.invokeMethodAsync('KANTAIM.WEB', 'OnSpecialKeyPressed');
            capturing = true;
            event.preventDefault();
        } else if (capturing) {
            /*
            if (event.key === 'Enter') {
                console.log("Enter !");
                event.preventDefault();
                // Invoke the .NET method to handle Enter key press
                DotNet.invokeMethodAsync('KANTAIM.WEB', 'TextfieldUserInputDetected');
            } else {
            */
            if (event.key === 'AltGraph' || event.key === 'Shift' || event.key === 'Control' || event.key === 'Unidentified') {
                return; // Ignore AltGr key or shift key itself
            }
            DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInput', event.key);
            DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputInit', event.key);
            //}
            
        } 

    });
};