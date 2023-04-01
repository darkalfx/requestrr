import { setOverseerrClient } from "./OverseerrClientActions"
export const OVERSEERR_SET_RADARR_SERVICE_SETTINGS = "movieClients:load_overseerr_profiles";
export const OVERSEERR_LOAD_RADARR_SERVICE_SETTINGS = "movieClients:set_overseerr_profiles";

export function isLoadingRadarrServiceSettings(isLoading) {
    return {
        type: OVERSEERR_LOAD_RADARR_SERVICE_SETTINGS,
        payload: isLoading
    };
};

export function setRadarrServiceSettings(radarrServiceSettings) {
    return {
        type: OVERSEERR_SET_RADARR_SERVICE_SETTINGS,
        payload: radarrServiceSettings
    };
};

export function addOverseerrMovieCategory(category) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.movies.overseerr.categories];
        categories.push(category);

        var overseerr = {
            ...state.movies.overseerr,
            categories: categories,
        };

        dispatch(setOverseerrClient({
            overseerr: overseerr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function removeOverseerrMovieCategory(categoryId) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.movies.overseerr.categories];
        categories = categories.filter(x => x.id !== categoryId);

        var overseerr = {
            ...state.movies.overseerr,
            categories: categories,
        };

        dispatch(setOverseerrClient({
            overseerr: overseerr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setOverseerrMovieCategory(categoryId, field, data) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.movies.overseerr.categories];

        for (let index = 0; index < categories.length; index++) {
            if (categories[index].id === categoryId) {
                var category = { ...categories[index] };

                if (field === "name") {
                    category.name = data;
                }
                else if (field === "serviceId") {
                    category.serviceId = data;
                }
                else if (field === "profileId") {
                    category.profileId = data;
                }
                else if (field === "rootFolder") {
                    category.rootFolder = data;
                }
                else if (field === "tags") {
                    const serviceId = category.serviceId;
                    category.tags = (state.movies.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === serviceId) ? state.movies.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === serviceId)[0].tags : []).filter(x => data.includes(x.id)).map(x => x.id);
                }
                else if (field === "is4K") {
                    category.is4K = data;
                }

                categories[index] = category
            }
        }

        var overseerr = {
            ...state.movies.overseerr,
            categories: categories,
        };

        dispatch(setOverseerrClient({
            overseerr: overseerr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setRadarrCategories(categories) {
    return (dispatch, getState) => {

        const state = getState();
        var overseerr = {
            ...state.movies.overseerr,
            categories: [...categories],
        };

        dispatch(setOverseerrClient({
            overseerr: overseerr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setOverseerrMovieConnectionSettings(connectionSettings) {
    return (dispatch, getState) => {
        const state = getState();

        var overseerr = {
            ...state.movies.overseerr,
            hostname: connectionSettings.hostname,
            port: connectionSettings.port,
            apiKey: connectionSettings.apiKey,
            useSSL: connectionSettings.useSSL,
            version: connectionSettings.version,
        };

        dispatch(setOverseerrClient({
            overseerr: overseerr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function loadRadarrServiceSettings(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var overseerr = state.movies.overseerr;

        if ((!overseerr.hasLoadedRadarrServiceSettings && !overseerr.isLoadinRadarrServiceSettings) || forceReload) {
            dispatch(isLoadingRadarrServiceSettings(true));

            return fetch("../api/movies/overseerr/radarr", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": overseerr.hostname,
                    "Port": Number(overseerr.port),
                    "ApiKey": overseerr.apiKey,
                    "UseSSL": overseerr.useSSL,
                    "Version": overseerr.version,
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
                    dispatch(setRadarrServiceSettings({ ok: true, data: data }));

                    return {
                        ok: true,
                        paths: data
                    };
                })
                .catch(err => {
                    dispatch(setRadarrServiceSettings({ ok: false, data: { radarrServices: [] } }));
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

export function testOverseerrMovieSettings(settings) {
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
                dispatch(loadRadarrServiceSettings(true));

                if (data.ok) {
                    return { ok: true };
                }

                return { ok: false, error: data };
            });
    };
};

export function saveOverseerrMovieClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        var movies = {
            DefaultApiUserID: saveModel.overseerr.defaultApiUserID,
            Categories: state.movies.overseerr.categories
        }

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
                'Movies': movies,
                'UseSSL': saveModel.overseerr.useSSL,
                'Version': saveModel.overseerr.version,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    return { ok: true };
                }

                return { ok: false, error: data };
            });
    }
};