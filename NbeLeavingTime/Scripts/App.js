$(document).ready(function () {
    var socketOpen = false;
    $("#btnPost").attr('disabled', 'disabled');
    var nbeSR = $.connection.nbeHub;
    var connectRetryCount = 0;
    var HubInitialising;

    UseCookie();

    $.connection.hub.start(function () {
        State_ReadyToPost();
        socketOpen = true;
        connectRetryCount = 0;
        HubInitialising = true;
        nbeSR.server.getAllPosts();
    });

    $.connection.hub.disconnected(function () {
        socketOpen = false;
        connectRetryCount++;
        
        setTimeout(function () {
           
            State_PostNotReady("Attempting Reconnect: Attempt:" + connectRetryCount);
            if (connectRetryCount>3)
                alert('Having problems connecting. I suggest refreshing your browser, Mark');
            $.connection.hub.start();
        }, 3000); 
    });

    nbeSR.client.newPost = function (posts) {
        var allPosts = JSON.parse(posts);
        $("#posts").html("");
               
        var i = 0;
        for (var p in allPosts) {
            var person = $("<span/>").html(allPosts[p].Person).text();
            var message = $("<span/>").html(allPosts[p].Message).text();
            if (allPosts[p].Person == $("#usr").val())
                $("#postTemplate").clone().attr('id', 'ptc' + i).attr('class', 'panel panel-danger').appendTo("#posts");
            else
                $("#postTemplate").clone().attr('id', 'ptc' + i).appendTo("#posts");
            if (allPosts[p].FormattedDate == undefined)
                $('#ptc' + i).children("#person").html(person + " &nbsp;&nbsp;&nbsp;&nbsp;(" + allPosts[p].PostTime.substring(0, 16) + ")");
            else
                $('#ptc' + i).children("#person").html(person + " &nbsp;&nbsp;&nbsp;&nbsp;(" + allPosts[p].FormattedDate + ")");
            $('#ptc' + i).children("#message").html(message);
            i++;
        }

        if(navigator.userAgent.toLowerCase().indexOf('chrome') > -1 && !HubInitialising && $("#chkMessages").prop("checked"))
        {
            alert('new message has been posted on NBE');
        }
        HubInitialising = false;
    };

    nbeSR.client.postSuccessful = function () {
        State_ReadyToPost();
    };

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
            alert('This only works for Google Chrome on a desktop computer. If you tick the box then leave the browser running in the background you should get a visual cue when new posts are added. There\'s no point to ticking it on a mobile phone or tablet as browsers go to sleep in those environments. Try it to see if it works for you, if not, untick the box.  \n\n Mark.');
        }
    });



        function PostMessage() {
            if (socketOpen && $("#usr").val() != "" && $("#comment").val() != "") {
                State_PostNotReady("Checking connection, trying to add your post.");
                var postItem = {};
                postItem.person = $("#usr").val();
                postItem.message = $("#comment").val();
                nbeSR.server.addPost(JSON.stringify(postItem));
                $("#comment").val("");
            }
        }

        function State_ReadyToPost() {
            $("#message").attr("class", "greenmessage");
            $("#message").html("Connected, checking for new messages");
            $("#btnPost").removeAttr('disabled');
            $("#btnPost").html('Post');
        }

        function State_PostNotReady(message) {
            $("#message").attr("class", "redmessage");
            $("#message").html(message);
            $("#btnPost").attr('disabled', 'disabled');
        }

        function UseCookie() {
            $("#usr")[0].value = getCookie("MyCookie");
            if(getCookie("nc") == "true")
                $("#chkMessages").prop("checked",true);
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


