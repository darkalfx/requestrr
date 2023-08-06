export const RADARR_SET_CLIENT = "movieClients:set_radarr_client";
export const RADARR_LOAD_PATHS = "movieClients:load_radarr_paths";
export const RADARR_SET_PATHS = "movieClients:set_radarr_paths";
export const RADARR_LOAD_TAGS = "movieClients:load_radarr_tags";
export const RADARR_SET_TAGS = "movieClients:set_radarr_tags";
export const RADARR_LOAD_PROFILES = "movieClients:load_radarr_profiles";
export const RADARR_SET_PROFILES = "movieClients:set_radarr_profiles";

export function setRadarrClient(settings) {
    return {
        type: RADARR_SET_CLIENT,
        payload: settings
    };
};

export function isLoadingRadarrPaths(isLoading) {
    return {
        type: RADARR_LOAD_PATHS,
        payload: isLoading
    };
};

export function setRadarrPaths(radarrPaths) {
    return {
        type: RADARR_SET_PATHS,
        payload: radarrPaths
    };
};

export function isLoadingRadarrProfiles(isLoading) {
    return {
        type: RADARR_LOAD_PROFILES,
        payload: isLoading
    };
};

export function setRadarrProfiles(radarrProfiles) {
    return {
        type: RADARR_SET_PROFILES,
        payload: radarrProfiles
    };
};

export function isLoadingRadarrTags(isLoading) {
    return {
        type: RADARR_LOAD_TAGS,
        payload: isLoading
    };
};

export function setRadarrTags(radarrTags) {
    return {
        type: RADARR_SET_TAGS,
        payload: radarrTags
    };
};

export function addRadarrCategory(category) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.movies.radarr.categories];
        categories.push(category);

        var radarr = {
            ...state.movies.radarr,
            categories: categories,
        };

        dispatch(setRadarrClient({
            radarr: radarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function removeRadarrCategory(categoryId) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.movies.radarr.categories];
        categories = categories.filter(x => x.id !== categoryId);

        var radarr = {
            ...state.movies.radarr,
            categories: categories,
        };

        dispatch(setRadarrClient({
            radarr: radarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setRadarrCategory(categoryId, field, data) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.movies.radarr.categories];

        for (let index = 0; index < categories.length; index++) {
            if (categories[index].id === categoryId) {
                var category = { ...categories[index] };

                if (field === "name") {
                    category.name = data;
                }
                else if (field === "minimumAvailability") {
                    category.minimumAvailability = data;
                }
                else if (field === "profileId") {
                    category.profileId = data;
                }
                else if (field === "rootFolder") {
                    category.rootFolder = data;
                }
                else if (field === "tags") {
                    category.tags = state.movies.radarr.tags.map(x => x.id).filter(x => data.includes(x));
                }

                categories[index] = category;
            }
        }

        var radarr = {
            ...state.movies.radarr,
            categories: categories,
        };

        dispatch(setRadarrClient({
            radarr: radarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setRadarrCategories(categories) {
    return (dispatch, getState) => {
        const state = getState();
        var radarr = {
            ...state.movies.radarr,
            categories: [...categories],
        };

        dispatch(setRadarrClient({
            radarr: radarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setRadarrConnectionSettings(connectionSettings) {
    return (dispatch, getState) => {
        const state = getState();

        var radarr = {
            ...state.movies.radarr,
            hostname: connectionSettings.hostname,
            baseUrl: connectionSettings.baseUrl,
            port: connectionSettings.port,
            apiKey: connectionSettings.apiKey,
            useSSL: connectionSettings.useSSL,
            version: connectionSettings.version,
        };

        dispatch(setRadarrClient({
            radarr: radarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function loadRadarrRootPaths(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var radarr = state.movies.radarr;

        if ((!radarr.hasLoadedPaths && !radarr.isLoadingPaths) || forceReload) {
            dispatch(isLoadingRadarrPaths(true));

            return fetch("../api/movies/radarr/rootpath", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": radarr.hostname,
                    'BaseUrl': radarr.baseUrl,
                    "Port": Number(radarr.port),
                    "ApiKey": radarr.apiKey,
                    "UseSSL": radarr.useSSL,
                    "Version": radarr.version,
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
                    dispatch(setRadarrPaths(data));

                    return {
                        ok: true,
                        paths: data
                    }
                })
                .catch(err => {
                    dispatch(setRadarrPaths([]));
                    return { ok: false };
                })
        }
        else {
            return new Promise((resolve, reject) => {
                return { ok: false };
            });
        }
    };
};

export function loadRadarrProfiles(forceReload) {

    return (dispatch, getState) => {
        const state = getState();

        var radarr = state.movies.radarr;

        if ((!radarr.hasLoadedProfiles && !radarr.isLoadingProfiles) || forceReload) {
            dispatch(isLoadingRadarrProfiles(true));

            return fetch("../api/movies/radarr/profile", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": radarr.hostname,
                    'BaseUrl': radarr.baseUrl,
                    "Port": Number(radarr.port),
                    "ApiKey": radarr.apiKey,
                    "UseSSL": radarr.useSSL,
                    "Version": radarr.version,
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
                    dispatch(setRadarrProfiles(data));

                    return {
                        ok: true,
                        profiles: data
                    }
                })
                .catch(err => {
                    dispatch(setRadarrProfiles([]));
                    return { ok: false };
                })
        }
        else {
            return new Promise((resolve, reject) => {
                return { ok: false };
            });
        }
    };
};

export function loadRadarrTags(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var radarr = state.movies.radarr;

        if ((!radarr.hasLoadedTags && !radarr.isLoadingTags) || forceReload) {
            dispatch(isLoadingRadarrTags(true));

            return fetch("../api/movies/radarr/tag", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": radarr.hostname,
                    'BaseUrl': radarr.baseUrl,
                    "Port": Number(radarr.port),
                    "ApiKey": radarr.apiKey,
                    "UseSSL": radarr.useSSL,
                    "Version": radarr.version,
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
                    dispatch(setRadarrTags({ ok: true, data: data }));

                    return {
                        ok: true,
                        tags: data
                    }
                })
                .catch(err => {
                    dispatch(setRadarrTags({ ok: false, data: [] }));
                    return { ok: false };
                })
        }
        else {
            return new Promise((resolve, reject) => {
                return { ok: false };
            });
        }
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
                dispatch(loadRadarrProfiles(true));
                dispatch(loadRadarrRootPaths(true));
                dispatch(loadRadarrTags(true));

                if (data.ok) {
                    return { ok: true };
                }

                return { ok: false, error: data };
            });
    };
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
                'Categories': state.movies.radarr.categories,
                "Version": saveModel.radarr.version,
                'SearchNewRequests': saveModel.radarr.searchNewRequests,
                'MonitorNewRequests': saveModel.radarr.monitorNewRequests
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    var newRadarr = {
                        ...state.movies.radarr,
                        hostname: saveModel.radarr.hostname,
                        baseUrl: saveModel.radarr.baseUrl,
                        port: saveModel.radarr.port,
                        apiKey: saveModel.radarr.apiKey,
                        useSSL: saveModel.radarr.useSSL,
                        categories: state.movies.radarr.categories,
                        searchNewRequests: saveModel.radarr.searchNewRequests,
                        monitorNewRequests: saveModel.radarr.monitorNewRequests,
                        version: saveModel.radarr.version
                    };

                    dispatch(setRadarrClient({
                        radarr: newRadarr
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};