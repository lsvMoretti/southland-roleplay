import * as alt from 'alt-client';
import * as native from 'natives';

export function Distance(positionOne: alt.Vector3, positionTwo: alt.Vector3) {
    return Math.sqrt(Math.pow(positionOne.x - positionTwo.x, 2) + Math.pow(positionOne.y - positionTwo.y, 2) + Math.pow(positionOne.z - positionTwo.z, 2));
}

export async function RequestModel(modelHash, timeoutMs = 1000, recall = false) {
    if (native.isModelValid(modelHash))
      throw new Error("Model does not exists: ${modelHash}")
    else if (timeoutMs <= 0)
        throw new Error("Failed to load model: ${modelHash}")
  
    if (native.hasModelLoaded(modelHash))
      return Promise.resolve()
  
    if (!recall) {
        native.requestModel(modelHash)
    }
  
    return new Promise((res, rej) => {
        alt.setTimeout(() => RequestModel(modelHash, timeoutMs - 10, true).then(res, rej), 10)
    })
  }

  export async function RequestAnimDict(animDict:string, timeoutMs = 1000, recall = false){
    if(timeoutMs <= 0){
        throw new Error("Failed to load anim Dict: " + animDict);
    }

      if(native.hasAnimDictLoaded(animDict)) return Promise.resolve();

      if(!recall){
          native.requestAnimDict(animDict);
      }

      return new Promise((res, rej) => {
          alt.setTimeout(() => RequestAnimDict(animDict, timeoutMs - 10, true).then(res, rej), 10);
      })
  }