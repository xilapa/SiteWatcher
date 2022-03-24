import { State } from './state'

export function saveState() {
    return (target: Object, propertyKey: string) => {
        
        const componentName = target.constructor.name;
        
        const setter = (newValue: any) => 
            State.SetComponentPropertyState(componentName, propertyKey, newValue);

        const getter = () =>
            State.GetComponentPropertySate(componentName, propertyKey);

        Object.defineProperty(target, propertyKey, {
            get: getter,
            set: setter
          }); 
    }
} 