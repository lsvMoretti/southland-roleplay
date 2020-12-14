// callNumber.html
function CallNumber() {
    var number = document.getElementById('numberInput').value;
    alt.emit('phone:CallNumber', number);
}

// smsNumber.html
function SMSNumber() {
    var number = document.getElementById('numberInput').value;
    var message = document.getElementById('textInput').value;
    alt.emit('phone:SmsNumber', number, message);
}

//smsContact.html
function SMSContact() {
    var message = document.getElementById('textInput').value;
    alt.emit('phone:SmsContact', message);
}

//addContact.html
function AddContact() {
    var name = document.getElementById('nameInput').value;
    var number = document.getElementById('numberInput').value;
    alt.emit('phone:addContact', name, number);
}