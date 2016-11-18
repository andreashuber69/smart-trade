import { Component } from '@angular/core';

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

    }

    public constructor() {
    }
}