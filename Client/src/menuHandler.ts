import * as alt from 'alt-client';
import * as native from 'natives';
import * as keyHandler from './keyHandler';
import * as sirenHandler from './vehicle/newSirenHandler';
import NativeUI, * as NativeUi from './NativeUi/NativeUi';

var ui: NativeUi.Menu = null;

alt.onServer('CreateNativeMenu', CreateNativeMenu);

function CreateNativeMenu(nativeJson: string) {
    var menuInfo = JSON.parse(nativeJson);

    if (ui !== null) {
        ui.Clear();
        ui.Close();
        ui = null;
        keyHandler.SetNativeUiState(false);
        sirenHandler.SetNativeUiState(false);
    }

    ui = new NativeUi.Menu(menuInfo.Title, menuInfo.SubTitle, new NativeUi.Point(50, 50));

    var menuItems = menuInfo.MenuItems;
    if (menuItems !== null) {
        menuItems.forEach((menuItem:any) => {
            ui.AddItem(new NativeUi.UIMenuItem(
                menuItem.Title,
                menuItem.Description
            ));
        });
    }

    var listMenuItems = menuInfo.ListMenuItems;
    if (listMenuItems !== null) {
        listMenuItems.forEach((listItem:any) => {
            ui.AddItem(new NativeUi.UIMenuListItem(
                listItem.Title,
                '',
                new NativeUi.ItemsCollection(listItem.StringList)
            ));
        });
    }

    var checkedMenuItems = menuInfo.CheckedMenuItems;
    if (checkedMenuItems !== null) {
        checkedMenuItems.forEach((checkedItem:any) => {
            ui.AddItem(new NativeUi.UIMenuCheckboxItem(checkedItem.Title, checkedItem.Checked, checkedItem.Description));
        });
    }

    var colorMenuItems = menuInfo.NativeColorItems;
    if (colorMenuItems !== null) {
        colorMenuItems.forEach((colorItem:any) => {
            alt.log('Adding ' + colorItem.Title);
            var item = new NativeUi.UIMenuItem(colorItem.Title, colorItem.Description);
            item.BackColor = new NativeUi.Color(colorItem.ColorR, colorItem.ColorG, colorItem.ColorB, colorItem.ColorA);
            item.HighlightedBackColor = new NativeUi.Color(colorItem.HighlightColorR, colorItem.HighlightColorG, colorItem.HighlightColorB, colorItem.HighlightColorA);
            item.ForeColor = new NativeUi.Color(255, 255, 255, 255);
            item.HighlightedForeColor = new NativeUi.Color(255, 255, 255, 255);
            ui.AddItem(item);
        });
    }

    ui.ItemSelect.on((selectedItem: NativeUi.UIMenuListItem | NativeUi.UIMenuSliderItem | NativeUi.UIMenuCheckboxItem | NativeUi.UIMenuAutoListItem, selectedItemIndex: number) => {
        ui.Clear();
        ui.Close();
        ui = null;

        if (menuInfo.PassIndex) {
            alt.emitServer('NativeMenuCallback', menuInfo.ServerTrigger, selectedItem.Text, selectedItemIndex);
        }
        else {
            alt.emitServer('NativeMenuCallback', menuInfo.ServerTrigger, selectedItem.Text, 0);
        }

        alt.log("[ItemSelect] " + selectedItemIndex, selectedItem.Text);
    });

    ui.ListChange.on((item: NativeUi.UIMenuListItem, newListItemIndex: number) => {
        var text = item.Text;
        var value = item.SelectedItem.DisplayText;

        alt.emitServer('NativeMenuListChange', menuInfo.ListTrigger, text, value);
        alt.log("[ListChange] " + newListItemIndex, item.Text, value);
    });

    ui.IndexChange.on((newIndex: number) => {
        if (menuInfo.ItemChangeTrigger == null) return;

        var menuItem = menuItems[newIndex];
        alt.emitServer('NativeMenuIndexChange', menuInfo.ItemChangeTrigger, newIndex, menuItem.Title);
        alt.log("[IndexChange] " + "Current Selection: " + newIndex);
    });

    ui.MenuClose.on(() => {
        
        keyHandler.SetNativeUiState(false);
        sirenHandler.SetNativeUiState(false);
        ui = null;
    });

    
    keyHandler.SetNativeUiState(true);
    sirenHandler.SetNativeUiState(true);

    ui.RefreshIndex();
    ui.Open();

}

alt.onServer('CloseNativeMenu', () => {
    if (ui === null)
        return;
    ui.Clear();
    ui.Close();
    ui = null;
    keyHandler.SetNativeUiState(false);
    sirenHandler.SetNativeUiState(false);
});