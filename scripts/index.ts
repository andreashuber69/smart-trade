let app = {
    initialize(): void {
        // Delay to allow the debugger to attach itself
        document.addEventListener('deviceready', () => setTimeout(() => this.deviceReady(), 2000), false);
    },

    deviceReady(): void {
        let button = <HTMLButtonElement>document.getElementById('goButton');
        button.onclick = this.onClick;
    },

    onClick(ev: MouseEvent): void {
        cordova.plugins.notification.local.schedule({ title: 'Test', text: 'Hurrah!', at: Date.now() + 10000 });
    }
};

app.initialize();