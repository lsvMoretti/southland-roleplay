if ('alt' in window) {
    alt.on('currentGender', setCurrentGender);
    alt.on('currentCharacter', setCurrentCharacter);
}

var gender = 0;

function setCurrentGender(newGender) {
    gender = newGender;
}

//#region Parents

var ParentNames = [
    "Benjamin", "Daniel", "Joshua", "Noah", "Andrew", "Joan", "Alex", "Isaac", "Evan", "Ethan", "Vincent",
    "Angel", "Diego", "Adrian", "Gabriel", "Michael", "Santiago", "Kevin", "Louis", "Samuel", "Anthony",
    "Claude", "Niko", "John", "Hannah", "Aubrey", "Jasmine", "Giselle", "Amelia", "Isabella", "Zoe", "Ava",
    "Camilla", "Violet", "Sophia", "Eveline", "Nicole", "Ashley", "Gracie", "Brianna", "Natalie", "Olivia",
    "Elizabeth", "Charlotte", "Emma", "Misty"
];

var parentIndex = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 44, 43, 42, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 45]

var ParentOne = ParentNames[0];
var ParentTwo = ParentNames[0];
var ParentOneNameIndex = 0;
var ParentTwoNameIndex = 0;
var ParentOneIndex = parentIndex[0];
var ParentTwoIndex = parentIndex[0];

var parentMix = 0.5;

var parentMixSlider = document.getElementById('parentMixSlider');
var parentMixValue = document.getElementById('parentMixValue');

parentMixSlider.value = parentMix;
parentMixValue.innerHTML = parentMixSlider.value;

var ParentOneSkin = ParentNames[0];
var ParentTwoSkin = ParentNames[0];
var ParentOneNameIndexSkin = 0;
var ParentTwoNameIndexSkin = 0;
var ParentOneIndexSkin = parentIndex[0];
var ParentTwoIndexSkin = parentIndex[0];

var skinMix = 0.5;

var skinMixSlider = document.getElementById('skinMixSlider');
var skinMixValue = document.getElementById('skinMixValue');

skinMixSlider.value = skinMix;
skinMixValue.innerHTML = skinMixSlider.value;

//#endregion

//#region Facial Features

var noseWidth = 0.0;

var noseWidthSlider = document.getElementById('noseWidthSlider');
var noseWidthValue = document.getElementById('noseWidthValue');

noseWidthSlider.value = noseWidth;
noseWidthValue.innerHTML = noseWidthSlider.value;

var noseBottom = 0.0;

var noseBottomSlider = document.getElementById('noseBottomSlider');
var noseBottomValue = document.getElementById('noseBottomValue');

noseBottomSlider.value = noseBottom;
noseBottomValue.innerHTML = noseBottomSlider.value;

var noseTipLength = 0.0;

var noseTipLengthSlider = document.getElementById('noseTipLengthSlider');
var noseTipLengthValue = document.getElementById('noseTipLengthValue');

noseTipLengthSlider.value = noseTipLength;
noseTipLengthValue.innerHTML = noseTipLengthSlider.value;


var noseBridgeDepth = 0.0;

var noseBridgeDepthSlider = document.getElementById('noseBridgeDepthSlider');
var noseBridgeDepthValue = document.getElementById('noseBridgeDepthValue');

noseBridgeDepthSlider.value = noseBridgeDepth;
noseBridgeDepthValue.innerHTML = noseBridgeDepthSlider.value;

var noseTipHeight = 0.0;

var noseTipHeightSlider = document.getElementById('noseTipHeightSlider');
var noseTipHeightValue = document.getElementById('noseTipHeightValue');

noseTipHeightSlider.value = noseTipHeight;
noseTipHeightValue.innerHTML = noseTipHeightSlider.value;

var noseBroken = 0.0;

var noseBrokenSlider = document.getElementById('noseBrokenSlider');
var noseBrokenValue = document.getElementById('noseBrokenValue');

noseBrokenSlider.value = noseBroken;
noseBrokenValue.innerHTML = noseBrokenSlider.value;

var browHeight = 0.0;

var browHeightSlider = document.getElementById('browHeightSlider');
var browHeightValue = document.getElementById('browHeightValue');

browHeightSlider.value = browHeight;
browHeightValue.innerHTML = browHeightSlider.value;

var cheekboneHeight = 0.0;

var checkBoneHeightSlider = document.getElementById('checkBoneHeightSlider');
var cheekBoneHeightValue = document.getElementById('cheekBoneHeightValue');

checkBoneHeightSlider.value = cheekboneHeight;
cheekBoneHeightValue.innerHTML = checkBoneHeightSlider.value;

var cheekboneWidth = 0.0;

var checkBoneWidthSlider = document.getElementById('checkBoneWidthSlider');
var cheekBoneWidthValue = document.getElementById('cheekBoneWidthValue');

checkBoneWidthSlider.value = cheekboneWidth;
cheekBoneWidthValue.innerHTML = checkBoneWidthSlider.value;

var cheekDepth = 0.0;

var cheekDepthSlider = document.getElementById('cheekDepthSlider');
var cheekDepthValue = document.getElementById('cheekDepthValue');

cheekDepthSlider.value = cheekDepth;
cheekDepthValue.innerHTML = cheekDepthSlider.value;


var eyeSize = 0.0;

var eyeSizeSlider = document.getElementById('eyeSizeSlider');
var eyeSizeValue = document.getElementById('eyeSizeValue');

eyeSizeSlider.value = eyeSize;
eyeSizeValue.innerHTML = eyeSizeSlider.value;

var lipThickness = 0.0;

var lipThicknessSlider = document.getElementById('lipThicknessSlider');
var lipThicknessValue = document.getElementById('lipThicknessValue');

lipThicknessSlider.value = lipThickness;
lipThicknessValue.innerHTML = lipThicknessSlider.value;

var jawWidth = 0.0;

var jawWidthSlider = document.getElementById('jawWidthSlider');
var jawWidthValue = document.getElementById('jawWidthValue');

jawWidthSlider.value = jawWidth;
jawWidthValue.innerHTML = jawWidthSlider.value;

var jawHeight = 0.0;

var jawHeightSlider = document.getElementById('jawHeightSlider');
var jawHeightValue = document.getElementById('jawHeightValue');

jawHeightSlider.value = jawHeight;
jawHeightValue.innerHTML = jawHeightSlider.value;

var chinHeight = 0.0;

var chinHeightSlider = document.getElementById('chinHeightSlider');
var chinHeightValue = document.getElementById('chinHeightValue');

chinHeightSlider.value = chinHeight;
chinHeightValue.innerHTML = chinHeightSlider.value;

var chinDepth = 0.0;

var chinDepthSlider = document.getElementById('chinDepthSlider');
var chinDepthValue = document.getElementById('chinDepthValue');

chinDepthSlider.value = chinDepth;
chinDepthValue.innerHTML = chinDepthSlider.value;

var chinWidth = 0.0;

var chinWidthSlider = document.getElementById('chinWidthSlider');
var chinWidthValue = document.getElementById('chinWidthValue');

chinWidthSlider.value = chinWidth;
chinWidthValue.innerHTML = chinWidthSlider.value;

var chinIndent = 0.0;

var chinIndentSlider = document.getElementById('chinIndentSlider');
var chinIndentValue = document.getElementById('chinIndentValue');

chinIndentSlider.value = chinIndent;
chinIndentValue.innerHTML = chinIndentSlider.value;

var neckWidth = 0.0;

var neckWidthSlider = document.getElementById('neckWidthSlider');
var neckWidthValue = document.getElementById('neckWidthValue');

neckWidthSlider.value = neckWidth;
neckWidthValue.innerHTML = neckWidthSlider.value;

//#endregion Facial Features

//#region Apperances

var blemishNames = ["None", "Measles", "Pimples", "Spots", "Break Out", "Blackheads", "Build Up", "Pustules", "Zits",
    "Full Acne", "Acne", "Cheek Rash", "Face Rash", "Picker", "Puberty", "Eyesore", "Chin Rash", "Two Face",
    "T Zone", "Greasy", "Marked", "Acne Scarring", "Full Acne Scarring", "Cold Sores", "Impetigo"]

var currentBlemish = 0;

var blemishText = document.getElementById('blemishName');

var blemishOpacity = 50;

var blemishOpactiyField = document.getElementById('blemishOpactiyValue');
var blemishOpactiySlider = document.getElementById('blemishOpacitySlider');



var facialHairNames = ["None", "Light Stubble", "Balbo", "Circle Beard", "Goatee", "Chin", "Chin Fuzz", "Pencil Chin Strap",
    "Scruffy", "Musketeer", "Mustache", "Trimmed Beard", "Stubble", "Thin Circle Beard", "Horseshoe",
    "Pencil and 'Chops", "Chin Strap Beard", "Balbo and Sideburns", "Mutton Chops", "Scruffy Beard", "Curly",
    "Curly & Deep Stranger", "Handlebar", "Faustic", "Otto & Patch", "Otto & Full Stranger", "Light Franz",
    "The Hampstead", "The Ambrose", "Lincoln Curtain"]

var currentFacialHair = 0;

var facialHairText = document.getElementById('facialHairName');

var facialHairOpacity = 50;

var facialHairOpactiyField = document.getElementById('facialHairOpacityValue');
var facialHairOpacitySlider = document.getElementById('facialHairOpacitySlider');

var eyebrowNames = ["None", "Balanced", "Fashion", "Cleopatra", "Quizzical", "Femme", "Seductive", "Pinched", "Chola",
    "Triomphe", "Carefree", "Curvaceous", "Rodent", "Double Tram", "Thin", "Penciled", "Mother Plucker",
    "Straight and Narrow", "Natural", "Fuzzy", "Unkempt", "Caterpillar", "Regular", "Mediterranean", "Groomed",
    "Bushels", "Feathered", "Prickly", "Monobrow", "Winged", "Triple Tram", "Arched Tram", "Cutouts",
    "Fade Away", "Solo Tram"]

var currentEyebrow = 0;

var eyebrowText = document.getElementById('eyebrowsName');

var eyebrowOpacity = 50;

var eyebrowOpactiyField = document.getElementById('eyebrowsOpactiyValue');
var eyebrowOpacitySlider = document.getElementById('eyebrowsOpacitySlider');

var ageingNames = ["None", "Crow's Feet", "First Signs", "Middle Aged", "Worry Lines", "Depression", "Distinguished", "Aged",
    "Weathered", "Wrinkled", "Sagging", "Tough Life", "Vintage", "Retired", "Junkie", "Geriatric"]

var currentAgeing = 0;

var ageingText = document.getElementById('ageingName');

var ageingOpacity = 50;

var ageingOpacityField = document.getElementById('ageingOpacityValue');
var ageingOpacitySlider = document.getElementById('ageingOpacitySlider');

var makeupNames = ["None", "Smoky Black", "Bronze", "Soft Gray", "Retro Glam", "Natural Look", "Cat Eyes", "Chola", "Vamp",
    "Vinewood Glamour", "Bubblegum", "Aqua Dream", "Pin Up", "Purple Passion", "Smoky Cat Eye",
    "Smoldering Ruby", "Pop Princess"]

var currentMakeup = 0;

var makeupText = document.getElementById('makeupName');

var makeupOpacity = 50;

var makeupOpacityField = document.getElementById('makeupOpacityValue');
var makeupOpacitySlider = document.getElementById('makeupOpacitySlider');

var blushNames = ["None", "Full", "Angled", "Round", "Horizontal", "High", "Sweetheart", "Eighties"]

var currentBlush = 0;

var blushText = document.getElementById('blushName');

var blushOpacity = 50;

var blushOpacityField = document.getElementById('blushOpacityValue');
var blushOpacitySlider = document.getElementById('blushOpacitySlider');

var complextionNames = ["None", "Rosy Cheeks", "Stubble Rash", "Hot Flush", "Sunburn", "Bruised", "Alchoholic", "Patchy", "Totem", "Blood Vessels", "Damaged", "Pale", "Ghostly"]

var currentComplextion = 0;

var complextionText = document.getElementById('complextionName');

var complextionOpacity = 50;

var complextionOpacityField = document.getElementById('complextionOpacityValue');
var complextionOpacitySlider = document.getElementById('complextionOpacitySlider');

var sunDamageNames = ["None", "Uneven", "Sandpaper", "Patchy", "Rough", "Leathery", "Textured", "Coarse", "Rugged", "Creased", "Cracked", "Gritty"]

var currentsunDamage = 0;

var sunDamageText = document.getElementById('sunDamageName');

var sunDamageOpacity = 50;

var sunDamageOpacityField = document.getElementById('sunDamageOpacityValue');
var sunDamageOpacitySlider = document.getElementById('sunDamageOpacitySlider');

var lipstickNames = ["None", "Color Matte", "Color Gloss", "Lined Matte", "Lined Gloss", "Heavy Lined Matte",
    "Heavy Lined Gloss", "Lined Nude Matte", "Liner Nude Gloss", "Smudged", "Geisha"]

var currentlipstick = 0;

var lipstickText = document.getElementById('lipstickName');

var lipstickOpacity = 50;

var lipstickOpacityField = document.getElementById('lipstickOpacityValue');
var lipstickOpacitySlider = document.getElementById('lipstickOpacitySlider');

var frecklesNames = ["None", "Cherub", "All Over", "Irregular", "Dot Dash", "Over the Bridge", "Baby Doll", "Pixie",
    "Sun Kissed", "Beauty Marks", "Line Up", "Modelesque", "Occasional", "Speckled", "Rain Drops", "Double Dip",
    "One Sided", "Pairs", "Growth"]

var currentfreckles = 0;

var frecklesText = document.getElementById('frecklesName');

var frecklesOpacity = 50;

var frecklesOpacityField = document.getElementById('frecklesOpacityValue');
var frecklesOpacitySlider = document.getElementById('frecklesOpacitySlider');

var chestHairNames = ["None", "Natural", "The Strip", "The Tree", "Hairy", "Grisly", "Ape", "Groomed Ape", "Bikini",
    "Lightning Bolt", "Reverse Lightning", "Love Heart", "Chestache", "Happy Face", "Skull", "Snail Trail",
    "Slug and Nips", "Hairy Arms"]

var currentchestHair = 0;

var chestHairText = document.getElementById('chestHairName');

var chestHairOpacity = 50;

var chestHairOpacityField = document.getElementById('chestHairOpacityValue');
var chestHairOpacitySlider = document.getElementById('chestHairOpacitySlider');

//#endregion

//#region Hair Colors

var maleHairs = ["Close Shave", "Buzzcut", "Faux Hawk", "Hipster", "Side Parting", "Shorter Cut", "Biker", "Ponytail",
    "Cornrows", "Slicked", "Short Brushed", "Spikey", "Caesar", "Chopped",
    "Dreads", "Long Hair", "Shaggy Curls", "Surfer Dude", "Short Side Part", "High Slicked Sides",
    "Long Slicked", "Hipster Youth", "Mullet", "Classic Cornrows", "Palm Cornrows", "Lightning Cornrows",
    "Whipped Cornrows", "Zig Zag Cornrows", "Snail Cornrows", "Hightop", "Loose Swept Back",
    "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk", "Mod", "Layered Mod", "Flattop",
    "Military Buzzcut"];

var maleHairsInt = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 24, 25, 26, 27, 28, 29,
    30, 31, 32, 33, 34, 35, 36, 72, 73];

var femaleHairs = ["Close Shave", "Short", "Layered Bob", "Pigtails", "Ponytail", "Braided Mohawk", "Braids", "Bob", "Faux Hawk", "French Twist",
    "Long Bob", "Loose Tied", "Pixie", "Shaved Bangs", "Top Knot", "Wavy Bob", "Messy Bun", "Pin Up Girl", "Tight Bun", "Twisted Bob",
    "Flapper Bob", "Big Bangs", "Braided Top Knot", "Mullet", "Pinched Cornrows", "Leaf Cornrows", "Zig Zag Cornrows", "Pigtail Bangs",
    "Wave Braids", "Coil Braids", "Rolled Quiff", "Loose Swept Back", "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk",
    "Bandana and Braid", "Layered Mod", "Skinbyrd", "Neat Bun", "Right Side Braid", "Short Right Part", "Long back Middle Part", "Long Middle Part",
    "Short Left Part", "Medium Straight", "Bob Bandana", "Medium Left Beach Wave", "Left High Pony", "Long Dreads", "Medium Smooth", "Long Braid Crown",
    "Loose Bun", "Looped Pigtails Bangs", "Medium one shoulder", "Long Mermaid Bangs", "Dreaded High Pony", "Medium Right Waves", "High Bun bangs",
    "Short Afro", "Short Curly Bands", "Thick Right Braid", "Long Left Deep Part", "Short Volume Bob", "Top Bun Wispy Bangs", "Long Straight Middle Part",
    "Long Straight Middle Part Bangs", "Medium Waves Middle Part", "High Pony Bangs", "High Pony Highlights", "Short Wide Right Part", "Long Super Straight",
    "Up do Sweep Across", "Long back layers", "Dreaded Bob", "Long back Highlights", "Left Braid", "Tight Bun Side Swept", "Cute Pixie"];

var femaleHairsInt = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 25, 26, 27, 28, 29, 30, 31,
    32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60,
    61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77];

var currentHair = 0;
var currentHairInt = 0;

var hairTypeName = document.getElementById('hairTypeName');

var hairColorValue = document.getElementById('hairColorValue');
var hairColorSlider = document.getElementById('hairColorSlider');

var currentHairColor = 0;

var hairHighlightColorValue = document.getElementById('hairHighlightColorValue');
var hairHighlightColorSlider = document.getElementById('hairHighlightColorSlider');

var currenthairHighlightColor = 0;

var eyebrowColorValue = document.getElementById('eyebrowColorValue');
var eyebrowColorSlider = document.getElementById('eyebrowColorSlider');

var currenteyebrowColor = 0;

var facialHairColorValue = document.getElementById('facialHairColorValue');
var facialHairColorSlider = document.getElementById('facialHairColorSlider');

var currentfacialHairColor = 0;

var blushColorValue = document.getElementById('blushColorValue');
var blushColorSlider = document.getElementById('blushColorSlider');

var currentblushColor = 0;

var lipstickColorValue = document.getElementById('lipstickColorValue');
var lipstickColorSlider = document.getElementById('lipstickColorSlider');

var currentlipstickColor = 0;

var chestHairColorValue = document.getElementById('chestHairColorValue');
var chestHairColorSlider = document.getElementById('chestHairColorSlider');

var currentchestHairColor = 0;

var eyeColorArray = ["Green", "Emerald", "Light Blue", "Ocean Blue", "Light Brown", "Dark Brown", "Hazel", "Dark Gray",
    "Light Gray", "Pink", "Yellow", "Purple", "Blackout", "Shades of Gray", "Tequila Sunrise", "Atomic", "Warp",
    "ECola", "Space Ranger", "Ying Yang", "Bullseye", "Lizard", "Dragon", "Extra Terrestrial", "Goat", "Smiley",
    "Possessed", "Demon", "Infected", "Alien", "Undead", "Zombie"];

var eyeColorValue = document.getElementById('eyeColorValue');

var currentEyeColorId = 0;
//#endregion

function setCurrentCharacter(characterString){
    let customCharacter = JSON.parse(characterString);

    let parentInfo = JSON.parse(customCharacter.Parents);

    let features = JSON.parse(customCharacter.Features);

    let appearanceItems = JSON.parse(customCharacter.Appearance);

    let hairInfo = JSON.parse(customCharacter.Hair);
    //#region Parents

    ParentOne = ParentNames[parentInfo.Father];
    ParentOneNameIndex = parentInfo.Father;
    ParentOneIndex = parentIndex[parentInfo.Father];

    ParentTwo = ParentNames[parentInfo.Mother];
    ParentTwoNameIndex = parentInfo.Mother;
    ParentTwoIndex = parentIndex[parentInfo.Mother];

    let parentOneField = document.getElementById('ParentOneName');
    let parentTwoField = document.getElementById('ParentTwoName');
    parentOneField.innerHTML = ParentOne + "(" + ParentOneIndex + ")";
    parentTwoField.innerHTML = ParentTwo + "(" + ParentTwoIndex + ")";

    parentMix = parentInfo.Similarity;
    parentMixValue.innerHTML = parentInfo.Similarity;
    parentMixSlider.value = parentMix;

    if(parentInfo.FatherSkin === undefined){
        parentInfo.FatherSkin = 0;
    }
    if(parentInfo.MotherSkin === undefined){
        parentInfo.MotherSkin = 0;
    }

    ParentOneSkin = ParentNames[parentInfo.FatherSkin];
    ParentTwoSkin = ParentNames[parentInfo.MotherSkin];
    ParentOneNameIndexSkin = parentInfo.FatherSkin;
    ParentTwoNameIndexSkin = parentInfo.MotherSkin;
    ParentOneIndexSkin = parentIndex[parentInfo.FatherSkin];
    ParentTwoIndexSkin = parentIndex[parentInfo.MotherSkin];

    let parentOneSkinField = document.getElementById('ParentOneSkinName');
    let parentTwoSkinField = document.getElementById('ParentTwoSkinName');
    parentOneSkinField.innerHTML = ParentOneSkin;
    parentTwoSkinField.innerHTML = ParentTwoSkin;

    skinMixSlider.value = parentInfo.SkinSimilarity;
    skinMixValue.innerHTML = parentInfo.SkinSimilarity;

    //#endregion Parents

    //#region Facial Features
    

    //#region Nose Width
    noseWidth = features[0];
    noseWidthSlider.value = noseWidth;
    noseWidthValue.innerHTML = noseWidthSlider.value;
    //#endregion

    //#region Nose Bottom height

    noseBottom = features[1];
    noseBottomSlider.value = noseBottom;
    noseBottomValue.innerHTML = noseBottomSlider.value;

    //#endregion

    //#region Nose Tip Length
    noseTipLength = features[2];
    noseTipLengthSlider.value = noseTipLength;
    noseTipLengthValue.innerHTML = noseTipLengthSlider.value;
    //#endregion

    //#region Nose Bridge Depth
    noseBridgeDepth = features[3];

    noseBridgeDepthSlider.value = noseBridgeDepth;
    noseBridgeDepthValue.innerHTML = noseBridgeDepthSlider.value;
    
    //#endregion

    //#region Nose Tip Height
    noseTipHeight = features[4];
    noseTipHeightSlider.value = noseTipHeight;
    noseTipHeightValue.innerHTML = noseTipHeightSlider.value;
    //#endregion

    //#region Nose Broken
    noseBroken = features[5];

    noseBrokenSlider.value = noseBroken;
    noseBrokenValue.innerHTML = noseBrokenSlider.value;
    //#endregion

    //#region Brow Height
    browHeight = features[6];

    browHeightSlider.value = browHeight;
    browHeightValue.innerHTML = browHeightSlider.value;
    //#endregion

    //#region Brow Depth
    browDepth = features[7];

    browDepthSlider.value = browDepth;
    browDepthValue.innerHTML = browDepthSlider.value;
    //#endregion

    //#region Cheekbone Height
    
    cheekboneHeight = features[8];

    checkBoneHeightSlider.value = cheekboneHeight;
    cheekBoneHeightValue.innerHTML = checkBoneHeightSlider.value;
    //#endregion

    //#region Check Bone Width

    cheekboneWidth = features[9];

    checkBoneWidthSlider.value = cheekboneWidth;
    cheekBoneWidthValue.innerHTML = checkBoneWidthSlider.value;
    //#endregion

    //#region Cheek Depth
    cheekDepth = features[10];
    cheekDepthSlider.value = cheekDepth;
    cheekDepthValue.innerHTML = cheekDepthSlider.value;
    //#endregion

    //#region Eye Size
    eyeSize = features[11];

    eyeSizeSlider.value = eyeSize;
    eyeSizeValue.innerHTML = eyeSizeSlider.value;
    //#endregion

    //#region Lip Thickness
    
    lipThickness = features[12];

    lipThicknessSlider.value = lipThickness;
    lipThicknessValue.innerHTML = lipThicknessSlider.value;
    //#endregion

    //#region Jaw Depth
        
    jawWidth = features[13];

    jawWidthSlider.value = jawWidth;
    jawWidthValue.innerHTML = jawWidthSlider.value;
    //#endregion

    //#region Jaw Height
    
    jawHeight = features[14];

    jawHeightSlider.value = jawHeight;
    jawHeightValue.innerHTML = jawHeightSlider.value;
    //#endregion

    //#region Chin Height
    chinHeight = features[15];

    chinHeightSlider.value = chinHeight;
    chinHeightValue.innerHTML = chinHeightSlider.value;
    //#endregion

    //#region Chin Depth
    
    chinDepth = features[16];

    chinDepthSlider.value = chinDepth;
    chinDepthValue.innerHTML = chinDepthSlider.value;
    //#endregion

    //#region Chin Width
    
    chinWidth = features[17];

    chinWidthSlider.value = chinWidth;
    chinWidthValue.innerHTML = chinWidthSlider.value;

    //#endregion

    //#region Chin Indent
    
    chinIndent = features[18];

    chinIndentSlider.value = chinIndent;
    chinIndentValue.innerHTML = chinIndentSlider.value;
    
    //#endregion

    //#region Neck Width
    
    neckWidth = features[19];

    neckWidthSlider.value = neckWidth;
    neckWidthValue.innerHTML = neckWidthSlider.value;
    //#endregion
    //#endregion

    //#region Apperances

    for (let index = 0; index < appearanceItems.length; index++) {
        const element = appearanceItems[index];

        console.log('CEF Appearance: ' + index + ', Value: ' + element.Value+ ', Opacity: ' + (element.Opacity * 100).toFixed(2));

        if(element.Value !== 255) continue;
        element.Value = -1;
    }

    currentBlemish = appearanceItems[0].Value + 1;
    blemishText.innerHTML = blemishNames[currentBlemish];

    blemishOpacity = (appearanceItems[0].Opacity * 100).toFixed(0);
    blemishOpactiySlider.value = blemishOpacity;
    blemishOpactiyField.innerHTML = blemishOpacity + "%";

    currentFacialHair = appearanceItems[1].Value + 1;
    facialHairText.innerHTML = facialHairNames[currentFacialHair];

    facialHairOpacity = (appearanceItems[1].Opacity * 100).toFixed(0);
    facialHairOpacitySlider.value = facialHairOpacity;
    facialHairOpactiyField.innerHTML = facialHairOpacity + "%";

    currentEyebrow = appearanceItems[2].Value + 1;
    eyebrowText.innerHTML = eyebrowNames[currentEyebrow];

    eyebrowOpacity = (appearanceItems[2].Opacity * 100).toFixed(0);
    eyebrowOpactiyField.innerHTML = eyebrowOpacity + "%";
    eyebrowOpacitySlider.value = eyebrowOpacity;

    currentAgeing = appearanceItems[3].Value + 1;
    ageingText.innerHTML = ageingNames[currentAgeing];

    ageingOpacity = (appearanceItems[3].Opacity * 100).toFixed(0);
    ageingOpacityField.innerHTML = ageingOpacity + "%";
    ageingOpacitySlider.value = ageingOpacity;

    currentMakeup = appearanceItems[4].Value + 1;
    makeupText.innerHTML = makeupNames[currentMakeup];

    makeupOpacity = (appearanceItems[4].Opacity * 100).toFixed(0);
    makeupOpacityField.innerHTML = makeupOpacity + "%";
    makeupOpacitySlider.value = makeupOpacity;

    currentBlush = appearanceItems[5].Value + 1;
    blushText.innerHTML = blushNames[currentBlush];

    blushOpacity = (appearanceItems[5].Opacity * 100).toFixed(0);
    blushOpacityField.innerHTML = blushOpacity + "%";
    blushOpacitySlider.value = blushOpacity;

    currentComplextion = appearanceItems[6].Value + 1;
    complextionText.innerHTML = complextionNames[currentComplextion];

    complextionOpacity = (appearanceItems[6].Opacity * 100).toFixed(0);
    complextionOpacityField.innerHTML = complextionOpacity + "%";
    complextionOpacitySlider.value = complextionOpacity;

    currentsunDamage = appearanceItems[7].Value + 1;
    sunDamageText.innerHTML = sunDamageNames[currentsunDamage];

    sunDamageOpacity = (appearanceItems[7].Opacity * 100).toFixed(0);
    sunDamageOpacityField.innerHTML = sunDamageOpacity + "%";
    sunDamageOpacitySlider.value = sunDamageOpacity;

    currentlipstick = appearanceItems[8].Value + 1;
    lipstickText.innerHTML = lipstickNames[currentlipstick];

    lipstickOpacity = (appearanceItems[8].Opacity * 100).toFixed(0);
    lipstickOpacityField.innerHTML = lipstickOpacity + "%";
    lipstickOpacitySlider.value = lipstickOpacity;

    currentfreckles = appearanceItems[9].Value + 1;
    frecklesText.innerHTML = frecklesNames[currentfreckles];

    frecklesOpacity = (appearanceItems[9].Opacity * 100).toFixed(0);
    frecklesOpacityField.innerHTML = frecklesOpacity + "%";
    frecklesOpacitySlider.value = frecklesOpacity;

    currentchestHair = appearanceItems[10].Value + 1;
    chestHairText.innerHTML = chestHairNames[currentchestHair];

    chestHairOpacity = (appearanceItems[10].Opacity * 100).toFixed(0);
    chestHairOpacityField.innerHTML = chestHairOpacity+ "%";
    chestHairOpacitySlider.value = chestHairOpacity;

    //#endregion

    //#region Hair & Colors
    
    currentHair = hairInfo.Hair;

    if(gender === 0){
        // Male
        currentHairInt = maleHairsInt[currentHair];
        hairTypeName.innerHTML = maleHairs[currentHair];
    }
    else{
        currentHairInt = femaleHairsInt[currentHair];
        hairTypeName.innerHTML = femaleHairs[currentHair];
    }

    currentHairColor = hairInfo.Color;
    hairColorValue.innerHTML = currentHairColor;
    hairColorSlider.value = currentHairColor;

    currenthairHighlightColor = hairInfo.HighlightColor;
    hairHighlightColorValue.innerHTML = currenthairHighlightColor;
    hairHighlightColorSlider.value = currenthairHighlightColor;

    currenteyebrowColor = customCharacter.EyebrowColor;
    eyebrowColorValue.innerHTML = currenteyebrowColor;
    eyebrowColorSlider.value = currenteyebrowColor;

    currentfacialHairColor = customCharacter.BeardColor;
    facialHairColorValue.innerHTML = currentfacialHairColor;
    facialHairColorSlider.value = currentfacialHairColor;

    currentblushColor = customCharacter.BlushColor;
    blushColorValue.innerHTML = currentblushColor;
    blushColorSlider.value = currentblushColor;

    currentlipstickColor = customCharacter.LipstickColor;
    lipstickColorValue.innerHTML = currentlipstickColor;
    lipstickColorSlider.value = currentlipstickColor;

    currentchestHairColor = customCharacter.ChestHairColor;
    chestHairColorValue.innerHTML = currentchestHairColor;
    chestHairColorSlider.value = currentchestHairColor;

    currentEyeColorId = customCharacter.EyeColor;
    eyeColorValue.innerHTML = eyeColorArray[currentEyeColorId];

    //#endregion
}


//#region Collapse Handler

//Collapsable Handler
var coll = document.getElementsByClassName("collapsible");
var i;

for (i = 0; i < coll.length; i++) {
    coll[i].addEventListener("click", function () {
        this.classList.toggle("active");
        var content = this.nextElementSibling;
        if (content.style.display === "block") {
            content.style.display = "none";
        } else {
            content.style.display = "block";
        }
    });
}

//#endregion

//#region Parents


function LoadParentNames() {
    var parentOneField = document.getElementById('ParentOneName');
    var parentTwoField = document.getElementById('ParentTwoName');

    parentOneField.innerHTML = ParentOne + "(" + ParentOneIndex + ")";
    parentTwoField.innerHTML = ParentTwo + "(" + ParentTwoIndex + ")";

    var parentOneSkinField = document.getElementById('ParentOneSkinName');
    var parentTwoSkinField = document.getElementById('ParentTwoSkinName');

    parentOneSkinField.innerHTML = ParentOneSkin;
    parentTwoSkinField.innerHTML = ParentTwoSkin;
}

function ParentOnePrevious() {
    var parentOneField = document.getElementById('ParentOneName');
    if (ParentOneNameIndex === 0) {
        //At start
        ParentOneNameIndex = ParentNames.length - 1;
    }
    else {
        ParentOneNameIndex -= 1;
    }

    ParentOneIndex = parentIndex[ParentOneNameIndex];

    ParentOne = ParentNames[ParentOneNameIndex];
    parentOneField.innerHTML = ParentOne + "(" + ParentOneIndex + ")";
    alt.emit('ParentChange', 1, ParentOneIndex);
}

function ParentOneNext() {
    var parentOneField = document.getElementById('ParentOneName');
    if (ParentOneNameIndex === ParentNames.length - 1) {
        // At end
        ParentOneNameIndex = 0;
    }
    else {
        ParentOneNameIndex += 1;
    }

    ParentOneIndex = parentIndex[ParentOneNameIndex];

    ParentOne = ParentNames[ParentOneNameIndex];
    parentOneField.innerHTML = ParentOne + "(" + ParentOneIndex + ")";

    console.log("Index: " + ParentOneNameIndex);
    console.log("Name: " + ParentOne);
    console.log("ID Index: " + ParentOneIndex);

    alt.emit('ParentChange', 1, ParentOneIndex);
}

function ParentTwoPrevious() {
    var parentTwoField = document.getElementById('ParentTwoName');
    if (ParentTwoNameIndex === 0) {
        //At start
        ParentTwoNameIndex = ParentNames.length - 1;
    }
    else {
        ParentTwoNameIndex -= 1;
    }

    ParentTwoIndex = parentIndex[ParentTwoNameIndex];

    ParentTwo = ParentNames[ParentTwoNameIndex];
    parentTwoField.innerHTML = ParentTwo + "(" + ParentTwoIndex + ")";

    alt.emit('ParentChange', 2, ParentTwoIndex);
}

function ParentTwoNext() {
    var parentTwoField = document.getElementById('ParentTwoName');
    if (ParentTwoNameIndex === ParentNames.length - 1) {
        // At end
        ParentTwoNameIndex = 0;
    }
    else {
        ParentTwoNameIndex += 1;
    }

    ParentTwoIndex = parentIndex[ParentTwoNameIndex];

    ParentTwo = ParentNames[ParentTwoNameIndex];
    parentTwoField.innerHTML = ParentTwo + "(" + ParentTwoIndex + ")";

    alt.emit('ParentChange', 2, ParentTwoIndex);
}


parentMixSlider.oninput = function () {
    parentMixValue.innerHTML = this.value;
    parentMix = this.value;
    alt.emit('creator:parentMixChange', this.value);
}


function ParentOneSkinPrevious() {
    var parentOneSkinField = document.getElementById('ParentOneSkinName');
    if (ParentOneNameIndexSkin === 0) {
        //At start
        ParentOneNameIndexSkin = ParentNames.length - 1;
    }
    else {
        ParentOneNameIndexSkin -= 1;
    }

    ParentOneSkin = ParentNames[ParentOneNameIndexSkin];
    parentOneSkinField.innerHTML = ParentOneSkin;

    ParentOneIndexSkin = parentIndex[ParentOneNameIndexSkin];

    alt.emit('ParentSkinChange', 1, ParentOneIndexSkin);
}

function ParentOneSkinNext() {
    var parentOneSkinField = document.getElementById('ParentOneSkinName');
    if (ParentOneNameIndexSkin === ParentNames.length - 1) {
        // At end
        ParentOneNameIndexSkin = 0;
    }
    else {
        ParentOneNameIndexSkin += 1;
    }

    ParentOneSkin = ParentNames[ParentOneNameIndexSkin];
    parentOneSkinField.innerHTML = ParentOneSkin;

    ParentOneIndexSkin = parentIndex[ParentOneNameIndexSkin];

    alt.emit('ParentSkinChange', 1, ParentOneIndexSkin);
}

function ParentTwoSkinPrevious() {
    var parentTwoSkinField = document.getElementById('ParentTwoSkinName');
    if (ParentTwoNameIndexSkin === 0) {
        //At start
        ParentTwoNameIndexSkin = ParentNames.length - 1;
    }
    else {
        ParentTwoNameIndexSkin -= 1;
    }

    ParentTwoSkin = ParentNames[ParentTwoNameIndexSkin];
    parentTwoSkinField.innerHTML = ParentTwoSkin;

    ParentTwoIndexSkin = parentIndex[ParentTwoNameIndexSkin];

    alt.emit('ParentSkinChange', 2, ParentTwoIndexSkin);
}

function ParentTwoSkinNext() {
    var parentTwoSkinField = document.getElementById('ParentTwoSkinName');
    if (ParentTwoNameIndexSkin === ParentNames.length - 1) {
        // At end
        ParentTwoNameIndexSkin = 0;
    }
    else {
        ParentTwoNameIndexSkin += 1;
    }

    ParentTwoSkin = ParentNames[ParentTwoNameIndexSkin];
    parentTwoSkinField.innerHTML = ParentTwoSkin;

    ParentTwoIndex = parentIndex[ParentTwoNameIndexSkin];

    alt.emit('ParentSkinChange', 2, ParentTwoIndex);
}


skinMixSlider.oninput = function () {
    skinMixValue.innerHTML = this.value;
    skinMix = this.value;
    alt.emit('creator:skinMixChange', this.value);
}
//#endregion

//#region Facial Features

//#region Nose Width

noseWidthSlider.oninput = function () {
    noseWidthValue.innerHTML = this.value;
    noseWidth = this.value;
    alt.emit('creator:facialFeatureUpdate', 0, this.value);
}

//#endregion

//#region Nose Bottom height


noseBottomSlider.oninput = function () {
    noseBottomValue.innerHTML = this.value;
    noseBottom = this.value;

    alt.emit('creator:facialFeatureUpdate', 1, this.value);
}
//#endregion

//#region Nose Tip Length

noseTipLengthSlider.oninput = function () {
    noseTipLengthValue.innerHTML = this.value;
    noseTipLength = this.value;
    alt.emit('creator:facialFeatureUpdate', 2, this.value);
}
//#endregion

//#region Nose Bridge Depth

noseBridgeDepthSlider.oninput = function () {
    noseBridgeDepthValue.innerHTML = this.value;
    noseBridgeDepth = this.value;

    alt.emit('creator:facialFeatureUpdate', 3, this.value);
}
//#endregion

//#region Nose Tip Height

noseTipHeightSlider.oninput = function () {
    noseTipHeightValue.innerHTML = this.value;
    noseTipHeight = this.value;

    alt.emit('creator:facialFeatureUpdate', 4, this.value);
}
//#endregion

//#region Nose Broken

noseBrokenSlider.oninput = function () {
    noseBrokenValue.innerHTML = this.value;
    noseBroken = this.value;

    alt.emit('creator:facialFeatureUpdate', 5, this.value);
}
//#endregion

//#region Brow Height

browHeightSlider.oninput = function () {
    browHeightValue.innerHTML = this.value;
    browHeight = this.value;

    alt.emit('creator:facialFeatureUpdate', 6, this.value);
}
//#endregion

//#region Brow Depth
var browDepth = 0.0;

var browDepthSlider = document.getElementById('browDepthSlider');
var browDepthValue = document.getElementById('browDepthValue');

browDepthSlider.value = browDepth;
browDepthValue.innerHTML = browDepthSlider.value;

browDepthSlider.oninput = function () {
    browDepthValue.innerHTML = this.value;
    browDepth = this.value;
    alt.emit('creator:facialFeatureUpdate', 7, this.value);
}
//#endregion

//#region Cheekbone Height

checkBoneHeightSlider.oninput = function () {
    cheekBoneHeightValue.innerHTML = this.value;
    cheekboneHeight = this.value;

    alt.emit('creator:facialFeatureUpdate', 8, this.value);
}
//#endregion

//#region Cheekbone Width

checkBoneWidthSlider.oninput = function () {
    cheekBoneWidthValue.innerHTML = this.value;
    cheekboneWidth = this.value;
    alt.emit('creator:facialFeatureUpdate', 9, this.value);
}
//#endregion

//#region Cheek Depth

cheekDepthSlider.oninput = function () {
    cheekDepthValue.innerHTML = this.value;
    cheekDepth = this.value;

    alt.emit('creator:facialFeatureUpdate', 10, this.value);
}
//#endregion

//#region Eye Size

eyeSizeSlider.oninput = function () {
    eyeSizeValue.innerHTML = this.value;
    eyeSize = this.value;

    alt.emit('creator:facialFeatureUpdate', 11, this.value);
}
//#endregion

//#region Lip Thickness

lipThicknessSlider.oninput = function () {
    lipThicknessValue.innerHTML = this.value;
    lipThickness = this.value;

    alt.emit('creator:facialFeatureUpdate', 12, this.value);
}
//#endregion

//#region Jaw Width

jawWidthSlider.oninput = function () {
    jawWidthValue.innerHTML = this.value;
    jawWidth = this.value;

    alt.emit('creator:facialFeatureUpdate', 13, this.value);
}
//#endregion

//#region Jaw Height

jawHeightSlider.oninput = function () {
    jawHeightValue.innerHTML = this.value;
    jawHeight = this.value;
    alt.emit('creator:facialFeatureUpdate', 14, this.value);
}
//#endregion

//#region Chin Height

chinHeightSlider.oninput = function () {
    chinHeightValue.innerHTML = this.value;
    chinHeight = this.value;

    alt.emit('creator:facialFeatureUpdate', 15, this.value);
}
//#endregion

//#region Chin Depth

chinDepthSlider.oninput = function () {
    chinDepthValue.innerHTML = this.value;
    chinDepth = this.value;

    alt.emit('creator:facialFeatureUpdate', 16, this.value);
}
//#endregion

//#region Chin Width

chinWidthSlider.oninput = function () {
    chinWidthValue.innerHTML = this.value;
    chinWidth = this.value;

    alt.emit('creator:facialFeatureUpdate', 17, this.value);
}
//#endregion

//#region Chin Indent

chinIndentSlider.oninput = function () {
    chinIndentValue.innerHTML = this.value;
    chinIndent = this.value;
    alt.emit('creator:facialFeatureUpdate', 18, this.value);
}
//#endregion

//#region Neck Width

neckWidthSlider.oninput = function () {
    neckWidthValue.innerHTML = this.value;
    neckWidth = this.value;

    alt.emit('creator:facialFeatureUpdate', 19, this.value);
}
//#endregion

function LoadFeatureMenu() {
    noseWidthSlider.value = noseWidth;
    noseWidthValue.innerHTML = noseWidthSlider.value;
    noseBottomSlider.value = noseBottom;
    noseBottomValue.innerHTML = noseBottomSlider.value;
    noseTipLengthSlider.value = noseTipLength;
    noseTipLengthValue.innerHTML = noseTipLengthSlider.value;
    noseBridgeDepthSlider.value = noseBridgeDepth;
    noseBridgeDepthValue.innerHTML = noseBridgeDepthSlider.value;
    noseTipHeightSlider.value = noseTipHeight;
    noseTipHeightValue.innerHTML = noseTipHeightSlider.value;
    noseBrokenSlider.value = noseTipHeight;
    noseBrokenValue.innerHTML = noseBrokenSlider.value;
    browHeightSlider.value = browHeight;
    browHeightValue.innerHTML = browHeightSlider.value;
    browDepthSlider.value = browDepth;
    browDepthValue.innerHTML = browDepthSlider.value;
    checkBoneHeightSlider.value = cheekboneHeight;
    cheekBoneHeightValue.innerHTML = checkBoneHeightSlider.value;
    checkBoneWidthSlider.value = cheekboneWidth;
    cheekBoneWidthValue.innerHTML = checkBoneWidthSlider.value;
    cheekDepthSlider.value = cheekDepth;
    cheekDepthValue.innerHTML = cheekDepthSlider.value;
    eyeSizeSlider.value = eyeSize;
    eyeSizeValue.innerHTML = eyeSizeSlider.value;
    lipThicknessSlider.value = lipThickness;
    lipThicknessValue.innerHTML = lipThicknessSlider.value;
    jawWidthSlider.value = jawWidth;
    jawWidthValue.innerHTML = jawWidthSlider.value;
    jawHeightSlider.value = jawHeight;
    jawHeightValue.innerHTML = jawHeightSlider.value;
    chinHeightSlider.value = chinHeight;
    chinHeightValue.innerHTML = chinHeightSlider.value;
    chinWidthSlider.value = chinWidth;
    chinWidthValue.innerHTML = chinWidthSlider.value;
    chinIndentSlider.value = chinIndent;
    chinIndentValue.innerHTML = chinIndentSlider.value;
    neckWidthSlider.value = neckWidth;
    neckWidthValue.innerHTML = neckWidthSlider.value;
}

//#endregion

//#region Appearances

//Blemishes, Facial Hair, Eyebrows, Ageing, Makeup, Blush, Complextion, Sun Damage, Lipstick, Moles/Freckles, Chest Hair

//#region Blemishes


function previousBlemish() {
    if (currentBlemish === 0) {
        // At start
        currentBlemish = blemishNames.length - 1;
    }
    else {
        currentBlemish -= 1;
    }
    blemishText.innerHTML = blemishNames[currentBlemish];
    alt.emit('creator:setFacialApperance', 0, currentBlemish, blemishOpacity);
}

function nextBlemish() {
    if (currentBlemish === blemishNames.length - 1) {
        //At end
        currentBlemish = 0;
    }

    else {
        currentBlemish += 1;
    }

    blemishText.innerHTML = blemishNames[currentBlemish];
    alt.emit('creator:setFacialApperance', 0, currentBlemish, blemishOpacity);
}

blemishOpactiySlider.oninput = function () {
    blemishOpactiyField.innerHTML = this.value + "%";
    blemishOpacity = this.value;
    alt.emit('creator:setFacialApperance', 0, currentBlemish, blemishOpacity);
}

//#endregion

//#region Facial Hair

function previousFacialHair() {
    if (currentFacialHair === 0) {
        // At start
        currentFacialHair = facialHairNames.length - 1;
    }
    else {
        currentFacialHair -= 1;
    }
    facialHairText.innerHTML = facialHairNames[currentFacialHair];
    alt.emit('creator:setFacialApperance', 1, currentFacialHair, facialHairOpacity);
}

function nextFacialHair() {
    if (currentFacialHair === facialHairNames.length - 1) {
        //At end
        currentFacialHair = 0;
    }

    else {
        currentFacialHair += 1;
    }

    facialHairText.innerHTML = facialHairNames[currentFacialHair];
    alt.emit('creator:setFacialApperance', 1, currentFacialHair, facialHairOpacity);
}

facialHairOpacitySlider.oninput = function () {
    facialHairOpactiyField.innerHTML = this.value + "%";
    facialHairOpacity = this.value;
    alt.emit('creator:setFacialApperance', 1, currentFacialHair, facialHairOpacity);
}

//#endregion

//#region Eyebrows



function previousEyebrows() {
    if (currentEyebrow === 0) {
        // At start
        currentEyebrow = eyebrowNames.length - 1;
    }
    else {
        currentEyebrow -= 1;
    }
    eyebrowText.innerHTML = eyebrowNames[currentEyebrow];
    alt.emit('creator:setFacialApperance', 2, currentEyebrow, eyebrowOpacity);
}

function nextEyebrows() {
    if (currentEyebrow === eyebrowNames.length - 1) {
        //At end
        currentEyebrow = 0;
    }

    else {
        currentEyebrow += 1;
    }

    eyebrowText.innerHTML = eyebrowNames[currentEyebrow];
    alt.emit('creator:setFacialApperance', 2, currentEyebrow, eyebrowOpacity);
}

eyebrowOpacitySlider.oninput = function () {
    eyebrowOpactiyField.innerHTML = this.value + "%";
    eyebrowOpacity = this.value;
    alt.emit('creator:setFacialApperance', 2, currentEyebrow, eyebrowOpacity);
}

//#endregion

//#region Ageing

function previousAgeing() {
    if (currentAgeing === 0) {
        // At start
        currentAgeing = ageingNames.length - 1;
    }
    else {
        currentAgeing -= 1;
    }
    ageingText.innerHTML = ageingNames[currentAgeing];
    alt.emit('creator:setFacialApperance', 3, currentAgeing, ageingOpacity);
}

function nextAgeing() {
    if (currentAgeing === ageingNames.length - 1) {
        //At end
        currentAgeing = 0;
    }

    else {
        currentAgeing += 1;
    }

    ageingText.innerHTML = ageingNames[currentAgeing];
    alt.emit('creator:setFacialApperance', 3, currentAgeing, ageingOpacity);
}

ageingOpacitySlider.oninput = function () {
    ageingOpacityField.innerHTML = this.value + "%";
    ageingOpacity = this.value;
    alt.emit('creator:setFacialApperance', 3, currentAgeing, ageingOpacity);
}

//#endregion

//#region Makeup

function previousMakeup() {
    if (currentMakeup === 0) {
        // At start
        currentMakeup = makeupNames.length - 1;
    }
    else {
        currentMakeup -= 1;
    }
    makeupText.innerHTML = makeupNames[currentMakeup];
    alt.emit('creator:setFacialApperance', 4, currentMakeup, makeupOpacity);
}

function nextMakeup() {
    if (currentMakeup === makeupNames.length - 1) {
        //At end
        currentMakeup = 0;
    }

    else {
        currentMakeup += 1;
    }

    makeupText.innerHTML = makeupNames[currentMakeup];
    alt.emit('creator:setFacialApperance', 4, currentMakeup, makeupOpacity);
}

makeupOpacitySlider.oninput = function () {
    makeupOpacityField.innerHTML = this.value + "%";
    makeupOpacity = this.value;
    alt.emit('creator:setFacialApperance', 4, currentMakeup, makeupOpacity);
}

//#endregion

//#region Blush

function previousBlush() {
    if (currentBlush === 0) {
        // At start
        currentBlush = blushNames.length - 1;
    }
    else {
        currentBlush -= 1;
    }
    blushText.innerHTML = blushNames[currentBlush];
    alt.emit('creator:setFacialApperance', 5, currentBlush, blushOpacity);
}

function nextBlush() {
    if (currentBlush === blushNames.length - 1) {
        //At end
        currentBlush = 0;
    }

    else {
        currentBlush += 1;
    }

    blushText.innerHTML = blushNames[currentBlush];
    alt.emit('creator:setFacialApperance', 5, currentBlush, blushOpacity);
}
blushOpacitySlider.oninput = function () {
    blushOpacityField.innerHTML = this.value + "%";
    blushOpacity = this.value;
    alt.emit('creator:setFacialApperance', 5, currentBlush, blushOpacity);
}

//#endregion

//#region Complextion

function previouscomplextion() {
    if (currentComplextion === 0) {
        // At start
        currentComplextion = complextionNames.length - 1;
    }
    else {
        currentComplextion -= 1;
    }
    complextionText.innerHTML = complextionNames[currentComplextion];
    alt.emit('creator:setFacialApperance', 6, currentComplextion, complextionOpacity);
}

function nextcomplextion() {
    if (currentComplextion === complextionNames.length - 1) {
        //At end
        currentComplextion = 0;
    }

    else {
        currentComplextion += 1;
    }

    complextionText.innerHTML = complextionNames[currentComplextion];
    alt.emit('creator:setFacialApperance', 6, currentComplextion, complextionOpacity);
}

complextionOpacitySlider.oninput = function () {
    complextionOpacityField.innerHTML = this.value + "%";
    complextionOpacity = this.value;
    alt.emit('creator:setFacialApperance', 6, currentComplextion, complextionOpacity);
}

//#endregion

//#region sunDamage

function previoussunDamage() {
    if (currentsunDamage === 0) {
        // At start
        currentsunDamage = sunDamageNames.length - 1;
    }
    else {
        currentsunDamage -= 1;
    }
    sunDamageText.innerHTML = sunDamageNames[currentsunDamage];
    alt.emit('creator:setFacialApperance', 7, currentsunDamage, sunDamageOpacity);
}

function nextsunDamage() {
    if (currentsunDamage === sunDamageNames.length - 1) {
        //At end
        currentsunDamage = 0;
    }

    else {
        currentsunDamage += 1;
    }

    sunDamageText.innerHTML = sunDamageNames[currentsunDamage];
    alt.emit('creator:setFacialApperance', 7, currentsunDamage, sunDamageOpacity);
}

sunDamageOpacitySlider.oninput = function () {
    sunDamageOpacityField.innerHTML = this.value + "%";
    sunDamageOpacity = this.value;
    alt.emit('creator:setFacialApperance', 7, currentsunDamage, sunDamageOpacity);
}

//#endregion

//#region lipstick

function previouslipstick() {
    if (currentlipstick === 0) {
        // At start
        currentlipstick = lipstickNames.length - 1;
    }
    else {
        currentlipstick -= 1;
    }
    lipstickText.innerHTML = lipstickNames[currentlipstick];
    alt.emit('creator:setFacialApperance', 8, currentlipstick, lipstickOpacity);
}

function nextlipstick() {
    if (currentlipstick === lipstickNames.length - 1) {
        //At end
        currentlipstick = 0;
    }

    else {
        currentlipstick += 1;
    }

    lipstickText.innerHTML = lipstickNames[currentlipstick];
    alt.emit('creator:setFacialApperance', 8, currentlipstick, lipstickOpacity);
}

lipstickOpacitySlider.oninput = function () {
    lipstickOpacityField.innerHTML = this.value + "%";
    lipstickOpacity = this.value;
    alt.emit('creator:setFacialApperance', 8, currentlipstick, lipstickOpacity);
}

//#endregion

//#region freckles

function previousfreckles() {
    if (currentfreckles === 0) {
        // At start
        currentfreckles = frecklesNames.length - 1;
    }
    else {
        currentfreckles -= 1;
    }
    frecklesText.innerHTML = frecklesNames[currentfreckles];
    alt.emit('creator:setFacialApperance', 9, currentfreckles, frecklesOpacity);
}

function nextfreckles() {
    if (currentfreckles === frecklesNames.length - 1) {
        //At end
        currentfreckles = 0;
    }

    else {
        currentfreckles += 1;
    }

    frecklesText.innerHTML = frecklesNames[currentfreckles];
    alt.emit('creator:setFacialApperance', 9, currentfreckles, frecklesOpacity);
}

frecklesOpacitySlider.oninput = function () {
    frecklesOpacityField.innerHTML = this.value + "%";
    frecklesOpacity = this.value;
    alt.emit('creator:setFacialApperance', 9, currentfreckles, frecklesOpacity);
}

//#endregion

//#region chestHair


function previouschestHair() {
    if (currentchestHair === 0) {
        // At start
        currentchestHair = chestHairNames.length - 1;
    }
    else {
        currentchestHair -= 1;
    }
    chestHairText.innerHTML = chestHairNames[currentchestHair];
    alt.emit('creator:setFacialApperance', 10, currentchestHair, chestHairOpacity);
}

function nextchestHair() {
    if (currentchestHair === chestHairNames.length - 1) {
        //At end
        currentchestHair = 0;
    }

    else {
        currentchestHair += 1;
    }

    chestHairText.innerHTML = chestHairNames[currentchestHair];
    alt.emit('creator:setFacialApperance', 10, currentchestHair, chestHairOpacity);
}

chestHairOpacitySlider.oninput = function () {
    chestHairOpacityField.innerHTML = this.value + "%";
    chestHairOpacity = this.value;
    alt.emit('creator:setFacialApperance', 10, currentchestHair, chestHairOpacity);
}

//#endregion

function LoadAppearanceMenu() {
    blemishText.innerHTML = blemishNames[currentBlemish];
    blemishOpactiyField.innerHTML = blemishOpacity + "%";

    facialHairText.innerHTML = facialHairNames[currentFacialHair];
    facialHairOpactiyField.innerHTML = facialHairOpacity + "%";

    eyebrowText.innerHTML = eyebrowNames[currentEyebrow];
    eyebrowOpactiyField.innerHTML = eyebrowOpacity + "%";

    ageingText.innerHTML = ageingNames[currentAgeing];
    ageingOpacityField.innerHTML = ageingOpacity + "%";

    makeupText.innerHTML = makeupNames[currentMakeup];
    makeupOpacityField.innerHTML = makeupOpacity + "%";

    blushText.innerHTML = blushNames[currentBlush];
    blushOpacityField.innerHTML = blushOpacity + "%";

    complextionText.innerHTML = complextionNames[currentComplextion];
    complextionOpacityField.innerHTML = complextionOpacity + "%";

    sunDamageText.innerHTML = sunDamageNames[currentsunDamage];
    sunDamageOpacityField.innerHTML = sunDamageOpacity + "%";

    lipstickText.innerHTML = lipstickNames[currentlipstick];
    lipstickOpacityField.innerHTML = lipstickOpacity + "%";

    frecklesText.innerHTML = frecklesNames[currentfreckles];
    frecklesOpacityField.innerHTML = frecklesOpacity + "%";

    chestHairText.innerHTML = chestHairNames[currentchestHair];
    chestHairOpacityField.innerHTML = chestHairOpacity + "%";
}

//#endregion

//#region Hair & Colors

// Male Hair, Female Hair, Color 0-64, Hair Color, Eyebrow Color, Facial Hair Color, Eye Color, Blush Colors (0-27), Lipstick Color (0-32), Chest Hair Color (0-64)

//#region Hair Type


function previoushairType() {
    if (gender === 0) {
        if (currentHair === 0) {
            // At start
            currentHair = maleHairs.length - 1;
        }
        else {
            currentHair -= 1;
        }

        currentHairInt = maleHairsInt[currentHair];
        hairTypeName.innerHTML = maleHairs[currentHair];
        alt.emit('creator:onHairChange', currentHairInt);
    }
    if (gender === 1) {
        if (currentHair === 0) {
            // At start
            currentHair = femaleHairs.length - 1;
        }
        else {
            currentHair -= 1;
        }

        currentHairInt = femaleHairsInt[currentHair];
        hairTypeName.innerHTML = femaleHairs[currentHair];
        alt.emit('creator:onHairChange', currentHairInt);
    }
}

function nexthairType() {
    if (gender === 0) {
        if (currentHair === maleHairs.length - 1) {
            // At end
            currentHair = 0;
        }
        else {
            currentHair += 1;
        }

        currentHairInt = maleHairsInt[currentHair];
        hairTypeName.innerHTML = maleHairs[currentHair];
        alt.emit('creator:onHairChange', currentHairInt);
    }
    if (gender === 1) {
        if (currentHair === femaleHairs.length - 1) {
            // At start end
            currentHair = 0;
        }
        else {
            currentHair += 1;
        }

        currentHairInt = femaleHairsInt[currentHair];
        hairTypeName.innerHTML = femaleHairs[currentHair];
        alt.emit('creator:onHairChange', currentHairInt);
    }
}

//#endregion

//#region Hair Color


hairColorSlider.oninput = function () {
    hairColorValue.innerHTML = this.value;
    currentHairColor = this.value;
    alt.emit('creator:onHairColorChange', currentHairColor);
};

//#endregion

//#region Hair Highlight Color

hairHighlightColorSlider.oninput = function () {
    hairHighlightColorValue.innerHTML = this.value;
    currenthairHighlightColor = this.value;
    alt.emit('creator:onhairHighlightColorChange', currenthairHighlightColor);
}

//#endregion

//#region Eyebrow Color


eyebrowColorSlider.oninput = function () {
    eyebrowColorValue.innerHTML = this.value;
    currenteyebrowColor = this.value;
    alt.emit('creator:onEyebrowColorChange', currenteyebrowColor);
}

//#endregion

//#region Facial Hair Color


facialHairColorSlider.oninput = function () {
    facialHairColorValue.innerHTML = this.value;
    currentfacialHairColor = this.value;
    alt.emit('creator:onFacialHairColorChange', currentfacialHairColor);
}

//#endregion

//#region Blush Color


blushColorSlider.oninput = function () {
    blushColorValue.innerHTML = this.value;
    currentblushColor = this.value;
    alt.emit('creator:onblushColorChange', currentblushColor);
}

//#endregion

//#region Lipstick Color


lipstickColorSlider.oninput = function () {
    lipstickColorValue.innerHTML = this.value;
    currentlipstickColor = this.value;
    alt.emit('creator:onlipstickColorChange', currentlipstickColor);
}

//#endregion

//#region Chest Hair Color


chestHairColorSlider.oninput = function () {
    chestHairColorValue.innerHTML = this.value;
    currentchestHairColor = this.value;
    alt.emit('creator:onchestHairColorChange', currentchestHairColor);
}

//#endregion

//#region Eye Color


function previouseyeColor() {
    if (currentEyeColorId === 0) {
        // At start
        currentEyeColorId = eyeColorArray.length - 1;
    }
    else {
        currentEyeColorId -= 1;
    }

    eyeColorValue.innerHTML = eyeColorArray[currentEyeColorId];
    alt.emit('creator:onEyeColorChange', currentEyeColorId);
}

function nexteyeColor() {
    if (currentEyeColorId === eyeColorArray.length - 1) {
        currentEyeColorId = 0;
    }
    else {
        currentEyeColorId += 1;
    }

    eyeColorValue.innerHTML = eyeColorArray[currentEyeColorId];
    alt.emit('creator:onEyeColorChange', currentEyeColorId);
}

//#endregion

function LoadHairColors() {
    if (gender === 0) {
        // Male
        hairTypeName.innerHTML = maleHairs[currentHair];
    }
    if (gender === 1) {
        // Female
        hairTypeName.innerHTML = femaleHairs[currentHair];
    }

    hairColorValue.innerHTML = currentHairColor;
    hairHighlightColorValue.innerHTML = currenthairHighlightColor;
    eyebrowColorValue.innerHTML = currenteyebrowColor;
    facialHairColorValue.innerHTML = currentfacialHairColor;
    eyeColorValue.innerHTML = eyeColorArray[currentEyeColorId];
    blushColorValue.innerHTML = currentblushColor;
    lipstickColorValue.innerHTML = currentlipstickColor;
    chestHairColorValue.innerHTML = currentchestHairColor;
}

//#endregion

//#region Rotation

var currentRotation = 0;

var currentRotationValue = document.getElementById('currentRotationValue');
var rotationSlider = document.getElementById('rotationSlider');

rotationSlider.oninput = function () {
    currentRotationValue.innerHTML = this.value;
    currentRotation = this.value;
    alt.emit('creator:rotationchange', this.value);
};

function loadRotation() {
    currentRotationValue.innerHTML = rotationSlider.value;
}

//#endregion

//#region Zoom

var currentZoom = 20;

var currentZoomValue = document.getElementById('currentZoomValue');
var zoomSlider = document.getElementById('zoomSlider');

zoomSlider.oninput = function () {
    currentZoomValue.innerHTML = this.value;
    currentZoom = this.value;
    alt.emit('creator:zoomchange', this.value);
}

function loadZoom() {
    zoomSlider.value = currentZoom;
    currentZoomValue.innerHTML = zoomSlider.value;
}

//#endregion

function creatorPageLoaded() {
    alt.emit('creatorLoaded');
}

function finishCreation() {
    alt.emit('creator:finishCreation');
}

function maleGenderSelection() {
    alt.emit('creator:genderChange', 0);
    gender = 0;
}
function femaleGenderSelection() {
    alt.emit('creator:genderChange', 1);
    gender = 1;
}