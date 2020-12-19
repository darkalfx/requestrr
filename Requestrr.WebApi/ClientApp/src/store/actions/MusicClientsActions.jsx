export const GET_SETTINGS = "musicClients:get_settings";
export const SET_DISABLED_CLIENT = "musicClients:set_disabled_client";
export const SET_LIDARR_CLIENT = "musicClients:set_lidarr_client";

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

export function setLidarrClient(settings) {
    return {
        type: SET_LIDARR_CLIENT,
        payload: settings
    };
};

export function getSettings() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music", {
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

export function loadLidarrRootPaths(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music/lidarr/rootpath", {
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

export function loadLidarrProfiles(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music/lidarr/profile", {
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

export function loadLidarrMetadataProfiles(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music/lidarr/metadataprofile", {
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
                    metadataProfiles: data
                }
            })
            .catch(err => { return { ok: false } })
    };
};

export function loadLidarrTags(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music/lidarr/tag", {
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

export function testLidarrSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music/lidarr/test", {
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

        return fetch("../api/music/disable", {
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

export function saveLidarrClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/music/lidarr", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Hostname': saveModel.lidarr.hostname,
                'BaseUrl': saveModel.lidarr.baseUrl,
                'Port': Number(saveModel.lidarr.port),
                'ApiKey': saveModel.lidarr.apiKey,
                'MusicPath': saveModel.lidarr.musicPath,
                'MusicProfile': saveModel.lidarr.musicProfile,
                'MusicMetadataProfile': saveModel.lidarr.musicMetadataProfile,
                'MusicTags': saveModel.lidarr.musicTags,
                'MusicUseAlbumFolders': saveModel.lidarr.musicUseAlbumFolders,
                'UseSSL': saveModel.lidarr.useSSL,
                'SearchNewRequests': saveModel.lidarr.searchNewRequests,
                'MonitorNewRequests': saveModel.lidarr.monitorNewRequests,
                "Version": saveModel.lidarr.version,
                'Command': saveModel.command,
                'Restrictions': saveModel.restrictions,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setLidarrClient({
                        lidarr: {
                            hostname: saveModel.lidarr.hostname,
                            baseUrl: saveModel.lidarr.baseUrl,
                            port: saveModel.lidarr.port,
                            apiKey: saveModel.lidarr.apiKey,
                            musicPath: saveModel.lidarr.musicPath,
                            musicProfile: saveModel.lidarr.musicProfile,
                            musicMetadataProfile: saveModel.lidarr.musicMetadataProfile,
                            musicTags: saveModel.lidarr.musicTags,
                            musicUseAlbumFolders: saveModel.lidarr.musicUseAlbumFolders,
                            searchNewRequests: saveModel.lidarr.searchNewRequests,
                            monitorNewRequests: saveModel.lidarr.monitorNewRequests,
                            useSSL: saveModel.lidarr.useSSL,
                            version: saveModel.lidarr.version
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