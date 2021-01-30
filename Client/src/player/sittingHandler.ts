import * as alt from 'alt-client';
import * as native from "natives";

var isSitting: boolean = false;
var currentScenario: string = null;
var sittingObject: number;
var lastPos: alt.Vector3;
var objectName: string;
var entityPosition: alt.Vector3;

alt.onServer('sitting:ToggleSitting', toggleSitting);
alt.onServer('sitting:PositionFree', playerPositionFree);

function toggleSitting() {
    alt.log('ToggleSitting');
    let ped = alt.Player.local.scriptID;

    lastPos = alt.Player.local.pos;

    if (isSitting) {
        alt.log('isSitting');
        stopSitting();
        return;
    }

    alt.log('Sittables: ' + sitableObjects.length);

    sitableObjects.forEach(object => {
        var obj = native.getClosestObjectOfType(lastPos.x,
            lastPos.y,
            lastPos.z,
            3.0,
            native.getHashKey(object.propName),
            false,
            true,
            true);

        var objPos = native.getEntityCoords(obj, false);

        var dist = native.getDistanceBetweenCoords(lastPos.x, lastPos.y, lastPos.z, objPos.x, objPos.y, objPos.z, true);
        if (dist != undefined) {
            if (dist < 1.5) {
                propInfoList.push(new propInfo(obj, dist, object.propName));
            }
        }
    });

    alt.log('PropInfoList count: ' + propInfoList.length);

    var closest = propInfoList[0];

    if(closest == null) return;
    
    alt.log('0Closest: ' + propInfoList[0]);
    alt.log('Closest: ' + closest);

    propInfoList.forEach(prop => {
        if (prop.dist < closest.dist) {
            closest = prop;
        }
    });

    var distance: number = closest.dist;
    var object: number = closest.obj;

    alt.log('Distance:' + distance);
    alt.log('Object: ' + object);

    var entityExist = native.doesEntityExist(object);

    var entityPos = native.getEntityCoords(object, false);

    alt.log('EntityPos' + entityPos.x + ',' + entityPos.y + ',' + entityPos.z);

    alt.log('EntityExists: ' + entityExist);
    alt.log('IsSittingx2' + isSitting);

    if (distance < 1.5 && !isSitting) {
        alt.log('Sending IsPlayerFree');
        sittingObject = object;
        objectName = closest.objName;
        entityPosition = new alt.Vector3(entityPos.x, entityPos.y, entityPos.z);
        var pos = new alt.Vector3(entityPos.x, entityPos.y, entityPos.z);
        alt.emitServer('sitting:IsPlayerPositionFree', pos.x, pos.y, pos.z, closest.objName);
        return;
    }
}

alt.onServer('Sitting:RemoveEntityCollision', removeEntityCollision);

function removeEntityCollision(objectName: string, objectPos: alt.Vector3) {
    var object = native.getClosestObjectOfType(objectPos.x, objectPos.y, objectPos.z, 0.5, alt.hash(objectName), false, true, true);
    if (object == undefined) return;

    alt.log('Setting Collision for: ' + object);
    native.setEntityCollision(object, false, false);
}

alt.onServer('Sitting:SetEntityCollision', setEntityCollision);

function setEntityCollision(objectName: string, objectPos: alt.Vector3) {
    var object = native.getClosestObjectOfType(objectPos.x, objectPos.y, objectPos.z, 0.5, alt.hash(objectName), false, true, true);
    if (object == undefined) return;

    alt.log('Setting Collision for: ' + object);
    native.setEntityCollision(object, true, true);
}

function playerPositionFree() {
    alt.log('PositionFree');
    var localPlayer = alt.Player.local.scriptID;

    lastPos = alt.Player.local.pos;
    var entityPos = native.getEntityCoords(sittingObject, false);

    var sittableProp: SitableProp;

    sitableObjects.forEach(object => {
        if (object.propName == objectName) {
            sittableProp = object;
        }
    });

    var newPos: alt.Vector3 = new alt.Vector3(entityPos.x, entityPos.y, entityPos.z + sittableProp.verticalOffset);
    lastPos = alt.Player.local.pos;
    //native.setEntityCoords(localPlayer, entityPos.x, entityPos.y, entityPos.z + sittableProp.verticalOffset, true, false, false, false);
    //native.setEntityHeading(localPlayer, native.getEntityHeading(sittingObject) + 180);
    native.freezeEntityPosition(localPlayer, true);
    isSitting = true;
    currentScenario = sittableProp.scenario;
    alt.emitServer('sitting:SetPlayerPosition', newPos, native.getEntityHeading(sittingObject));
    alt.log('CurrentScenario ' + currentScenario);
    alt.setTimeout(() => {
        alt.log('Moving');

        native.taskStartScenarioInPlace(localPlayer, sittableProp.scenario, 0, true);
    }, 500);
}

function stopSitting() {
    var localPlayer = alt.Player.local.scriptID;
    alt.log('Pos: ' + lastPos.x + ',' + lastPos.y + ',' + lastPos.z);
    native.setEntityCoords(localPlayer, lastPos.x, lastPos.y, lastPos.z, true, false, false, false);
    native.clearPedTasks(localPlayer);
    native.freezeEntityPosition(localPlayer, false);
    alt.log('StopStting');
    alt.emitServer("sitting:ClearSeat", entityPosition.x, entityPosition.y, entityPosition.z, objectName);
    isSitting = false;
    currentScenario = null;
    sittingObject = null;
    lastPos = null;
    objectName = null;
    entityPosition = null;
    propInfoList = new Array();

    // Handle stop scenario here
}

class propInfo {
    obj: number;
    dist: number;
    objName: string;

    constructor(objectHash: number, distance: number, objectName: string) {
        this.obj = objectHash;
        this.dist = distance;
        this.objName = objectName;
    }
}

var propInfoList: propInfo[] = new Array();

class SitableProp {
    propName: string;
    scenario: string;
    verticalOffset: number;
    forwardOffset: number;
    leftOffset: number;

    constructor(propName: string, scenario: string, verticalOffset: number, forwardOffset: number, leftOffset: number) {
        this.propName = propName;
        this.scenario = scenario;
        this.verticalOffset = verticalOffset;
        this.forwardOffset = forwardOffset;
        this.leftOffset = leftOffset;
    }
}

var sitableObjects: SitableProp[] = new Array();

// Bench
sitableObjects.push(new SitableProp("prop_bench_01a", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_01b", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_01c", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_04", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_05", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_06", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_07", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_08", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_09", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_10", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_bench_11", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_fib_3b_bench", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_ld_bench01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_wait_bench_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));

// Chair
sitableObjects.push(new SitableProp("v_club_stagechair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("hei_prop_heist_off_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("hei_prop_hei_skid_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_01a", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_01b", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_04a", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_04b", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_05", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_06", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_07", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_08", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_09", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chair_10", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_chateau_chair_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_clown_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_cs_office_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_direct_chair_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_direct_chair_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_gc_chair02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_off_chair_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_off_chair_03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_off_chair_04", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_off_chair_04b", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_off_chair_04_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_off_chair_05", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_old_deck_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_old_wood_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_rock_chair_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_skid_chair_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_skid_chair_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_skid_chair_03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_sol_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_wheelchair_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_wheelchair_01_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_armchair_01_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_clb_officechair_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_dinechair_01_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_ilev_p_easychair_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_soloffchair_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_yacht_chair_01_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_club_officechair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_corp_bk_chair3", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_corp_cd_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_corp_offchair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_chair02_ped", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_hd_chair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_p_easychair", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ret_gc_chair03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_ld_farm_chair01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_04_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_05_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_06_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_leath_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_01_chr_a", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_01_chr_b", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_02_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_03b_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_table_03_chr", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_torture_ch_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_fh_dineeamesa", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_fh_kitchenstool", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_tort_stool", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
// Seat
sitableObjects.push(new SitableProp("hei_prop_yah_seat_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("hei_prop_yah_seat_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("hei_prop_yah_seat_03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_waiting_seat_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_yacht_seat_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_yacht_seat_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_yacht_seat_03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_hobo_seat_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
// Couch
sitableObjects.push(new SitableProp("prop_rub_couch01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("miss_rub_couch_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_ld_farm_couch01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_ld_farm_couch02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_rub_couch02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_rub_couch03", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_rub_couch04", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
// Sofa
sitableObjects.push(new SitableProp("p_lev_sofa_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_res_sofa_l_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_v_med_p_sofa_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("p_yacht_sofa_01_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_ilev_m_sofa", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_res_tre_sofa_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_tre_sofa_mess_a_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_tre_sofa_mess_b_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("v_tre_sofa_mess_c_s", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_roller_car_01", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));
sitableObjects.push(new SitableProp("prop_roller_car_02", "PROP_HUMAN_SEAT_BENCH", +0.5, -0.0, 0.0));