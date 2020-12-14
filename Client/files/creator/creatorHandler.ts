import * as alt from 'alt-client';
import * as native from 'natives';
import * as animation from "files/animation";

var creatorView: alt.WebView = undefined;
var creatorCamera: number = undefined;

// Custom Character

var defaultCustomCharacter: any = undefined;
var customCharacter: any = undefined;
var parentInfo: any = undefined;
var features: any = undefined;
var appearanceItems: any = undefined;
var hairInfo: any = undefined;

var localPlayer = alt.Player.local.scriptID;

var orginalRotation = 270;

var featureNames = ["Nose Width", "Nose Bottom Height", "Nose Tip Length", "Nose Bridge Depth", "Nose Tip Height",
    "Nose Broken", "Brow Height", "Brow Depth", "Cheekbone Height", "Cheekbone Width", "Cheek Depth",
    "Eye Size", "Lip Thickness", "Jaw Width", "Jaw Shape", "Chin Height", "Chin Depth", "Chin Width",
    "Chin Indent", "Neck Width"];

alt.onServer('loadCharacterCreator', loadCharacterCreator);
alt.onServer('creatorSetGender', setCreatorGender);

alt.everyTick(() => {
    if (creatorCamera !== undefined) {
        native.disableAllControlActions(0);
        native.disableAllControlActions(1);
    }
});

alt.on('connectionComplete', (hasMapChanged: any) => {
    alt.loadModel(native.getHashKey('mp_m_freemode_01'));
    alt.loadModel(native.getHashKey('mp_f_freemode_01'));
    native.requestAnimDict("mp_creator_headik");
});

var currentGender = 0;

function setCreatorGender(gender: any) {
    if (creatorView === undefined) return;
    alt.log('New Gender: ' + gender);
    currentGender = gender;
    creatorView.emit('currentGender', gender);
    ApplyCreatorOutfit();
}

function loadCharacterCreator(customCharacterJson: any, defaultCustomCharacterJson: any) {
    localPlayer = alt.Player.local.scriptID;

    defaultCustomCharacter = JSON.parse(defaultCustomCharacterJson);

    customCharacter = JSON.parse(customCharacterJson);

    alt.log(customCharacter);

    if (creatorView !== undefined) {
        closeCharacterCreator();
    }

    creatorView = new alt.WebView('http://resource/files/creator/creatorHome.html', false);
    creatorView.focus();

    creatorView.on('creatorLoaded', creatorLoaded);

    creatorView.on('creator:genderChange', genderChange);

    creatorView.on('creator:rotationchange', rotationChange);

    creatorView.on('ParentChange', onParentChange);

    creatorView.on('creator:parentMixChange', onParentMixChange);

    creatorView.on('creator:skinMixChange', onSkinMixChange);

    creatorView.on('creator:facialFeatureUpdate', facialFeatureUpdate);

    creatorView.on('creator:setFacialApperance', setFacialApperance);

    creatorView.on('creator:onHairChange', onHairChange);

    creatorView.on('creator:onHairColorChange', onHairColorChange);

    creatorView.on('creator:onhairHighlightColorChange', onhairHighlightColorChange);

    creatorView.on('creator:onEyebrowColorChange', onEyebrowColorChange);

    creatorView.on('creator:onFacialHairColorChange', onFacialHairColorChange);

    creatorView.on('creator:onEyeColorChange', onEyeColorChange);

    creatorView.on('creator:onblushColorChange', onblushColorChange);

    creatorView.on('creator:onlipstickColorChange', onlipstickColorChange);

    creatorView.on('creator:onchestHairColorChange', onchestHairColorChange);

    creatorView.on('creator:finishCreation', finishCreation);

    creatorView.on('creator:zoomchange', zoomChange);

    creatorView.on('ParentSkinChange', onParentSkinChange);

    creatorCamera = native.createCamWithParams('DEFAULT_SCRIPTED_CAMERA', -839.6431, -719.5154, -156.3, 0, 0, 87.2606, 20, true, 2);

    native.displayRadar(false);

    native.pointCamAtPedBone(creatorCamera, localPlayer, 31086, 0, 0, 0, false);

    native.setCamActive(creatorCamera, true);

    native.renderScriptCams(true, false, 0, true, false, null);

    native.freezeEntityPosition(localPlayer, true);

    animation.startAnimation("mp_creator_headik", "mp_head_ik_override", -1, 1);

    native.setEntityHeading(localPlayer, orginalRotation);

    native.setPedCanPlayAmbientAnims(localPlayer, false);
    native.setPedCanPlayAmbientBaseAnims(localPlayer, false);
    native.setPedCanPlayInjuredAnims(localPlayer, false);

    parentInfo = JSON.parse(customCharacter.Parents);

    features = JSON.parse(customCharacter.Features);

    appearanceItems = JSON.parse(customCharacter.Appearance);

    hairInfo = JSON.parse(customCharacter.Hair);

    if (features.length < featureNames.length) {
        for (let index = 0; index < featureNames.length; index++) {
            features[index] = 0.0;
        }
    }

    if (appearanceItems.length === 0) {
        for (let index = 0; index <= 11; index++) {
            var newApperance
            {
                const Value = -1;
                const Opacity = -1;
            }

            appearanceItems.push(newApperance);
        }
    }
    alt.showCursor(true);

    currentGender = customCharacter.Gender;

    ApplyCreatorOutfit();
}

//#region Parent

var parentOne = 0;
var parentTwo = 0;
var parentMix = 0.5;
var skinMix = 0.5;

function onParentChange(parent: any, faceId: any) {
    alt.log('Parent Change: ' + parent + ", " + faceId);

    if (parent === 1) {
        // Parent One
        parentOne = faceId;
        parentInfo.Mother = Number(faceId);
    }
    if (parent === 2) {
        // Parent Two
        parentTwo = faceId;
        parentInfo.Father = Number(faceId);
    }

    if (currentGender === 0) {
        // Male
        alt.log('male component variation');
        native.setPedComponentVariation(localPlayer, 3, 15, 0, 2);
        native.setPedComponentVariation(localPlayer, 4, 21, 0, 2);
        native.setPedComponentVariation(localPlayer, 6, 34, 0, 2);
        native.setPedComponentVariation(localPlayer, 8, 15, 0, 2);
        native.setPedComponentVariation(localPlayer, 11, 15, 0, 2);
    }
    else {
        native.setPedComponentVariation(localPlayer, 3, 15, 0, 2);
        native.setPedComponentVariation(localPlayer, 4, 10, 0, 2);
        native.setPedComponentVariation(localPlayer, 6, 35, 0, 2);
        native.setPedComponentVariation(localPlayer, 8, 15, 0, 2);
        native.setPedComponentVariation(localPlayer, 11, 15, 0, 2);
    }

    customCharacter = defaultCustomCharacter;
    PedHeadBlendUpdate();
}

var parentOneSkin = 0;
var parentTwoSkin = 0;

function onParentSkinChange(parent: any, newParentSkin: any) {
    if (parent == 1) {
        parentOneSkin = newParentSkin;
    }
    if (parent == 2) {
        parentTwoSkin = newParentSkin;
    }

    PedHeadBlendUpdate();
}

function onParentMixChange(newParentMix: any) {
    parentMix = newParentMix;
    parentInfo.Similarity = newParentMix;
    PedHeadBlendUpdate();
}

function onSkinMixChange(newSkinMix: any) {
    skinMix = newSkinMix;
    parentInfo.SkinSimilarity = newSkinMix;
    PedHeadBlendUpdate();
}

function PedHeadBlendUpdate() {
    alt.log('Native Called');
    alt.log('Parent One: ' + parentOne);
    alt.log('Parent Two: ' + parentTwo);
    alt.log('Similarity: ' + parentMix);
    alt.log('Skin Mix: ' + skinMix);

    localPlayer = alt.Player.local.scriptID;
    native.setPedHeadBlendData(localPlayer, parentOne, parentTwo, 0, parentOneSkin, parentTwoSkin, 0, parentMix, skinMix, 0, false);
}

//#endregion

//#region Facial Features

function facialFeatureUpdate(slot: any, value: any) {
    localPlayer = alt.Player.local.scriptID;
    native.setPedFaceFeature(localPlayer, slot, value);
    features[slot] = value;
}

//#endregion

//#region Apperances

function setFacialApperance(slot: any, appearance: any, opacity: any) {
    localPlayer = alt.Player.local.scriptID;
    var convertedOpacity = Number(opacity) / 100;

    var convertedAppearance = appearance - 1;

    if (convertedAppearance == -1) {
        convertedAppearance = 255;
    }

    appearanceItems[slot].Value = convertedAppearance;
    appearanceItems[slot].Opacity = convertedOpacity;

    native.setPedHeadOverlay(localPlayer, slot, convertedAppearance, convertedOpacity);
}

//#endregion

//#region Hair

//#region Head Hair

function onHairChange(newHair: any) {
    localPlayer = alt.Player.local.scriptID;

    native.clearPedDecorationsLeaveScars(localPlayer);

    hairInfo.Hair = newHair;

    native.setPedComponentVariation(localPlayer, 2, hairInfo.Hair, 0, 2);

    var hairTattoo = undefined;

    if (customCharacter.Gender === 0) {
        hairTattoo = FetchMaleHairTattooData(newHair);
    }

    if (customCharacter.Gender === 1) {
        hairTattoo = FetchFemaleHairTattooData(newHair);
    }

    if (hairTattoo !== undefined) {
        var hairTattooCollection = native.getHashKey(hairTattoo[0]);
        var hairTattooOverlay = native.getHashKey(hairTattoo[1]);

        alt.log(hairTattooCollection);
        alt.log(hairTattooOverlay);

        //native.setPedFacialDecoration(localPlayer, hairTattooCollection, hairTattooOverlay);
        native.addPedDecorationFromHashes(localPlayer, hairTattooCollection, hairTattooOverlay);
    }
}

var currentHairColor = 0;
var currentHairHighlightColor = 0;

function onHairColorChange(newHairColor: any) {
    currentHairColor = newHairColor;
    hairInfo.Color = newHairColor;

    alt.log('currentHairColor: ' + currentHairColor);
    UpdateHairColor();
}

function onhairHighlightColorChange(newHairHighlightColor: any) {
    currentHairHighlightColor = newHairHighlightColor;
    hairInfo.HighlightColor = newHairHighlightColor;

    UpdateHairColor();
}

function UpdateHairColor() {
    native.setPedHairColor(alt.Player.local.scriptID, hairInfo.Color, hairInfo.HighlightColor);
    alt.log('currentHairColor' + currentHairColor);
    alt.log('currentHairHighlightColor ' + currentHairHighlightColor);
    alt.log('updatedHairColor');
}

//#region Male Hair Tattoo's

function FetchMaleHairTattooData(hairId: any) {
    if (hairId === 0) {
        return ["mpbeach_overlays", "FM_Hair_Fuzz"];
    }
    if (hairId === 1) {
        return ["multiplayer_overlays", "NG_M_Hair_001"];
    }
    if (hairId === 2) {
        return ["multiplayer_overlays", "NG_M_Hair_002"];
    }
    if (hairId === 3) {
        return ["multiplayer_overlays", "NG_M_Hair_003"];
    }
    if (hairId === 4) {
        return ["multiplayer_overlays", "NG_M_Hair_004"];
    }
    if (hairId === 5) {
        return ["multiplayer_overlays", "NG_M_Hair_005"];
    }
    if (hairId === 6) {
        return ["multiplayer_overlays", "NG_M_Hair_006"];
    }
    if (hairId === 7) {
        return ["multiplayer_overlays", "NG_M_Hair_007"];
    }
    if (hairId === 8) {
        return ["multiplayer_overlays", "NG_M_Hair_008"];
    }
    if (hairId === 9) {
        return ["multiplayer_overlays", "NG_M_Hair_009"];
    }
    if (hairId === 10) {
        return ["multiplayer_overlays", "NG_M_Hair_013"];
    }
    if (hairId === 11) {
        return ["multiplayer_overlays", "NG_M_Hair_002"];
    }
    if (hairId === 12) {
        return ["multiplayer_overlays", "NG_M_Hair_011"];
    }
    if (hairId === 13) {
        return ["multiplayer_overlays", "NG_M_Hair_012"];
    }
    if (hairId === 14) {
        return ["multiplayer_overlays", "NG_M_Hair_014"];
    }
    if (hairId === 15) {
        return ["multiplayer_overlays", "NG_M_Hair_015"];
    }
    if (hairId === 16) {
        return ["multiplayer_overlays", "NGBea_M_Hair_000"];
    }
    if (hairId === 17) {
        return ["multiplayer_overlays", "NGBea_M_Hair_001"];
    }
    if (hairId === 18) {
        return ["multiplayer_overlays", "NGBus_M_Hair_000"];
    }
    if (hairId === 19) {
        return ["multiplayer_overlays", "NGBus_M_Hair_002"];
    }
    if (hairId === 20) {
        return ["multiplayer_overlays", "NGHip_M_Hair_000"];
    }
    if (hairId === 21) {
        return ["multiplayer_overlays", "NGHip_M_Hair_001"];
    }
    if (hairId === 22) {
        return ["multiplayer_overlays", "NGInd_M_Hair_000"];
    }
    if (hairId === 24) {
        return ["mplowrider_overlays", "LR_M_Hair_000"];
    }
    if (hairId === 25) {
        return ["mplowrider_overlays", "LR_M_Hair_001"];
    }
    if (hairId === 26) {
        return ["mplowrider_overlays", "LR_M_Hair_002"];
    }
    if (hairId === 27) {
        return ["mplowrider_overlays", "LR_M_Hair_003"];
    }
    if (hairId === 28) {
        return ["mplowrider2_overlays", "LR_M_Hair_004"];
    }
    if (hairId === 29) {
        return ["mplowrider2_overlays", "LR_M_Hair_005"];
    }
    if (hairId === 30) {
        return ["mplowrider2_overlays", "LR_M_Hair_006"];
    }
    if (hairId === 31) {
        return ["mpbiker_overlays", "MP_Biker_Hair_000_M"];
    }
    if (hairId === 32) {
        return ["mpbiker_overlays", "MP_Biker_Hair_001_M"];
    }
    if (hairId === 33) {
        return ["mpbiker_overlays", "MP_Biker_Hair_002_M"];
    }
    if (hairId === 34) {
        return ["mpbiker_overlays", "MP_Biker_Hair_003_M"];
    }
    if (hairId === 35) {
        return ["mpbiker_overlays", "MP_Biker_Hair_004_M"];
    }
    if (hairId === 36) {
        return ["mpbiker_overlays", "MP_Biker_Hair_005_M"];
    }
    if (hairId === 72) {
        return ["mpgunrunning_overlays", "MP_Gunrunning_Hair_M_000_M"];
    }
    if (hairId === 73) {
        return ["mpgunrunning_overlays", "MP_Gunrunning_Hair_M_001_M"];
    }
}

//#endregion

//#region Female Hair Tattoo's

function FetchFemaleHairTattooData(hairId: any) {
    if (hairId === 0) {
        return ["mpbeach_overlays", "FM_Hair_Fuzz"];
    }
    if (hairId === 1) {
        return ["multiplayer_overlays", "NG_F_Hair_001"];
    }
    if (hairId === 2) {
        return ["multiplayer_overlays", "NG_F_Hair_002"];
    }
    if (hairId === 3) {
        return ["multiplayer_overlays", "NG_F_Hair_003"];
    }
    if (hairId === 4) {
        return ["multiplayer_overlays", "NG_F_Hair_004"];
    }
    if (hairId === 5) {
        return ["multiplayer_overlays", "NG_F_Hair_005"];
    }
    if (hairId === 6) {
        return ["multiplayer_overlays", "NG_F_Hair_006"];
    }
    if (hairId === 7) {
        return ["multiplayer_overlays", "NG_F_Hair_007"];
    }
    if (hairId === 8) {
        return ["multiplayer_overlays", "NG_F_Hair_008"];
    }
    if (hairId === 9) {
        return ["multiplayer_overlays", "NG_F_Hair_009"];
    }
    if (hairId === 10) {
        return ["multiplayer_overlays", "NG_F_Hair_010"];
    }
    if (hairId === 11) {
        return ["multiplayer_overlays", "NG_F_Hair_011"];
    }
    if (hairId === 12) {
        return ["multiplayer_overlays", "NG_F_Hair_012"];
    }
    if (hairId === 13) {
        return ["multiplayer_overlays", "NG_F_Hair_013"];
    }
    if (hairId === 14) {
        return ["multiplayer_overlays", "NG_F_Hair_014"];
    }
    if (hairId === 15) {
        return ["multiplayer_overlays", "NG_F_Hair_015"];
    }
    if (hairId === 16) {
        return ["multiplayer_overlays", "NGBea_F_Hair_000"];
    }
    if (hairId === 17) {
        return ["multiplayer_overlays", "NGBea_F_Hair_001"];
    }
    if (hairId === 18) {
        return ["multiplayer_overlays", "NG_F_Hair_007"];
    }
    if (hairId === 19) {
        return ["multiplayer_overlays", "NGBus_F_Hair_000"];
    }
    if (hairId === 20) {
        return ["multiplayer_overlays", "NGBus_F_Hair_001"];
    }
    if (hairId === 21) {
        return ["multiplayer_overlays", "NGBea_F_Hair_001"];
    }
    if (hairId === 22) {
        return ["multiplayer_overlays", "NGHip_F_Hair_000"];
    }
    if (hairId === 23) {
        return ["multiplayer_overlays", "NGHip_F_Hair_000"];
    }
    if (hairId === 25) {
        return ["mplowrider_overlays", "LR_F_Hair_000"];
    }
    if (hairId === 26) {
        return ["mplowrider_overlays", "LR_F_Hair_001"];
    }
    if (hairId === 27) {
        return ["mplowrider_overlays", "LR_F_Hair_002"];
    }
    if (hairId === 28) {
        return ["mplowrider2_overlays", "LR_F_Hair_003"];
    }
    if (hairId === 29) {
        return ["mplowrider2_overlays", "LR_F_Hair_003"];
    }
    if (hairId === 30) {
        return ["mplowrider2_overlays", "LR_F_Hair_004"];
    }
    if (hairId === 31) {
        return ["mplowrider2_overlays", "LR_F_Hair_006"];
    }
    if (hairId === 32) {
        return ["mpbiker_overlays", "MP_Biker_Hair_000_F"];
    }
    if (hairId === 33) {
        return ["mpbiker_overlays", "MP_Biker_Hair_001_F"];
    }
    if (hairId === 34) {
        return ["mpbiker_overlays", "MP_Biker_Hair_002_F"];
    }
    if (hairId === 35) {
        return ["mpbiker_overlays", "MP_Biker_Hair_003_F"];
    }
    if (hairId === 36) {
        return ["multiplayer_overlays", "MP_Biker_Hair_005_M"];
    }
    if (hairId === 36) {
        return ["mpgunrunning_overlays", "NG_F_Hair_003"];
    }
    if (hairId === 37) {
        return ["mpbiker_overlays", "MP_Biker_Hair_006_F"];
    }
    if (hairId === 38) {
        return ["mpbiker_overlays", "MP_Biker_Hair_004_F"];
    }
    if (hairId === 73) {
        return ["mpgunrunning_overlays", "MP_Gunrunning_Hair_F_000_F"];
    }
    if (hairId === 73) {
        return ["mpgunrunning_overlays", "MP_Gunrunning_Hair_F_001_F"];
    }
}

//#endregion

//#endregion

//#region Eyebrows

function onEyebrowColorChange(newColor: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.EyebrowColor = newColor;

    native.setPedHeadOverlayColor(localPlayer, 2, 1, newColor, 0);
}

//#endregion

//#region Facial Hair

function onFacialHairColorChange(newColor: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.BeardColor = newColor;

    native.setPedHeadOverlayColor(localPlayer, 1, 1, newColor, 0);
}

//#endregion

//#endregion

//#region Eye Color

function onEyeColorChange(newColor: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.EyeColor = newColor;

    native.setPedEyeColor(localPlayer, newColor);
}

function onblushColorChange(newColor: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.BlushColor = newColor;

    native.setPedHeadOverlayColor(localPlayer, 5, 2, newColor, 0);
}

function onlipstickColorChange(newColor: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.LipstickColor = newColor;

    native.setPedHeadOverlayColor(localPlayer, 8, 2, newColor, 0);
}

function onchestHairColorChange(newColor: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.ChestHairColor = newColor;

    native.setPedHeadOverlayColor(localPlayer, 10, 1, newColor, 0);
}

//#endregion

//#region Zoom

function zoomChange(newZoom: any) {
    if (creatorCamera === undefined) return;

    native.setCamFov(creatorCamera, newZoom);
}

//#endregion

//#region Rotation

function rotationChange(newRotation: any) {
    localPlayer = alt.Player.local.scriptID;

    var stringRotation = newRotation.toString();

    if (stringRotation.startsWith("-")) {
        var rotation = stringRotation.slice(1);

        var numberRotation = Number(rotation);

        var minus = orginalRotation - numberRotation;
        native.setEntityHeading(localPlayer, Number(minus));
        alt.log('Minus: ' + minus);
        return;
    }
    else {
        if (Number(newRotation) > 90) {
            var calculated = 0 + (Number(newRotation) - 90);
            native.setEntityHeading(localPlayer, Number(calculated));
            alt.log('Calculated: ' + calculated);
            return;
        }
        var plus = orginalRotation + Number(newRotation);
        native.setEntityHeading(localPlayer, Number(plus));
        alt.log('Plus: ' + plus);
    }
}

//#endregion

//#region Gender Change

function genderChange(newGender: any) {
    localPlayer = alt.Player.local.scriptID;

    customCharacter.Gender = newGender;
    currentGender = newGender;

    setCreatorGender(newGender);
    if (newGender === 0) {
        // Male
        alt.emitServer('creator:onGenderChange', newGender);
        ApplyCreatorOutfit();
    }
    else {
        // Female
        alt.emitServer('creator:onGenderChange', newGender);
        ApplyCreatorOutfit();
    }
}

//#endregion

function creatorLoaded() {
    if (creatorView !== undefined) {
        creatorView.emit('currentGender', customCharacter.Gender);
        genderChange(customCharacter.Gender);
        alt.log('Current Gender' + customCharacter.Gender);
    }
}

function ApplyCreatorOutfit() {
    localPlayer = alt.Player.local.scriptID;

    native.clearPedDecorations(localPlayer);
    native.clearAllPedProps(localPlayer);
    native.clearPedDecorationsLeaveScars(localPlayer);
    PedHeadBlendUpdate();

    animation.startAnimation("mp_creator_headik", "mp_head_ik_override", -1, 1);

    alt.setTimeout(() => {
        if (currentGender === 0) {
            // Male
            alt.log('male component variation');
            native.setPedComponentVariation(localPlayer, 3, 15, 0, 2);
            native.setPedComponentVariation(localPlayer, 4, 21, 0, 2);
            native.setPedComponentVariation(localPlayer, 6, 34, 0, 2);
            native.setPedComponentVariation(localPlayer, 8, 15, 0, 2);
            native.setPedComponentVariation(localPlayer, 11, 15, 0, 2);
        } else {
            native.setPedComponentVariation(localPlayer, 3, 15, 0, 2);
            native.setPedComponentVariation(localPlayer, 4, 10, 0, 2);
            native.setPedComponentVariation(localPlayer, 6, 35, 0, 2);
            native.setPedComponentVariation(localPlayer, 8, 15, 0, 2);
            native.setPedComponentVariation(localPlayer, 11, 15, 0, 2);
        }
    },
        600);
}

function closeCharacterCreator() {
    if (creatorView !== undefined) {
        creatorView.destroy();
        creatorView = undefined;
    }
}

function finishCreation() {
    closeCharacterCreator();

    if (creatorCamera !== undefined) {
        native.renderScriptCams(false, false, 0, true, false, null);
        native.destroyCam(creatorCamera, true);
        native.destroyAllCams(true);
        creatorCamera = undefined;
        native.setFollowPedCamViewMode(1);
        native.clearFocus();
    }

    alt.showCursor(false);

    native.displayRadar(true);

    native.freezeEntityPosition(localPlayer, false);

    animation.stopAnimation();

    customCharacter.Parents = JSON.stringify(parentInfo);
    customCharacter.Features = JSON.stringify(features);
    customCharacter.Appearance = JSON.stringify(appearanceItems);
    customCharacter.Hair = JSON.stringify(hairInfo);
    customCharacter.Gender = currentGender;

    alt.log('Character Feature: ' + customCharacter.features);

    var customJSON = JSON.stringify(customCharacter);

    alt.emitServer('creator:CharacterCreatorFinished', customJSON);
}