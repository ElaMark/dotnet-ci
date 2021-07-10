$(document).ready(function () {
    var socketOpen = false;
    var DonkeySR;
    $("#btnPost").attr('disabled', 'disabled');
 

    DonkeySR = new signalR.HubConnectionBuilder()
            .withUrl("/Donkeyhub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

    function start(){      
            DonkeySR.start().then(function () {
                State_ReadyToPost();
                socketOpen = true;
                DonkeySR.invoke("GetAllThoughts");
            });
    }

    start();
    UseCookie();

    DonkeySR.onreconnected((connectionId) => {
       State_ReadyToPost();
    });     

    DonkeySR.onreconnecting((connectionId) => {
        State_PostNotReady("Connecting");
    });  

    DonkeySR.onclose(function () {
        socketOpen = false;
        State_PostNotReady("Connecting");
    });


    DonkeySR.on("newPost", function (thoughts) {
        var allThoughts = JSON.parse(thoughts);
        $("#thoughts").html("");
               
        var i = 0;
        for (var p in allThoughts) {
            var person = $("<span/>").html(allThoughts[p].Person).text();
            var message = $("<span/>").html(allThoughts[p].Message).text();
            if (allThoughts[p].Person == $("#usr").val())
                $("#postTemplate").clone().attr('id', 'ptc' + i).attr('class', 'panel panel-danger').appendTo("#thoughts");
            else
                $("#postTemplate").clone().attr('id', 'ptc' + i).appendTo("#thoughts");
            if (allThoughts[p].FormattedDate == undefined)
                $('#ptc' + i).children("#person").html(person + " &nbsp;&nbsp;&nbsp;&nbsp;(" + allThoughts[p].PostTime.substring(0, 16) + ")");
            else
                $('#ptc' + i).children("#person").html(person + " &nbsp;&nbsp;&nbsp;&nbsp;(" + allThoughts[p].FormattedDate + ")");
            $('#ptc' + i).children("#message").html(message);
            i++;
        }
    });

    DonkeySR.on("postSuccessful", function () {
        State_ReadyToPost();
    });

    $("#btnRefresh").click(function(e){
        location.reload();
    });

    $("#btnPost").click(function(e){
        PostMessage();
    });


    $("textarea").focus(function () { $(this).select(); });

    $("#usr").keyup(function (e) {
        var exDate = new Date();
        exDate.setDate(exDate.getDate() + 3000);
        var cookieValue = escape($("#usr")[0].value) + "; expires=" + exDate.toUTCString();
        document.cookie = "MyCookie=" + cookieValue;
    });

    $("#chkMessages").change(function (e) {
        var exDate = new Date();
        exDate.setDate(exDate.getDate() + 3000);
        var cookieValue = escape($("#chkMessages").prop("checked")) + "; expires=" + exDate.toUTCString();
        document.cookie = "nc=" + cookieValue;
    });

    $("#chkMessages").click(function (e) {
        if ($("#chkMessages").prop("checked")) {
            alert('This only works for Google Chrome on a desktop computer. If you tick the box then leave the browser running in the background you should get a visual cue when new thoughts are added. There\'s no point to ticking it on a mobile phone or tablet as browsers go to sleep in those environments. Try it to see if it works for you, if not, untick the box.  \n\n Mark.');
        }
    });

    setInterval(function(){
        if(DonkeySR.connectionState != "Connected"){
            State_PostNotReady("Connecting");
            start();
        }
    },2000)



    function PostMessage() {
        if (socketOpen && $("#usr").val() != "" && $("#comment").val() != "") {
            State_PostNotReady("Connecting");
            var postItem = {};
            postItem.person = $("#usr").val();
            postItem.message = $("#comment").val();
            postItem.token = $("#token").val();
            DonkeySR.invoke("AddPost", JSON.stringify(postItem));
            $("#comment").val("");
        }
    }

    function State_ReadyToPost() {
 
        $("#message").attr("class", "greenmessage");
        $("#message").html("Status: Connected" );
        $("#btnPost").removeAttr('disabled');
        $("#btnRefresh").removeAttr('disabled');
        $("#btnPost").html('Post message');
        $("#btnPost").show();
        $("#btnRefresh").show();
    }


    function State_PostNotReady(message) {
        $("#message").attr("class", "redmessage");
        $("#message").html("Status: " + message);
        $("#btnPost").attr('disabled', 'disabled');
        $("#btnRefresh").attr('disabled', 'disabled');
      
    }

    function UseCookie() {
        if(!$("#usr").prop('disabled')){
            $("#usr")[0].value = getCookie("MyCookie");
            if(getCookie("nc") == "true")
                $("#chkMessages").prop("checked",true);
        }

    }

    function getCookie(cname) {
        var name = cname + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1);
            if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
        }
        return "";
    }
});


