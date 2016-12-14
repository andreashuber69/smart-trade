
let hmac_sha256 = require('../node_modules/crypto-js/hmac-sha256.js');


export class Blah {
    schnapp(): void {
        cordova.plugins.notification.local.hasPermission(
            (granted: boolean) => { console.log(granted); })
        let x = hmac_sha256("Blah", "blap");
    }
}
