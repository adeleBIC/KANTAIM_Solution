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

function Pages() {
    DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInput', event.key);
    DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputInit', event.key);
    DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputFindProd', event.key);
    DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputInject', event.key);
    DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputTransfer', event.key);
    DotNet.invokeMethodAsync('KANTAIM.WEB', 'CaptureInputStock', event.key);
}
window.initializeKeyListener = function () {
    let capturing = false;
    document.addEventListener('keydown', function (event) {

        if (event.key === 'Enter') {
            Pages();
            capturing = false;
        } else if (event.key === '!') {
            capturing = true;
            event.preventDefault();
        } else if (capturing) {
            if (event.key === 'AltGraph' || event.key === 'Shift' || event.key === 'Control' || event.key === 'Unidentified') {
                return; // Ignore AltGr key or shift key itself
            }
            Pages(); 
        } 

    });
};

let scannedCode = '';
document.addEventListener('keydown', function (event) {

    evt = event || window.event;
    if (evt.key.length === 1) { // Vérifie si la touche produit un caractère
        if (evt.key == '!') scannedCode = '';
        else scannedCode += evt.key;
    }
    if (evt.key == 'Enter')
        DotNet.invokeMethodAsync('KANTAIM.WEB', 'CodeScanned', scannedCode);

});