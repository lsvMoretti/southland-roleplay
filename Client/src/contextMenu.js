import alt from 'alt-client';
import * as native from 'natives';

var drawCursor = false;
var contextMenu;

class ContextMenu {
    constructor(serverEventTrigger, pos, itemHeight, itemWidth, backgroundColor, textColor, positionFrozen) {
        this.show = false;
        this.pos = pos;
        this.itemHeight = itemHeight;
        this.itemWidth = itemWidth;
        this.items = [];
        this.serverEventTrigger = serverEventTrigger;
        this.backgroundColor = backgroundColor;
        this.textColor = textColor;
        this.positionFrozen = positionFrozen;
        contextMenu = this;
        alt.log('Text Color: ' + textColor);
    }

    AppendItem(text, event) {
        this.items.push({ text: text, event: event });
    }

    ShowMenu(state) {
        alt.setCamFrozen(this.positionFrozen);
        this.show = state;
        this.isContextOpen = true;
    }

    Draw() {
        if (!this.show)
            return;

        native.setMouseCursorActiveThisFrame();

        var screenPos = native.getScreenCoordFromWorldCoord(this.pos.X, this.pos.Y, this.pos.Z, undefined, undefined);

        if (!screenPos[0])
            return;

        if (this.positionFrozen) {
            native.disableAllControlActions(0);
            native.disableAllControlActions(1);
        }

        for (var i = 0; i < this.items.length; i++) {
            let lineHeight = native.getTextScaleHeight(0.5, 4);
            let lineFourth = lineHeight / 4;
            let actualHeight = (lineFourth + lineHeight);

            var hovered = this.isHovered(screenPos[1], screenPos[2] + (i * actualHeight), this.itemWidth, actualHeight);

            if (hovered) {
                this.isPressed(i);
                this.drawRectangle(screenPos[1], screenPos[2] + (i * actualHeight), this.itemWidth, actualHeight, this.backgroundColor.R, this.backgroundColor.G, this.backgroundColor.B, 100);
                this.drawContextText(this.items[i].text, screenPos[1], screenPos[2] + (i * actualHeight), 0.5, this.textColor.R, this.textColor.G, this.textColor.B, 255, 4, 0, false, true, lineHeight);
            } else {
                this.drawRectangle(screenPos[1], screenPos[2] + (i * actualHeight), this.itemWidth, actualHeight, this.backgroundColor.R, this.backgroundColor.G, this.backgroundColor.B, 200);
                this.drawContextText(this.items[i].text, screenPos[1], screenPos[2] + (i * actualHeight), 0.5, this.textColor.R, this.textColor.G, this.textColor.B, 200, 4, 0, false, true, lineHeight);
            }
        }
    }

    isHovered(xPos, yPos, width, height) {
        var cursorPos = GetMousePOS();

        if (cursorPos.x < xPos - (width / 2))
            return false;

        if (cursorPos.x > xPos + (width / 2))
            return false;

        if (cursorPos.y < yPos - (height / 2))
            return false;

        if (cursorPos.y > yPos + (height / 2))
            return false;

        return true;
    }

    isPressed(e) {
        if (!native.isDisabledControlJustPressed(0, 25))
            return;

        this.isContextOpen = false;
        contextMenu = undefined;
        alt.setCamFrozen(false);
        alt.emitServer('contextMenuPressed', this.serverEventTrigger, this.items[e].event);
    }

    drawRectangle(xPos, yPos, width, height, r, g, b, alpha) {
        native.drawRect(xPos, yPos, width, height, r, g, b, alpha);
    }

    drawContextText(text, xPos, yPos, scale, r, g, b, alpha, font, justify, shadow, outline, lineHeight) {
        native.setTextScale(1.0, scale);
        native.setTextFont(font);
        native.setTextColour(r, g, b, alpha);
        native.setTextJustification(justify);

        if (shadow)
            native.setTextDropshadow(0, 0, 0, 0, 255);

        if (outline)
            native.setTextOutline();

        native.beginTextCommandDisplayText('STRING');
        native.addTextComponentSubstringPlayerName(text);
        native.endTextCommandDisplayText(xPos, yPos - (lineHeight / 2));
    }
}

export function CreateContextMenu(pos, itemHeight, itemWidth) {
    new ContextMenu(pos, itemHeight, itemWidth);
}

export function AppendContextMenu(item, eventName) {
    if (contextMenu === undefined) {
        alt.log('====> Context Menu is UNDEFINED.');
        return;
    }

    contextMenu.AppendItem(item, eventName);
}

export function ShowContextMenu(state) {
    contextMenu.ShowMenu(state);
}

alt.onServer('createContextMenu', loadContextMenu);
alt.onServer('closeContextMenu', closeContextMenu);

function closeContextMenu() {
    contextMenu.ShowMenu(false);
    contextMenu = undefined;
}

function loadContextMenu(json) {
    var contextMenuJson = JSON.parse(json);

    var eventTrigger = contextMenuJson.EventTrigger;

    var pos = contextMenuJson.Pos;

    var itemHeight = contextMenuJson.ItemHeight;

    var itemWidth = contextMenuJson.ItemWidth;

    var menuItems = contextMenuJson.MenuItems;

    var backgroundColor = contextMenuJson.BackgroundColor;

    var textColor = contextMenuJson.TextColor;

    alt.log(textColor);

    var positionFrozen = contextMenuJson.FreezePosition;

    contextMenu = new ContextMenu(eventTrigger, pos, itemHeight, itemWidth, backgroundColor, textColor, positionFrozen);
    for (let index = 0; index < menuItems.length; index++) {
        const menuItem = menuItems[index];
        contextMenu.AppendItem(menuItem, menuItem);
    }
    contextMenu.ShowMenu(true);
}

alt.everyTick(() => {
    if (contextMenu !== undefined && contextMenu.show)
        contextMenu.Draw();
});

export function GetMousePOS() {
    var x = native.getControlNormal(0, 239);
    var y = native.getControlNormal(0, 240);
    return { x: x, y: y };
}