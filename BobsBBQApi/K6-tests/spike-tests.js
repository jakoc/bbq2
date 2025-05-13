import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '3s', target: 2000 },
        { duration: '3s', target: 2000 },
        { duration: '3s', target: 0 },
    ]
};

let payload = JSON.stringify({
    date: "2025-12-12",
    partySize: 2
});

let params = {
    headers: {
        'Content-Type': 'application/json',
    },
};

export default () => {
    let url = `http://79.76.101.254:5000/GetAvailableTimeSlots`;
    let res = http.post(url, payload, params);
    check(res, {
        'is status 200': (r) => r.status === 200,
        'response body contains slots': (r) => r.body.includes('Available time slots'),
    });
    sleep(1);
};