export class MiddlewareList{

    getStringValueForEmum(middlewareName: MiddlewareName ){
       switch (middlewareName) {
        case  MiddlewareName.Protected:
            return 'protected';       
        default:
            return 'auth'
       }

    }
    
}

  export enum MiddlewareName {
  Protected,
  Auth}