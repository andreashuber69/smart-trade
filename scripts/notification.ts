interface NotificationInfo {
    id?: number;
    title?: string;
    text?: string;

    // number is only allowed on Android and specifies an interval in minutes
    every?: 'second' | 'minute' | 'hour' | 'day' | 'week' | 'month' | 'year' | number;
    at?: Date | number;
    firstAt?: Date | number;
    badge?: number;
    sound?: string;
    data?: string;

    // Android-specific
    icon?: string;
    smallIcon?: string;
    ongoing?: boolean;
    led?: string; // RGB value from 000000 to FFFFFF encoding the color of the device LED.
}

interface Local {
    schedule(
        notifications: NotificationInfo | NotificationInfo[],
        callback?: (notification: NotificationInfo) => void,
        scope?: any,
        args?: any): void;
    on(
        event: 'schedule' | 'update' | 'clear' | 'cancel' | 'trigger' | 'click',
        callback: (notification: NotificationInfo) => void,
        scope?: any): void;
    hasPermission(
        callback: (granted: boolean) => void, scope?: any);
}

interface Notification {
    local: Local;
}

interface CordovaPlugins {
    notification: Notification;
}
