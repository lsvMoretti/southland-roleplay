import * as alt from 'alt-client';
import * as native from 'natives';

var highlightColor:number = 0;
var hairColor: number = 0;

alt.onServer('HairStore:HighlightColor', (highlightIndex:number, colorIndex:number) => {
	highlightColor = highlightIndex;
	hairColor = colorIndex;
});

alt.onServer('HairStore:HairColorChange', onHairColorChange);
function onHairColorChange(type: number, index: number) {

	var player: number = alt.Player.local.scriptID;

	if (type === 0) {

		// Hair Color
		native.setPedHairColor(player, index, highlightColor);
		return;
	}
	if (type === 1) {

		// Hair Highlight Color
		native.setPedHairColor(player, hairColor, index);
		return;
	} 
	if (type === 2) {
		// Facial Hair Color

		native.setPedHeadOverlayColor(player, 1, 1, index, 0);
	}
	
	if (type === 3) {
		// Eyebrow Color

		native.setPedHeadOverlayColor(player, 2, 1, index, 0);
	}

	if (type === 4) {
		// Chest Hair Color

		native.setPedHeadOverlayColor(player, 10, 1, index, 0);
	}
}