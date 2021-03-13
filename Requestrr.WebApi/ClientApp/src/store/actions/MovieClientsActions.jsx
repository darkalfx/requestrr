export const GET_SETTINGS = "movieClients:get_settings";
export const SET_DISABLED_CLIENT = "movieClients:set_disabled_client";
export const SET_OMBI_CLIENT = "movieClients:set_ombi_client";
export const SET_OVERSEERR_CLIENT = "movieClients:set_overseerr_client";
export const SET_RADARR_CLIENT = "movieClients:set_radarr_client";

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

export function setRadarrClient(settings) {
    return {
        type: SET_RADARR_CLIENT,
        payload: settings
    };
};

export function getSettings() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies", {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            }
        })
            .then(data => data.json())
            .then(data => {
                dispatch(setSettings(data));
            })
    };
};

export function testOmbiSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/ombi/test", {
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

        return fetch("../api/movies/overseerr/test", {
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

export function loadRadarrRootPaths(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/radarr/rootpath", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "Hostname": settings.hostname,
                'BaseUrl': settings.baseUrl,
                "Port": Number(settings.port),
                "ApiKey": settings.apiKey,
                "UseSSL": settings.useSSL,
                "Version": settings.version,
            })
        })
            .then(data => {
                if (data.status !== 200) {
                    throw new Error("Bad request.");
                }

                return data;
            })
            .then(data => data.json())
            .then(data => {
                return {
                    ok: true,
                    paths: data
                }
            })
            .catch(err => { return { ok: false } })
    };
};

export function loadRadarrProfiles(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/radarr/profile", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "Hostname": settings.hostname,
                'BaseUrl': settings.baseUrl,
                "Port": Number(settings.port),
                "ApiKey": settings.apiKey,
                "UseSSL": settings.useSSL,
                "Version": settings.version,
            })
        })
            .then(data => {
                if (data.status !== 200) {
                    throw new Error("Bad request.");
                }

                return data;
            })
            .then(data => data.json())
            .then(data => {
                return {
                    ok: true,
                    profiles: data
                }
            })
            .catch(err => { return { ok: false } })
    };
};

export function loadRadarrTags(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/radarr/tag", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "Hostname": settings.hostname,
                'BaseUrl': settings.baseUrl,
                "Port": Number(settings.port),
                "ApiKey": settings.apiKey,
                "UseSSL": settings.useSSL,
                "Version": settings.version,
            })
        })
            .then(data => {
                if (data.status !== 200) {
                    throw new Error("Bad request.");
                }

                return data;
            })
            .then(data => data.json())
            .then(data => {
                return {
                    ok: true,
                    tags: data
                }
            })
            .catch(err => { return { ok: false } })
    };
};

export function testRadarrSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/radarr/test", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "Hostname": settings.hostname,
                'BaseUrl': settings.baseUrl,
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

export function saveDisabledClient() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/disable", {
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

export function saveRadarrClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/radarr", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Hostname': saveModel.radarr.hostname,
                'BaseUrl': saveModel.radarr.baseUrl,
                'Port': Number(saveModel.radarr.port),
                'ApiKey': saveModel.radarr.apiKey,
                'UseSSL': saveModel.radarr.useSSL,
                'MoviePath': saveModel.radarr.moviePath,
                'MovieProfile': saveModel.radarr.movieProfile,
                'MovieMinAvailability': saveModel.radarr.movieMinAvailability,
                'MovieTags': saveModel.radarr.movieTags,
                'AnimePath': saveModel.radarr.animePath,
                'AnimeProfile': saveModel.radarr.animeProfile,
                'AnimeMinAvailability': saveModel.radarr.animeMinAvailability,
                'AnimeTags': saveModel.radarr.animeTags,
                "Version": saveModel.radarr.version,
                'SearchNewRequests': saveModel.radarr.searchNewRequests,
                'MonitorNewRequests': saveModel.radarr.monitorNewRequests,
                'Command': saveModel.command,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setRadarrClient({
                        radarr: {
                            hostname: saveModel.radarr.hostname,
                            baseUrl: saveModel.radarr.baseUrl,
                            port: saveModel.radarr.port,
                            apiKey: saveModel.radarr.apiKey,
                            useSSL: saveModel.radarr.useSSL,
                            moviePath: saveModel.radarr.moviePath,
                            movieProfile: saveModel.radarr.movieProfile,
                            movieMinAvailability: saveModel.radarr.movieMinAvailability,
                            movieTags: saveModel.radarr.movieTags,
                            animePath: saveModel.radarr.animePath,
                            animeProfile: saveModel.radarr.animeProfile,
                            animeMinAvailability: saveModel.radarr.animeMinAvailability,
                            animeTags: saveModel.radarr.animeTags,
                            searchNewRequests: saveModel.radarr.searchNewRequests,
                            monitorNewRequests: saveModel.radarr.monitorNewRequests,
                            version: saveModel.radarr.version
                        },
                        command: saveModel.command
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};

export function saveOmbiClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/movies/ombi", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Hostname': saveModel.ombi.hostname,
                'BaseUrl': saveModel.ombi.baseUrl,
                'Port': Number(saveModel.ombi.port),
                'ApiKey': saveModel.ombi.apiKey,
                'ApiUsername': saveModel.ombi.apiUsername,
                'UseSSL': saveModel.ombi.useSSL,
                'Version': saveModel.ombi.version,
                'Command': saveModel.command,
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
                        command: saveModel.command
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

        return fetch("../api/movies/overseerr", {
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
                'Command': saveModel.command,
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
                        command: saveModel.command
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};