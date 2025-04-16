function focusElementById(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}


//function Pages() {
//    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInput', event.key);
//    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputInit', event.key);
//    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputFindProd', event.key);
//    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputInject', event.key);
//    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputTransfer', event.key);
//    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputStock', event.key);
//}
//window.addGlobalKeyListener = function () {
//    let capturing = false;
//    document.addEventListener('keydown', function (event) {

//        if (event.key === 'Enter') {
//            Pages();
//            capturing = false;
//        } else if (event.key === '!') {
//            capturing = true;
//            event.preventDefault();
//        } else if (capturing) {
//            if (event.key === 'AltGraph' || event.key === 'Shift' || event.key === 'Control' || event.key === 'Unidentified') {
//                return; // Ignore AltGr key or shift key itself
//            }
//            Pages();
//        }

//    });
//};

let scannedCode = '';
document.addEventListener('keydown', function (event) {

    evt = event || window.event;
    if (evt.key.length === 1) { // Vérifie si la touche produit un caractère
        if (evt.key == '!') scannedCode = '';
        else scannedCode += evt.key;
    }
    if (evt.key == 'Enter')
        DotNet.invokeMethodAsync('KANTAIM.APK', 'CodeScanned', scannedCode);

});