import * as alt from 'alt-client';
import * as native from 'natives';
alt.onServer('setPlayerClothes', setPlayerClothes);
alt.onServer('setPlayerAccessory', setPlayerAccessory);
alt.onServer('loadCustomPlayer', loadCustomPlayer);
alt.onServer('previewFacialHair', (index) => {
    native.setPedHeadOverlay(alt.Player.local.scriptID, 1, index, 1);
});
function setPlayerClothes(slot, drawable, texture) {
    native.setPedComponentVariation(alt.Player.local.scriptID, slot, drawable, texture, 0);
}
function setPlayerAccessory(slot, drawable, texture) {
    if (drawable === -1) {
        native.clearPedProp(alt.Player.local.scriptID, slot);
        return;
    }
    native.setPedPropIndex(alt.Player.local.scriptID, slot, drawable, texture, true);
}
function loadCustomPlayer(customCharacterJson, clothesJson, accessoryJson) {
    setCustomCharacter(alt.Player.local.scriptID, customCharacterJson, clothesJson, accessoryJson);
}
var pedArray = new Array();
alt.onServer('LoadNPCPreview', LoadNPCPreview);
alt.onServer('destroyNPCPreviews', destroyNPCPreviews);
alt.onServer('loadTattooData', (collection, overlay) => {
    var localPlayer = alt.Player.local.scriptID;
    alt.log('Loading Tattoo Data: ' + collection + ',' + overlay);
    var hairTattooCollection = native.getHashKey(collection);
    var hairTattooOverlay = native.getHashKey(overlay);
    alt.log('After Hash: ' + hairTattooCollection + ',' + hairTattooOverlay);
    native.addPedDecorationFromHashes(localPlayer, hairTattooCollection, hairTattooOverlay);
});
alt.onServer('ClearTattoos', () => {
    native.clearPedDecorations(alt.Player.local.scriptID);
});
function destroyNPCPreviews() {
    if (pedArray.length > 0) {
        pedArray.forEach(element => {
            native.deletePed(element);
        });
    }
}
function LoadNPCPreview(pos, customCharacterJson, clothesJson, accessoryJson, torso) {
    var customCharacter = JSON.parse(customCharacterJson);
    var ped = null;
    if (pos === 0) {
        alt.log('Loading in Char 0');
        if (customCharacter.Gender == 0) {
            native.requestModel(1885233650);
            ped = native.createPed(0, 1885233650, -241.72748, -1194.0396, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        else if (customCharacter.Gender == 1) {
            native.requestModel(-1667301416);
            ped = native.createPed(0, -1667301416, -241.72748, -1194.0396, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
    }
    if (pos === 1) {
        alt.log('Loading in Char 1');
        if (customCharacter.Gender == 0) {
            native.requestModel(1885233650);
            ped = native.createPed(0, 1885233650, -238.77362, -1194.2373, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        else if (customCharacter.Gender == 1) {
            native.requestModel(-1667301416);
            ped = native.createPed(0, -1667301416, -238.77362, -1194.2373, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
    }
    if (pos === 2) {
        alt.log('Loading in Char 2');
        if (customCharacter.Gender == 0) {
            native.requestModel(1885233650);
            ped = native.createPed(0, 1885233650, -235.26593, -1194.2902, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        else if (customCharacter.Gender == 1) {
            native.requestModel(-1667301416);
            ped = native.createPed(0, -1667301416, -235.26593, -1194.2902, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
    }
    if (pos === 3) {
        alt.log('Loading in Char 3');
        if (customCharacter.Gender == 0) {
            native.requestModel(1885233650);
            ped = native.createPed(0, 1885233650, -229.13406, -1194.3693, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        else if (customCharacter.Gender == 1) {
            native.requestModel(-1667301416);
            ped = native.createPed(0, -1667301416, -229.13406, -1194.3693, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
    }
    if (pos === 4) {
        alt.log('Loading in Char 4');
        if (customCharacter.Gender == 0) {
            native.requestModel(1885233650);
            ped = native.createPed(0, 1885233650, -226.16704, -1194.567, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        else if (customCharacter.Gender == 1) {
            native.requestModel(-1667301416);
            ped = native.createPed(0, -1667301416, -226.16704, -1194.567, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        alt.log('Char4: ' + ped);
    }
    if (pos === 5) {
        alt.log('Loading in Char 5');
        if (customCharacter.Gender == 0) {
            native.requestModel(1885233650);
            ped = native.createPed(0, 1885233650, -223.02856, -1194.8044, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
        else if (customCharacter.Gender == 1) {
            native.requestModel(-1667301416);
            ped = native.createPed(0, -1667301416, -223.02856, -1194.8044, -149.92383, 0, false, true);
            pedArray.push(ped);
        }
    }
    else if (ped === null) {
        return;
    }
    native.freezeEntityPosition(ped, true);
    native.setEntityInvincible(ped, true);
    native.setPedComponentVariation(ped, 3, torso, 0, 0);
    setCustomCharacter(ped, customCharacterJson, clothesJson, accessoryJson);
}
function setCustomCharacter(ped, customCharacterJson, clothesJson, accessoryJson) {
    var customCharacter = JSON.parse(customCharacterJson);
    if (customCharacter === null || customCharacter === undefined)
        return;
    var parentData = JSON.parse(customCharacter.Parents);
    var features = JSON.parse(customCharacter.Features);
    var apperanceInfo = JSON.parse(customCharacter.Appearance);
    var hairInfo = JSON.parse(customCharacter.Hair);
    native.setPedHeadBlendData(ped, parentData.Mother, parentData.Father, 0, parentData.Mother, parentData.Father, 0, parentData.Similarity, parentData.SkinSimilarity, 0, false);
    for (let index = 0; index < apperanceInfo.length; index++) {
        const element = apperanceInfo[index];
        if (element.Value == -1) {
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
    var characterClothes = JSON.parse(clothesJson);
    var accessories = JSON.parse(accessoryJson);
    characterClothes.forEach((element) => {
        native.setPedComponentVariation(ped, element.slot, element.drawable, element.texture, 0);
    });
    accessories.forEach((element) => {
        native.setPedPropIndex(ped, element.slot, element.drawable, element.texture, true);
    });
}
alt.on('disconnect', () => {
    if (pedArray.length > 0) {
        pedArray.forEach(element => {
            native.deletePed(element);
        });
    }
});
