import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '2s', target: 0 },
        { duration: '1s', target: 10 },
        { duration: '10s', target: 10 },
        { duration: '3s', target: 0 },
    ]
};

const url = 'http://79.76.42.159:8080/GetAvailableTimeSlots';

const payload = JSON.stringify({
    date: '2025-12-12',
    partySize: 2,
});

const params = {
    headers: {
        'Content-Type': 'application/json',
    },
};

export default function () {
    const res = http.post(url, payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'response not empty': (r) => r.body && r.body.length > 10,
    });

    sleep(1);
}