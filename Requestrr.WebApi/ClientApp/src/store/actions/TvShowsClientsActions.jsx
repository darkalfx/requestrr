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

export function setSonarrClient(settings) {
    return {
        type: SET_SONARR_CLIENT,
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
                dispatch(setSettings(data));
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


export function loadSonarrLanguages(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/sonarr/language", {
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
                    languages: data
                }
            })
            .catch(err => { return { ok: false } })
    };
};

export function loadSonarrRootPaths(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/sonarr/rootpath", {
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

export function loadSonarrProfiles(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/sonarr/profile", {
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

export function loadSonarrTags(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/sonarr/tag", {
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

export function testSonarrSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/sonarr/test", {
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

export function saveSonarrClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvshows/sonarr", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Hostname': saveModel.sonarr.hostname,
                'BaseUrl': saveModel.sonarr.baseUrl,
                'Port': Number(saveModel.sonarr.port),
                'ApiKey': saveModel.sonarr.apiKey,
                'TvPath': saveModel.sonarr.tvPath,
                'TvProfile': saveModel.sonarr.tvProfile,
                'TvTags': saveModel.sonarr.tvTags,
                'TvLanguage': saveModel.sonarr.tvLanguage,
                'TvUseSeasonFolders': saveModel.sonarr.tvUseSeasonFolders,
                'AnimePath': saveModel.sonarr.animePath,
                'AnimeProfile': saveModel.sonarr.animeProfile,
                'AnimeTags': saveModel.sonarr.animeTags,
                'AnimeLanguage': saveModel.sonarr.animeLanguage,
                'AnimeUseSeasonFolders': saveModel.sonarr.animeUseSeasonFolders,
                'UseSSL': saveModel.sonarr.useSSL,
                'SearchNewRequests': saveModel.sonarr.searchNewRequests,
                'MonitorNewRequests': saveModel.sonarr.monitorNewRequests,
                "Version": saveModel.sonarr.version,
                'Command': saveModel.command,
                'Restrictions': saveModel.restrictions,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setSonarrClient({
                        sonarr: {
                            hostname: saveModel.sonarr.hostname,
                            baseUrl: saveModel.sonarr.baseUrl,
                            port: saveModel.sonarr.port,
                            apiKey: saveModel.sonarr.apiKey,
                            path: saveModel.sonarr.path,
                            profile: saveModel.sonarr.profile,
                            language: saveModel.sonarr.language,
                            useSeasonFolders: saveModel.sonarr.useSeasonFolders,
                            tvPath: saveModel.sonarr.tvPath,
                            tvProfile: saveModel.sonarr.tvProfile,
                            tvTags: saveModel.sonarr.tvTags,
                            tvLanguage: saveModel.sonarr.tvLanguage,
                            tvUseSeasonFolders: saveModel.sonarr.tvUseSeasonFolders,
                            animePath: saveModel.sonarr.animePath,
                            animeProfile: saveModel.sonarr.animeProfile,
                            animeTags: saveModel.sonarr.animeTags,
                            animeLanguage: saveModel.sonarr.animeLanguage,
                            animeUseSeasonFolders: saveModel.sonarr.animeUseSeasonFolders,
                            searchNewRequests: saveModel.sonarr.searchNewRequests,
                            monitorNewRequests: saveModel.sonarr.monitorNewRequests,
                            useSSL: saveModel.sonarr.useSSL,
                            version: saveModel.sonarr.version
                        },
                        command: saveModel.command,
                        restrictions: saveModel.restrictions
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
                'Command': saveModel.command,
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
                        command: saveModel.command,
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
                'Command': saveModel.command,
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
                        command: saveModel.command,
                        restrictions: saveModel.restrictions
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};