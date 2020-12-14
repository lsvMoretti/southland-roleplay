import * as alt from 'alt-client';
import * as native from 'natives';
const maxCountLoadTry = 255;
alt.onServer('animation:StartAnimation', startAnimation);
alt.onServer('animation:StopAnimation', stopAnimation);
export function stopAnimation() {
    var scriptId = alt.Player.local.scriptID;
    native.clearPedTasks(scriptId);
}
export function startAnimation(dict, name, duration, flag, ped = undefined) {
    let scriptId = alt.Player.local.scriptID;
    if (ped !== undefined) {
        scriptId = ped.scriptID;
    }
    native.clearPedTasks(scriptId);
    if (native.hasAnimDictLoaded(dict)) {
        native.taskPlayAnim(scriptId, dict, name, 1, -1, duration, flag, 1.0, false, false, false);
        return;
    }
    let result = loadAnimation(dict);
    result.then(() => {
        native.taskPlayAnim(scriptId, dict, name, 1, -1, duration, flag, 1.0, false, false, false);
    });
}
function loadAnimation(dict) {
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
function playScenario(scenarioName) {
    native.taskStartScenarioInPlace(alt.Player.local.scriptID, scenarioName, 0, true);
}
function stopScenario() {
    native.clearPedTasks(alt.Player.local.scriptID);
}
