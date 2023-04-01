export const GET_SETTINGS = "tvShowsClients:get_settings";
export const SET_DISABLED_CLIENT = "tvShowsClients:set_disabled_client";
export const SET_OMBI_CLIENT = "tvShowsClients:set_ombi_client";
export const SET_OVERSEERR_CLIENT = "tvShowsClients:set_overseerr_client";
export const SET_SONARR_CLIENT = "tvShowsClients:set_sonarr_client";

export function setSettings(settings) {
    return {
        type: GET_SETTINGS,
        payload: settings
    };
};

export function setDisabledClient() {
    return {
        type: SET_DISABLED_CLIENT,
    };
};

export function setOmbiClient(settings) {
    return {
        type: SET_OMBI_CLIENT,
        payload: settings
    };
};

export function setOverseerrClient(settings) {
    return {
        type: SET_OVERSEERR_CLIENT,
        payload: settings
    };
};

export function getSettings() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows", {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            }
        })
            .then(data => data.json())
            .then(data => {
                return dispatch(setSettings(data));
            });
    };
};

export function testOmbiSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/ombi/test", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "Hostname": settings.hostname,
                "BaseUrl": settings.baseUrl,
                "Port": Number(settings.port),
                "ApiKey": settings.apiKey,
                "UseSSL": settings.useSSL,
                "Version": settings.version,
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

export function testOverseerrSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/overseerr/test", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "Hostname": settings.hostname,
                "Port": Number(settings.port),
                "ApiKey": settings.apiKey,
                'DefaultApiUserID': settings.defaultApiUserID,
                "UseSSL": settings.useSSL,
                "Version": settings.version,
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

export function saveDisabledClient() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/disable", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setDisabledClient());
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};

export function saveOmbiClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/ombi", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Hostname': saveModel.ombi.hostname,
                "BaseUrl": saveModel.ombi.baseUrl,
                'Port': Number(saveModel.ombi.port),
                'ApiKey': saveModel.ombi.apiKey,
                'ApiUsername': saveModel.ombi.apiUsername,
                'UseSSL': saveModel.ombi.useSSL,
                'Version': saveModel.ombi.version,
                'Restrictions': saveModel.restrictions,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setOmbiClient({
                        ombi: {
                            hostname: saveModel.ombi.hostname,
                            baseUrl: saveModel.ombi.baseUrl,
                            port: saveModel.ombi.port,
                            apiKey: saveModel.ombi.apiKey,
                            apiUsername: saveModel.ombi.apiUsername,
                            useSSL: saveModel.ombi.useSSL,
                            version: saveModel.ombi.version,
                        },
                        restrictions: saveModel.restrictions
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};

export function saveOverseerrClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/overseerr", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Hostname': saveModel.overseerr.hostname,
                'Port': Number(saveModel.overseerr.port),
                'ApiKey': saveModel.overseerr.apiKey,
                'DefaultApiUserID': saveModel.overseerr.defaultApiUserID,
                'UseSSL': saveModel.overseerr.useSSL,
                'Version': saveModel.overseerr.version,
                'Restrictions': saveModel.restrictions,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setOverseerrClient({
                        overseerr: {
                            hostname: saveModel.overseerr.hostname,
                            port: saveModel.overseerr.port,
                            apiKey: saveModel.overseerr.apiKey,
                            defaultApiUserID: saveModel.overseerr.defaultApiUserID,
                            useSSL: saveModel.overseerr.useSSL,
                            version: saveModel.overseerr.version,
                        },
                        restrictions: saveModel.restrictions
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};