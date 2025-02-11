portrait_menu = document.querySelector(".portrait-menu");
portrait_menu.onclick = function(){
    nav_bar = document.querySelector(".nav-bar");
    notif_box = document.querySelector(".notif-container");
    notif_box = notif_box.classList.remove("active");
    nav_bar.classList.toggle("active");
}

notif_btn = document.querySelector(".notifications");
notif_btn.onclick = function(){
    notif_box = document.querySelector(".notif-container");
    notif_box.classList.toggle("active");
}

const notification = document.getElementById("notifications");

function get_notification() {
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {
            let announcement_list = JSON.parse(xhttp.response);
            console.log(announcement_list);
        }
    };
    xhttp.open("GET", "/Announcement/GetAnnouncement", true);
    xhttp.send();
}