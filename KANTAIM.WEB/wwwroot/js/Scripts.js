function focusElementById(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}

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