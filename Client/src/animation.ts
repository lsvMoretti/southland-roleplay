import * as alt from 'alt-client';
import * as native from 'natives';

const maxCountLoadTry = 255;

alt.onServer('animation:StartAnimation', startAnimation);
alt.onServer('animation:StopAnimation', stopAnimation);
alt.onServer('animation:StartScenario', startScenario);

export function startScenario(scenario:string){
    native.taskStartScenarioInPlace(alt.Player.local.scriptID, scenario, 0, true);
}

export function stopAnimation() {
    var scriptId = alt.Player.local.scriptID;

    native.clearPedTasks(scriptId);
}

export function startAnimation(dict: string, name: string, duration: number, flag: number, ped:any = undefined) {
    
    let scriptId = alt.Player.local.scriptID;
    
    if(ped !== undefined){
        scriptId = ped.scriptID;
    }
   
    native.clearPedTasks(scriptId);

    if (native.hasAnimDictLoaded(dict)) {
        native.taskPlayAnim(
            scriptId,
            dict,
            name,
            1,
            -1,
            duration,
            flag,
            1.0,
            false,
            false,
            false
        );
        return;
    }

    let result = loadAnimation(dict);

    result.then(() => {
        native.taskPlayAnim(
            scriptId,
            dict,
            name,
            1,
            -1,
            duration,
            flag,
            1.0,
            false,
            false,
            false
        );
    });
}

function loadAnimation(dict: string) {
    return new Promise(resolve => {
        native.requestAnimDict(dict);

        let count = 0;
        let inter = alt.setInterval(() => {
            if (count > maxCountLoadTry) {
                alt.clearInterval(inter);
                return;
            }

            if (native.hasAnimDictLoaded(dict)) {
                resolve(true);
                alt.clearInterval(inter);
                return;
            }

            count += 1;
        }, 5);
    });
}

alt.onServer('PlayScenario', playScenario);
alt.onServer('StopScenario', stopScenario);

function playScenario(scenarioName: string) {
    native.taskStartScenarioInPlace(alt.Player.local.scriptID, scenarioName, 0, true);
}

function stopScenario() {
    native.clearPedTasks(alt.Player.local.scriptID);
}

alt.onServer('Animation:Surrender', startSurrender);

let surrendered:boolean = false;
let LastAD:string = undefined;
let playerCurrentlyAnimated = false;

function startSurrender(){

    const player = alt.Player.local.scriptID;

    const ad:string = "random@arrests";
    const ad2:string = "random@arrests@busted";

    native.requestAnimDict(ad);
    native.requestAnimDict(ad2);

    if(native.isEntityPlayingAnim(player, ad2, 'idle+a', 3)){
        native.taskPlayAnim(player, ad2, "exit", 8.0, 1.0, -1, 2, 0, false, false, false);
        alt.setTimeout(() => {
            native.taskPlayAnim(player, ad, "kneeling_arrest_get_up", 8.0, 1.0, -1, 128, 0, false, false, false);
            native.removeAnimDict("random@arrests@busted");
            native.removeAnimDict("random@arrests");
            surrendered = false
			LastAD = ad
			playerCurrentlyAnimated = false
        }, 3000);
        return;
    }
    native.taskPlayAnim(player, "random@arrests", "idle_2_hands_up", 8.0, 1.0, -1, 2, 0, false, false, false);
    alt.setTimeout(() => {
        native.taskPlayAnim(player, "random@arrests", "kneeling_arrest_idle", 8.0, 1.0, -1, 2, 0, false, false, false);
        alt.setTimeout(() => {
            native.taskPlayAnim(player, "rrandom@arrests@busted", "enter", 8.0, 1.0, -1, 2, 0, false, false, false);
            alt.setTimeout(() => {
                native.taskPlayAnim(player, "rrandom@arrests@busted", "enter", 8.0, 1.0, -1, 9, 0, false, false, false);
                alt.setTimeout(() => {
                    surrendered = true;
                    playerCurrentlyAnimated = true;
                    LastAD = ad2;
                    native.removeAnimDict("random@arrests" );
                    native.removeAnimDict("random@arrests@busted");
                }, 100);
            }, 1000);
        }, 500);

    }, 4000);

}