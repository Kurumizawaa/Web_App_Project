self.onmessage = (event) => {
    close_date_list = event.data;
}

close_date_list = [];
function calculate_remaining_time() {
    calculated_list = [];
    close_date_list.forEach(close_date => {
        if (close_date > Date.now()) {
            if (close_date.getFullYear() == new Date().getFullYear() && close_date.getMonth() == new Date().getMonth() && close_date.getDate() == new Date().getDate()) {
                let diff = close_date - Date.now();
                let sec = (diff / 1000) % 60;
                let min = (diff / 60000) % 60;
                let hour = diff / 3600000;
                calculated_list.push(`(${pad(parseInt(hour))}:${pad(parseInt(min))}:${pad(parseInt(sec))} left)`);
            }
            else if (close_date.getFullYear() == new Date().getFullYear() && close_date.getMonth() == new Date().getMonth()) {
                let diff = close_date - Date.now();
                calculated_list.push(`(${new Date(diff).toLocaleString("en-us", {day: "numeric"})} days left)`);
            }
            else if (close_date.getFullYear() == new Date().getFullYear()) {
                calculated_list.push(`(${close_date.getMonth() - new Date().getMonth()} months left)`);
            }
            else {
                calculated_list.push(`(${close_date.getFullYear() - new Date().getFullYear()} years left)`);
            }
        }
        else {
            calculated_list.push("");
        }
    });
    postMessage(calculated_list);
}

function pad(number, size = 2) {
    return String(number).padStart(size, "0");
}

var milisecond_until_next_second = 1000 - new Date().getMilliseconds();

setTimeout(() => {
    setInterval(calculate_remaining_time, 1000)
}, milisecond_until_next_second);