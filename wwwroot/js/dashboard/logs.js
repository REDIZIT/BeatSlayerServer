const connection = new signalR.HubConnectionBuilder()
    .withUrl("/DashboadHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();

        connection.on("OnPlayerConnected", (connectionId, ip) => {
            var item = getConnectedElement(connectionId, ip);
            document.getElementById("logs-container").prepend(item);
        });
        connection.on("OnPlayerDisconnected", (connectionId, ip) => {
            var item = getDisconnectedElement(connectionId, ip);
            document.getElementById("logs-container").prepend(item);
        }); 
        connection.on("OnPlayerLoggedIn", (connectionId, ip, nick) => {
            var item = getLoggedElement(connectionId, ip, nick);
            document.getElementById("logs-container").prepend(item);
        });
        connection.on("OnOnlineChange", (online, logged) => {
            document.getElementById("online").innerText = `Online ${online} (Logged in ${logged})`;
        });

        connection.on("OnMapPlayed", (trackname, mapper, player, score, RP, accuracy) => {
            var item = getMapPlayedElement(trackname, mapper, player, score, RP, accuracy);
            document.getElementById("maps-container").prepend(item);
        });

    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};

connection.onclose(async () => {
    await start();
});

function getLogElement(message) {
    var item = document.createElement("div");
    var span = document.createElement("span");
    span.textContent = `${connectionId}`;

    item.appendChild(span);

    return item;
}


function getConnectedElement(connectionId, ip) {
    var item = document.createElement("div");
    item.className = "item connected";

    //var span = document.createElement("span");
    item.innerHTML = `<span style="color: #666">${connectionId}</span> connected<span class="ip">${ip}</span>`;

    //item.appendChild(span);

    return item;
}
function getDisconnectedElement(connectionId, ip) {
    var item = document.createElement("div");
    item.className = "item disconnected";

    item.innerHTML = `<span style="color: #666">${connectionId}</span> disconnected<span class="ip">${ip}</span>`;

    return item;
}
function getLoggedElement(connectionId, ip, nick) {
    var item = document.createElement("div");
    item.className = "item logged";

    item.innerHTML = `<span style="color: #666">${connectionId}</span> logged in as <b><span style="color: #00dcff">${nick}</span></b><span class="ip">${ip}</span>`;

    return item;
}


function getMapPlayedElement(trackname, mapper, player, score, RP, accuracy) {

    var item = document.createElement("div");
    item.className = "item map";


    var coverdiv = document.createElement("div");
    coverdiv.className = "coverdiv";
    item.appendChild(coverdiv);

    var textdiv = document.createElement("div");
    textdiv.className = "textdiv";
    item.appendChild(textdiv);


    var img = document.createElement("img");
    img.src = `http://www.bsserver.tk/Maps/GetCoverPicture?trackname=${trackname}&mapper=${mapper}`;
    coverdiv.appendChild(img);


    var tracknameDiv = document.createElement("div");
    textdiv.appendChild(tracknameDiv);

    var playerDiv = document.createElement("div");
    textdiv.appendChild(playerDiv);


    var tracknameSpan = document.createElement("span");
    tracknameSpan.innerText = trackname + " by " + mapper;
    tracknameDiv.appendChild(tracknameSpan);

    var nickSpan = document.createElement("span");
    nickSpan.innerHTML = `${player} <span style="color: rgb(255, 128, 0)">${score}</span> <span style="color: rgb(0, 128, 255)">${RP}</span> <span style="color: rgb(190, 190, 190)">${accuracy}%</span>`
    playerDiv.appendChild(nickSpan);



    return item;

    /*

<div id="maps-container" class="log-window">
    <div class="item map">
        <div class="coverdiv">
            @*<img src="http://www.bsserver.tk/Maps/GetCoverPicture?trackname=Apashe-Lacrimosa&mapper=REDIZIT">*@
            <img src="http://www.bsserver.tk/Account/GetAvatarAsPicture?nick=REDIZIT">
        </div>
        <div class="textdiv">
            <div>
                <span>This is author - this is name</span>
            </div>
            <div>
                <span>This is player</span>
            </div>
        </div>
    </div>
</div>

     * */

}

// Start the connection.
start();