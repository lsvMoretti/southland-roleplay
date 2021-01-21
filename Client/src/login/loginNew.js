if('alt' in window){
	alt.on('hideLoginScreen', closeLoginScreen);
}

function loaded(){
	alt.emit('LoginScreenLoaded');
}

function loginAccount(){
    var user = document.getElementById("Username").value;
    var password = document.getElementById("Password").value;

    alt.emit('loginRequest', user, password);
}
