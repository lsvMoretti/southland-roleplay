import * as alt from 'alt-client';
import * as natives from 'natives';

class AsyncModel {
    private loadingModels: Set<any>;
    constructor( ) {
        this.loadingModels = new Set( );
    }

    cancel( entityId:any ) {
        this.loadingModels.delete( +entityId );
    }

    async load( entityId:any, model:any ) {
        return new Promise( resolve => {
            if( typeof model === 'string' )
                model = natives.getHashKey( model );

            this.loadingModels.add( +entityId );

            natives.requestModel( model );

            const interval = alt.setInterval( () => {
                if( !this.loadingModels.has( +entityId ) ) {
                    return done( !!natives.hasModelLoaded( model ) );
                }

                if( natives.hasModelLoaded( model ) ) {
                    return done( true );
                }
            }, 0 );

            const timeout = alt.setTimeout( ( ) => {
                return done( !!natives.hasModelLoaded( model ) );
            }, 3000 );

            const done = (result:any) => {
                alt.clearInterval( interval );
                alt.clearTimeout( timeout );

                this.loadingModels.delete( +entityId );
                resolve( result );
            };

            if( !natives.isModelValid( model ) )
                return done( false );

            if( natives.hasModelLoaded( model ) )
                return done( true );
        } );
    }
}

export const asyncModel = new AsyncModel();