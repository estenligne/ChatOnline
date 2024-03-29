
export const AccountBaseURL = process.env.REACT_APP_AccountBaseURL;
export const WebAPIBaseURL = process.env.REACT_APP_WebAPIBaseURL;


/**
 * Make a HTTP request
 * @param {{ account: {}}} user
 * @param {string} url
 * @param {"GET"|"POST"|"PUT"|"DELETE"} method
 * @param {Object} jsonBody
 * @param {FormData} formData
 * @returns {Promise<Response>} Promise of HTTP response
 */
export function _fetch(user, url, method, jsonBody, formData) {
    const init = { headers: {} };

    if (method)
        init.method = method;

    if (jsonBody) {
        init.body = JSON.stringify(jsonBody);
        init.headers['Content-Type'] = 'application/json';
    }

    if (formData)
        init.body = formData;

    if (user)
        init.headers['Authorization'] = user.account.authorization;

    if (url[0] === '/')
        url = WebAPIBaseURL + url;

    return fetch(url, init)
        .then(response => {
            if (user && response.status === 401) {
                window.localStorage.removeItem('userData');
                alert("This session has expired, please reload or refresh the page to sign-in again.");
            }
            return response;
        });
}


/**
 * Delete all fields with false values: false, undefined, null, empty string, 0.
 * @param {{}} obj
 * @returns {{}}
 */
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
    return i > 0 ? s.substring(0, i) : date.toUTCString();
}
