setInterval(() => {
  let d = new Date();
  let timeDiv = document.getElementById('time');

  timeDiv.innerHTML = ` ${d.getUTCHours().toString().padStart(2, '0')}:${d.getUTCMinutes().toString().padStart(2, '0')}`
}, 1000);

function hideAppIcons(){
    let appIcons = document.getElementsByClassName('appIcon');
    for (let index = 0; index < appIcons.length; index++) {
        const element = appIcons[index];
        element.classList.add('transparent');
    }

}

function phoneIconClick(){
    hideAppIcons();

    let phonePage = document.getElementsByClassName('phonePage');
    for (let index = 0; index < phonePage.length; index++) {
        const element = phonePage[index];
        element.classList.remove('transparent');
    }
}
function smsIconClick(){
    hideAppIcons();
}

function homeIconClick(){

    let pages = document.getElementsByClassName('page');
    for (let index = 0; index < pages.length; index++) {
        const element = pages[index];
        element.classList.add('transparent');
    }

    let appIcons = document.getElementsByClassName('appIcon');
    for (let index = 0; index < appIcons.length; index++) {
        const element = appIcons[index];
        element.classList.remove('transparent');
    }
}

function phoneDialNumber(number){
    console.log("hello");

}