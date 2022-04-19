export class Data {
    private static _values = new Map<string, any>();
    
    public static Get = (key: string): any =>
        this._values.get(key);    

    public static Share = (key: string, value: any): void => {
        this._values.set(key, value)
    }

    public static GetAndRemove = (key: string): any => {
        const value = this._values.get(key);
        this._values.delete(key);
        return value;
    }
    
    public static ClearAllData = (): void =>
        this._values.clear();    

    public static RemoveByKeys = (...keys: string[]): void =>
        keys.forEach(key => this._values.delete(key));
}