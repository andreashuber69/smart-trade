import { Component } from '@angular/core';
import hashes = require('crypto-js');

@Component({
    moduleId: module.id,
    selector: 'my-dashboard',
    templateUrl: 'dashboard.component.html',
    styleUrls: [ 'dashboard.component.css' ]
})
export class DashboardComponent {
    public customerId: number;
    public apiKey: string;
    public apiSecret: string;

    public doIt(): void {
        let result = hashes.HmacSHA1("Hello", "there");
        console.log(result);
    }

    public constructor() {
    }
}