let app = {
    initialize(): void {
        document.addEventListener('deviceready', () => setTimeout(() => this.doIt(), 2000), false);
    },

    doIt() : void {
        let button = <HTMLButtonElement>document.getElementById('goButton');
        button.addEventListener('click', function() { console.log('yeei'); });
    }
};

app.initialize();