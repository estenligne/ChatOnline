
const WebAPIBaseURL = "http://localhost:44363";


export function _fetch(user, path, method, body) {
    const init = { headers: {} };

    if (method) init.method = method;
    if (body) init.body = JSON.stringify(body);

    if (body) init.headers['Content-Type'] = 'application/json';
    if (user) init.headers['Authorization'] = user.authorization;

    console.debug(path, init);
    return fetch(WebAPIBaseURL + path, init);
}


export function trimObject(obj) {
    for (const [key, value] of Object.entries(obj)) {
        if (!value)
            delete obj[key];
    }
    return obj;
}


export function getFormValues(inputIds) {
    const values = {};

    for (let i = 0; i < inputIds.length; i++) {

        const id = inputIds[i];
        const element = document.getElementById(id);

        if (!element.reportValidity()) // if input not valid
            return null; // then report and return null

        else values[id] = element.value;
    }
    return values;
}


export function getFileURL(fileName) {
    return fileName ? (WebAPIBaseURL + "/api/File/Download?fileName=" + fileName) : null;
}


export function dateToLocal(date) {
    if (!date) return null;
    const s = new Date(date).toString();
    const i = s.indexOf(" GMT");
    return i ? s.substr(0, i) : date.toUTCString();
}
