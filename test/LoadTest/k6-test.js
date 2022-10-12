import * as http from 'k6/http';
import {check, sleep} from 'k6';
import {Rate} from 'k6/metrics';
import {URL} from 'https://jslib.k6.io/url/1.0.0/index.js';
import {htmlReport} from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import {textSummary} from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';
export function handleSummary(data) {
    return {
        'result.html': htmlReport(data),
        stdout: textSummary(data, {indent: ' ', enableColors: true}),
    };
}

export const options = {
    vus: 100,
    duration: '30s',
    insecureSkipTLSVerify: true
};

const auth_token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjRjODA2ZThkLTI0NDktNGViNS1hODc4LWI4YTNlODY2ZDgzNCIsIm5hbWUiOiJEaXJjZXUgSnVuaW9yIiwiZW1haWwiOiJkaXJjZXUuc2pyQGdtYWlsLmNvbSIsImVtYWlsLWNvbmZpcm1lZCI6InRydWUiLCJsYW5ndWFnZSI6IjEiLCJ0aGVtZSI6IjIiLCJuYmYiOjE2NjU1MTMzNTYsImV4cCI6MTY2NTU0MjE1NiwiaWF0IjoxNjY1NTEzMzU2LCJpc3MiOiJMb2dpbiJ9.2Hxl9NmoGNIixCvlNL7Tdk_z9MF5R91DZMo2FGZ2sCQ'
const params = {
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${auth_token}`
    },
};

const payload = JSON.stringify({
    id: 1176, 
    PredictIdeia: false
  });

const routes = [
    "https://localhost:7126/alert?Take=7",
    "https://localhost:7126/alert?Take=7&LastAlertId=z6Bz",
    "https://localhost:7126/alert?Take=7&LastAlertId=zOgz"
]
export const errorRate = new Rate('errors');
export default function () {
    routes.forEach(route =>{
        const res = http.get(route, params);
        check(res, {
            'status is 200': (r) => {
                return r.status === 200
            },
        }) || errorRate.add(1);
    });
}