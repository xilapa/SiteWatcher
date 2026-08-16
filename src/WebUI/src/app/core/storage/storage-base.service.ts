export abstract class StorageBaseService {

    private storage: Storage;

    constructor(storage: Storage){
        this.storage = storage;
    }

    public setItem = (key: string, value: any): void =>
        this.storage.setItem(key, value);

    public getItem = (key: string): any =>
        this.storage.getItem(key);

    public removeItem = (key: string): void =>
        this.storage.removeItem(key);

    public getAndRemove = (key: string): any => {
        const value = this.getItem(key);
        this.removeItem(key);
        return value;
    }
}