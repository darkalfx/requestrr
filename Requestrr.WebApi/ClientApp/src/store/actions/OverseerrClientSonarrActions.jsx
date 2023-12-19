import { setOverseerrClient } from "./OverseerrClientActions"
export const OVERSEERR_SET_SONARR_SERVICE_SETTINGS = "tvShowClients:load_overseerr_profiles";
export const OVERSEERR_LOAD_SONARR_SERVICE_SETTINGS = "tvShowClients:set_overseerr_profiles";

export function isLoadingSonarrServiceSettings(isLoading) {
    return {
        type: OVERSEERR_LOAD_SONARR_SERVICE_SETTINGS,
        payload: isLoading
    };
};

export function setSonarrServiceSettings(sonarrServiceSettings) {
    return {
        type: OVERSEERR_SET_SONARR_SERVICE_SETTINGS,
        payload: sonarrServiceSettings
    };
};

export function addOverseerrTvShowCategory(category) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.tvShows.overseerr.categories];
        categories.push(category);

        var overseerr = {
            ...state.tvShows.overseerr,
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

export function removeOverseerrTvShowCategory(categoryId) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.tvShows.overseerr.categories];
        categories = categories.filter(x => x.id !== categoryId);

        var overseerr = {
            ...state.tvShows.overseerr,
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

export function setOverseerrTvShowCategory(categoryId, field, data) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.tvShows.overseerr.categories];

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
                else if (field === "languageProfileId") {
                    category.languageProfileId = data;
                }
                else if (field === "rootFolder") {
                    category.rootFolder = data;
                }
                else if (field === "tags") {
                    const serviceId = category.serviceId;
                    category.tags = (state.tvShows.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === serviceId) ? state.tvShows.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === serviceId)[0].tags : []).filter(x => data.includes(x.id)).map(x => x.id);
                }
                else if (field === "is4K") {
                    category.is4K = data;
                }

                categories[index] = category;
            }
        }

        var overseerr = {
            ...state.tvShows.overseerr,
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

export function setSonarrCategories(categories) {
    return (dispatch, getState) => {

        const state = getState();
        var overseerr = {
            ...state.tvShows.overseerr,
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

export function setOverseerrTvShowConnectionSettings(connectionSettings) {
    return (dispatch, getState) => {
        const state = getState();

        var overseerr = {
            ...state.tvShows.overseerr,
            hostname: connectionSettings.hostname,
            port: connectionSettings.port,
            apiKey: connectionSettings.apiKey,
            useSSL: connectionSettings.useSSL,
            useMovieIssue: connectionSettings.useMovieIssue,
            useTVIssue: connectionSettings.useTVIssue,
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

export function loadSonarrServiceSettings(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var overseerr = state.tvShows.overseerr;

        if ((!overseerr.hasLoadedSonarrServiceSettings && !overseerr.isLoadinSonarrServiceSettings) || forceReload) {
            dispatch(isLoadingSonarrServiceSettings(true));

            return fetch("../api/tvShows/overseerr/sonarr", {
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
                    "UseMovieIssue": overseerr.UseMovieIssue,
                    "UseTVIssue": overseerr.UseTVIssue,
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
                    dispatch(setSonarrServiceSettings({ ok: true, data: data }));

                    return {
                        ok: true,
                        paths: data
                    }
                })
                .catch(err => {
                    dispatch(setSonarrServiceSettings({ ok: false, data: { sonarrServices: [] } }));
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

export function testOverseerrTvShowSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/tvShows/overseerr/test", {
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
                dispatch(loadSonarrServiceSettings(true));

                if (data.ok) {
                    return { ok: true };
                }

                return { ok: false, error: data };
            });
    };
};

export function saveOverseerrTvShowClient(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        var tvShows = {
            DefaultApiUserID: saveModel.overseerr.defaultApiUserID,
            Categories: state.tvShows.overseerr.categories
        }

        return fetch("../api/tvShows/overseerr", {
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
                'Restrictions': saveModel.restrictions,
                'TvShows': tvShows,
                'UseSSL': saveModel.overseerr.useSSL,
                "UseMovieIssue": saveModel.overseerr.useMovieIssue,
                "UseTVIssue": saveModel.overseerr.useTVIssue,
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