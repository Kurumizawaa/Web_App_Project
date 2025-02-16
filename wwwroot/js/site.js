﻿function get_notification() {
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {
            let payload = JSON.parse(xhttp.response);
            if (payload.getannouncement_successful == true) {
                console.log(payload.announcement_list);
                insert_notification(payload.announcement_list);
            }
            else {
                console.log(payload.message);
            }
        }
    };
    xhttp.open("GET", "/Announcement/GetAnnouncement", true);
    xhttp.send();
}

function get_header_name(msg) {
    switch (msg.type) {
        case 0:
            return "Accepted into";
        case 1:
            return "Rejected from";
        case 2:
            return "Creator Update for";
        case 3:
            return "General Announcement for";
    }
}

function insert_notification(announcement_list) {
    const notif_list = document.getElementsByClassName("notif-list")[0];
    notif_list.innerHTML = "";
    announcement_list.forEach(announcement => {
        const notif = document.createElement("div");
        let header = get_header_name(announcement);
        if (announcement.message.length > 50) {
            announcement.message = announcement.message.slice(0,50) + "...";
        }
        notif.className = "notif-item";
        notif.classList.add(`announcement-${announcement.type}`);
        notif.onclick = function () {
            window.location.href = `/Post/Post?id=${announcement.postid}`
        };
        notif.innerHTML = `
            <img src="${announcement.picture ?? "../images/jaikere.PNG"}" alt="notif-img" class="notif-img">
            <div class="notif-text-container">
                <h3 class="notif-text-header">${header} ${announcement.title}</h3>
                <p class="notif-text">${announcement.message}</p>
            </div>
        `;
        notif_list.appendChild(notif);
    })
}

function notification_redirect(postid) {
    console.log(postid);
}

//======== Hidden UI elements ========//
const portrait_menu = document.querySelector(".portrait-menu");
portrait_menu.onclick = function(){
    nav_bar = document.querySelector(".nav-bar");
    notif_box = document.querySelector(".notif-container");
    notif_box = notif_box.classList.remove("active");
    nav_bar.classList.toggle("active");
}

const notif_btn = document.querySelector(".notifications");
notif_btn.onclick = function(){
    get_notification();
    notif_box = document.querySelector(".notif-container");
    notif_box.classList.toggle("active");
}

/* snackbar */
function show_snackbar(message, type) {
    const snackbar = document.getElementById("snackbar");
    snackbar.innerText = message;
    if (type == "success") {
        snackbar.style.backgroundColor = "lightgreen";
    }
    else if (type == "info") {
        snackbar.style.backgroundColor = "lightblue";
    }
    else if (type == "error") {
        snackbar.style.backgroundColor = "red";
    }
    snackbar.className = "show";
    setTimeout(function(){ snackbar.classList.remove("show"); }, 3000);
}