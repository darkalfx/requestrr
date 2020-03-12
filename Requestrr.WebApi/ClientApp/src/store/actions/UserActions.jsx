export const HAS_REGISTERED = "auth:has_registered";
export const LOGGED_IN = "auth:logged_in";
export const LOGGED_OUT = "auth:logged_out";

export function setHasRegistered(hasRegistered) {
    return {
        type: HAS_REGISTERED,
        payload: hasRegistered
    };
};

export function setLoggedIn(token) {
    return {
        type: LOGGED_IN,
        payload: token
    };
};

export function setLoggedOut() {
    return {
        type: LOGGED_OUT
    };
};

export function hasRegistered() {
    return dispatch => {
        return fetch("/api/auth/registration", {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
            }
        })
            .then(data => data.json())
            .then(data => {
                dispatch(setHasRegistered(data.hasRegistered));
            });
    };
};

export function validateLogin() {
    return (dispatch, getState) => {
        const state = getState();
        let token = null;

        if (state.user != null && typeof (state.user.token) === "undefined") {
            token = window.localStorage.getItem("token");
        }
        else {
            token = state.user.token;
        }

        return fetch("/api/auth/validate", {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        })
            .then(data => {
                if (data.status === 200) {
                    dispatch(setLoggedIn(token));
                }
                else {
                    dispatch(setLoggedOut());
                }
            });
    };
};

export function register(registrationModel) {
    return dispatch => {
        return fetch("/api/auth/register", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
            },
            body: JSON.stringify({
                "Username": registrationModel.username,
                "Password": registrationModel.password,
                "PasswordConfirmation": registrationModel.passwordConfirmation,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.token) {
                    if (registrationModel.rememberMe === true) {
                        window.localStorage.setItem("token", data.token);
                    }
                    dispatch(setLoggedIn(data.token));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};

export function login(loginModel) {
    return dispatch => {
        return fetch("/api/auth/login", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
            },
            body: JSON.stringify({
                "Username": loginModel.username,
                "Password": loginModel.password,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.token) {
                    if (loginModel.rememberMe === true) {
                        window.localStorage.setItem("token", data.token);
                    }

                    dispatch(setLoggedIn(data.token));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    };
};

export function logout() {
    return dispatch => {
        window.localStorage.removeItem("token");
        dispatch(setLoggedOut());
    };
};

export function changePassword(model) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("/api/auth/password", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'ExistingPassword': model.password,
                'NewPassword': model.newPassword,
                'NewPasswordConfirmation': model.newPasswordConfirmation,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    };
};