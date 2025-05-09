import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '3s', target: 2000 }, //ramp up
        { duration: '3s', target: 2000 }, //stable
        { duration: '3s', target: 0 }, //ramp down
    ]
}
let date = '2025-12-12';
let partySize = 2;
export default () =>{
    let url = `http://79.76.101.254:5000/GetAvailableTimeSlots?date=${date}&partySize=${partySize}`;
    let res = http.get(url);
    check(res, {
        'is status 200': (r) => r.status === 200,
        'response body contains slots': (r) => r.body.includes('Available time slots'), // Adjust as needed based on your response structure
    });

    sleep(1)
};