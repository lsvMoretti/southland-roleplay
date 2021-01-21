
var button = document.getElementById('mainButton');

var openForm = function() {
	button.className = 'active';
};

var checkInput = function(input) {
	if (input.value.length > 0) {
		input.className = 'active';
	} else {
		input.className = '';
	}
};

var closeForm = function() {
	button.className = '';
};

document.addEventListener("keyup", function(e) {
	if (e.keyCode == 27 || e.keyCode == 13) {
		closeForm();
	}
});

function loaded(){
	alt.emit('LoginScreenLoaded');
}

function loginAccount(){
    var user = document.getElementById("username").value;
    var password = document.getElementById("password").value;

    alt.emit('loginRequest', user, password);
}

if('alt' in window){
	alt.on('hideLoginScreen', closeLoginScreen);
}

function closeLoginScreen(){
	var bodyClass = document.getElementById("bodyClass").classList.add("hideme");
}