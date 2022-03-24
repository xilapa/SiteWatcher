export class State {
    private static _componentValues = new Map<string, Map<string, any>>();

    public static GetComponentPropertySate = (componentName: string, propertyName: string): any =>
        this._componentValues.get(componentName)?.get(propertyName);
 
    public static SetComponentPropertyState = (componentName: string, propertyName: string, value: any) : void  => {
        if (!this._componentValues.has(componentName))
            this._componentValues.set(componentName, new Map<string, any>());
 
        this._componentValues.get(componentName)?.set(propertyName, value);        
    }

    public static ClearAllComponentStates = (): void =>
        this._componentValues.clear();
    
    public static ClearComponentState = (componentName: string): void =>
        this._componentValues.get(componentName)?.clear();
    
    public static ClearComponentPropertiesStates = (componentName: string, ...propertyNames: string[]): void =>
        propertyNames.forEach(property => this._componentValues.get(componentName)?.delete(property));   
}