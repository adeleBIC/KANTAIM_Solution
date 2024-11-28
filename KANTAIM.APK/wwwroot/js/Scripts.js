function focusElementById(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}

//function Pages() {

//}

//window.addGlobalKeyListener = (dotNetHelper) => {
//    let capturing = false;
//    document.addEventListener('keydown', (event) => {

//        if (event.key === 'Enter') {
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressScanPage', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressFindProd', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressInit', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressInject', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressStock', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressTransfert', event.key);
//            capturing = false;
//        } else if (event.key === '!') {
//            capturing = true;
//            event.preventDefault();
//        } else if (capturing) {
//            if (event.key === 'AltGraph' || event.key === 'Shift' || event.key === 'Control' || event.key === 'Unidentified') {
//                return; // Ignore AltGr key or shift key itself
//            }
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressScanPage', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressFindProd', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressInit', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressInject', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressStock', event.key);
//            dotNetHelper.invokeMethodAsync('HandleGlobalKeyPressTransfert', event.key);
//        } 
//    });
//};

function Pages() {
    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInput', event.key);
    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputInit', event.key);
    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputFindProd', event.key);
    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputInject', event.key);
    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputTransfer', event.key);
    DotNet.invokeMethodAsync('KANTAIM.APK', 'CaptureInputStock', event.key);
}
window.addGlobalKeyListener = function () {
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
