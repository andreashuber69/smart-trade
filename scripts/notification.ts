class Notification {
    public constructor(
        public readonly id: number,
        public readonly title: string,
        public readonly text: string,
        public readonly sound: string,
        public readonly firstAt: number,
        public readonly at: number,
        public readonly every: number,
        public readonly data: any) {
    }
}

interface Local {
    schedule(notifications: Notification | Notification[], callback?: (notification: Notification) => void, scope?: any, args?: any): void;
    on(event: 'trigger', callback: (notification: Notification) => void, scope?: any): void;
    on(event: 'click', callback: (notification: Notification) => void, scope?: any): void;
    hasPermission(callback: (granted: boolean) => void, scope?: any);
}

interface Notification {
    local: Local;
}

interface CordovaPlugins {
    notification: Notification;
}
