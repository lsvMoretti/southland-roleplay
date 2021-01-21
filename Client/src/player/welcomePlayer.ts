import * as alt from 'alt-client';
import * as native from 'natives';
import * as notification from '../notification';
import {startAnimation} from "../animation";

// Ped
let ped:number = undefined;

// Spawns the ped and sets into an anim
alt.onServer('WelcomePed:SpawnPed', spawnPed);
function spawnPed(pos:alt.Vector3, rot:alt.Vector3){
    alt.loadModel(-199280229);
    
    ped = native.createPed(4,  -199280229, pos.x, pos.y, pos.z, rot.z, false, true );
    
    // Configure Ped
    native.setPedCanBeTargetted(ped, false);
    native.freezeEntityPosition(ped, true);
    native.setEntityInvincible(ped, true);
    native.setPedDiesWhenInjured(ped, false);
    
    startAnimation("amb@code_human_wander_texting@male@base", "static", -1, 1, ped);
}

alt.onServer('WelcomePed:ShowInitMessage', showInitMessage);
async function showInitMessage(pos:alt.Vector3){

    native.setNewWaypoint(pos.x, pos.y);

    await notification.ShowIconNotification(`Hey! I'm Oscar, nice to meet you! Head over to me and type /welcome for a quick guide. I'm here to help you get started!`, `Hey!`, '', 'CHAR_OSCAR', undefined, true);

}

// Shows a welcome message if the player hasn't interacted before
alt.onServer('WelcomePed:ShowWelcomeMessage', showWelcomeMessage);
async function showWelcomeMessage(){
    
    await notification.ShowIconNotification(`Hey! It looks like your new here! Head to the vehicle rental and come back here!`, `Vehicle Rental`, '', 'CHAR_OSCAR', undefined, true);
    
    native.setNewWaypoint(100.37802, -1073.3011);
}

alt.onServer('WelcomePed:OnRentVehicle', onRentVehicle);
async function onRentVehicle() {

    await notification.ShowIconNotification(`Looks like you've got a vehicle. Lets head to a nearby bank!`, `Vehicle Rental`, 'Next to the Bank!', 'CHAR_OSCAR', undefined, true);

    native.setNewWaypoint(149.9203, -1040.654)
    
    await alt.setTimeout(() => {
        notification.ShowIconNotification(`At the bank, type /bank. Make a debit account!`, `Bank`, '', 'CHAR_OSCAR', undefined, true);
    }, 10000);

}

alt.onServer('WelcomePed:OnBankCommand', onBankCommand);
async function onBankCommand() {

    await notification.ShowIconNotification(`Make sure the account is setup to receive paychecks! This is done in /bank`, `Bank`, '', 'CHAR_OSCAR', undefined, true);

    await alt.setTimeout(() => {
        notification.ShowIconNotification(`Looks like you've not got a phone yet. Head to the nearest WiFi icon and buy one!`, `Phone`, '', 'CHAR_OSCAR', undefined, true);
    }, 10000);
}

alt.onServer('WelcomePed:OnBuyCommand', onBuyCommand);
async function onBuyCommand(type:string, dmvPos:alt.Vector3){
    if(type === "phone"){
        await notification.ShowIconNotification(`Looks like you got yourself a phone! Now, lets head to a clothing store. These are a clothing icon.`, `Phone`, '', 'CHAR_OSCAR', undefined, true);
        return;
    }
    
    if(type === "clothing"){
        await notification.ShowIconNotification(`Now, we can head to the DMV to get our driving license!`, `DMV`, '', 'CHAR_OSCAR', undefined, true);
        native.setNewWaypoint(dmvPos.x, dmvPos.y);
        return;
    }
}

alt.onServer('WelcomePed:SendToDmv', onSendToDmv);
async function onSendToDmv(dmvPos:alt.Vector3){
    await notification.ShowIconNotification(`Looks like you need to head to the DMV. Head there now.`, `DMV`, '', 'CHAR_OSCAR', undefined, true);
    native.setNewWaypoint(dmvPos.x, dmvPos.y);
    return;
}

alt.onServer('WelcomePed:OnDmvFinish', onDmvFinish);
async function onDmvFinish(commandUsed:boolean) {

    if(!commandUsed){
        await notification.ShowIconNotification(`Well, that looks like your nearly set to go! You can head back to me to look for a job!`, `Jobs`, '', 'CHAR_OSCAR', undefined, true);
        return;
    }
    await notification.ShowIconNotification(`I know a few guys looking for work around the city. What job takes your fancy?`, `Jobs`, "Select a job from the menu", 'CHAR_OSCAR', undefined, true);


}

alt.onServer('WelcomePed:SendMessage', onSendMessage);
async function onSendMessage(title:string,subTitle:string, message:string) {
    await notification.ShowIconNotification(message, title, subTitle, 'CHAR_OSCAR', undefined, true);

}