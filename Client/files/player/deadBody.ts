import * as alt from 'alt-client';
import * as native from 'natives';

var deadBodies = new Array();
var deadPeds = new Array();

alt.onServer('SendDeadBody', onReceiveDeadBody)

function onReceiveDeadBody(bodyJson:string){

    var deadBody = JSON.parse(bodyJson);

    var customCharacter = JSON.parse(deadBody.CustomCharacter);

    var ped:number;

    if(customCharacter.Gender == 0){
        ped = native.createPed(0, 1885233650, deadBody.PosX, deadBody.PosY, deadBody.PosZ, 0, false, true);
    }
    else{
        ped = native.createPed(0, -1667301416, deadBody.PosX, deadBody.PosY, deadBody.PosZ, 0, false, true);
    }

    //native.freezeEntityPosition(ped, true);


    native.setPedToRagdoll(ped, 10000, 10000, 0, true, true, false);
    native.setPedConfigFlag(ped, 71, true);
    

    var parentData = JSON.parse(customCharacter.Parents);

    var features = JSON.parse(customCharacter.Features);

    var apperanceInfo = JSON.parse(customCharacter.Appearance);

    var hairInfo = JSON.parse(customCharacter.Hair);

    native.setPedHeadBlendData(ped, parentData.Mother, parentData.Father, 0, parentData.Mother, parentData.Father, 0, parentData.Similarity, parentData.SkinSimilarity, 0, false);


    for (let index = 0; index < apperanceInfo.length; index++) {
        const element = apperanceInfo[index];

        if(element.Value == -1){
            element.Value = 255;
        }

        if (element.Value > 0) {
            native.setPedHeadOverlay(ped, index, element.Value, element.Opacity);
            if (index == 1) {
                native.setPedHeadOverlayColor(ped, index, 1, customCharacter.BeardColor, customCharacter.BeardColor);
            }
            else if (index == 2) {
                native.setPedHeadOverlayColor(ped, index, 1, customCharacter.EyebrowColor, customCharacter.EyebrowColor);
            }
            else if (index == 5) {
                native.setPedHeadOverlayColor(ped, index, 2, customCharacter.BlushColor, customCharacter.BlushColor);
            }
            else if (index == 8) {
                native.setPedHeadOverlayColor(ped, index, 2, customCharacter.LipstickColor, customCharacter.LipstickColor);
            }
            else if (index == 10) {
                native.setPedHeadOverlayColor(ped, index, 1, customCharacter.ChestHairColor, customCharacter.ChestHairColor);
            }
        }
    }

    for (let index = 0; index < features.length; index++) {
        const element = features[index];
        native.setPedFaceFeature(ped, index, element);
    }

    native.setPedComponentVariation(ped, 2, hairInfo.Hair, 0, 0);
    native.setPedEyeColor(ped, customCharacter.EyeColor);
    native.setPedHairColor(ped, hairInfo.Color, hairInfo.HighlightColor);

    var characterClothes = JSON.parse(deadBody.Clothes);
    var accessories = JSON.parse(deadBody.Accessories);
    
    characterClothes.forEach((element: { slot: number; drawable: number; texture: number; })  => {
        native.setPedComponentVariation(ped, element.slot, element.drawable, element.texture, 0);
    });

    native.setPedComponentVariation(ped, 3, deadBody.Torso, 0, 0);

    accessories.forEach((element: { slot: number; drawable: number; texture: number; }) => {
        native.setPedPropIndex(ped, element.slot, element.drawable, element.texture, true);
    });

    //var tattoos = JSON.parse(deadBody.Tattoos);

    deadBodies.push(deadBody);
    deadPeds.push(ped);
}

alt.onServer('RemoveDeadBody', OnRemoveDeadBody);

function OnRemoveDeadBody(bodyJson:string){

    var deadBody = JSON.parse(bodyJson);

    var body:any;

    var index:number;

    deadBodies.forEach((dead, deadIndex) => {
        if(dead.PosX == deadBody.PosX && dead.PosY == deadBody.PosY && dead.PosZ == deadBody.PosZ){
            body = dead;
            index = deadIndex;
        }
    });

    if(body == undefined) return;

    var ped:number = deadPeds[index];

    native.deletePed(ped);

    deadPeds.splice(index, 1);
    deadBodies.splice(index, 1);
}

alt.setInterval(() => {

    if(deadPeds.length == 0 ) return;
    deadPeds.forEach((ped:number) => {
        
        native.setPedToRagdoll(ped, 10000, 10000, 0, true, true, false);
        native.setPedConfigFlag(ped, 71, true);
    });

}, 1);