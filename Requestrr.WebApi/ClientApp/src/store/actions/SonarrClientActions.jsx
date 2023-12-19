export const SONARR_SET_CLIENT = "tvShowsClients:set_sonarr_client";
export const SONARR_LOAD_PATHS = "tvShowsClients:load_sonarr_paths";
export const SONARR_SET_PATHS = "tvShowsClients:set_sonarr_paths";
export const SONARR_LOAD_TAGS = "tvShowsClients:load_sonarr_tags";
export const SONARR_SET_TAGS = "tvShowsClients:set_sonarr_tags";
export const SONARR_LOAD_PROFILES = "tvShowsClients:load_sonarr_profiles";
export const SONARR_SET_PROFILES = "tvShowsClients:set_sonarr_profiles";
export const SONARR_LOAD_LANGUAGES = "tvShowsClients:load_sonarr_languages";
export const SONARR_SET_LANGUAGES = "tvShowsClients:set_sonarr_languages";

export function setSonarrClient(settings) {
    return {
        type: SONARR_SET_CLIENT,
        payload: settings
    };
};

export function isLoadingSonarrPaths(isLoading) {
    return {
        type: SONARR_LOAD_PATHS,
        payload: isLoading
    };
};

export function setSonarrPaths(sonarrPaths) {
    return {
        type: SONARR_SET_PATHS,
        payload: sonarrPaths
    };
};

export function isLoadingSonarrProfiles(isLoading) {
    return {
        type: SONARR_LOAD_PROFILES,
        payload: isLoading
    };
};

export function setSonarrProfiles(sonarrProfiles) {
    return {
        type: SONARR_SET_PROFILES,
        payload: sonarrProfiles
    };
};

export function isLoadingSonarrTags(isLoading) {
    return {
        type: SONARR_LOAD_TAGS,
        payload: isLoading
    };
};

export function setSonarrTags(sonarrTags) {
    return {
        type: SONARR_SET_TAGS,
        payload: sonarrTags
    };
};

export function isLoadingSonarrLanguages(isLoading) {
    return {
        type: SONARR_LOAD_LANGUAGES,
        payload: isLoading
    };
};

export function setSonarrLanguages(sonarrLanguages) {
    return {
        type: SONARR_SET_LANGUAGES,
        payload: sonarrLanguages
    };
};

export function addSonarrCategory(category) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.tvShows.sonarr.categories];
        categories.push(category);

        var sonarr = {
            ...state.tvShows.sonarr,
            categories: categories,
        };

        dispatch(setSonarrClient({
            sonarr: sonarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function removeSonarrCategory(categoryId) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.tvShows.sonarr.categories];
        categories = categories.filter(x => x.id !== categoryId);

        var sonarr = {
            ...state.tvShows.sonarr,
            categories: categories,
        };

        dispatch(setSonarrClient({
            sonarr: sonarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setSonarrCategory(categoryId, field, data) {
    return (dispatch, getState) => {
        const state = getState();

        var categories = [...state.tvShows.sonarr.categories];

        for (let index = 0; index < categories.length; index++) {
            if (categories[index].id === categoryId) {
                var category = { ...categories[index] };

                if (field === "name") {
                    category.name = data;
                }
                else if (field === "languageId") {
                    category.languageId = data;
                }
                else if (field === "profileId") {
                    category.profileId = data;
                }
                else if (field === "rootFolder") {
                    category.rootFolder = data;
                }
                else if (field === "tags") {
                    category.tags = state.tvShows.sonarr.tags.map(x => x.id).filter(x => data.includes(x));
                }
                else if (field === "seriesType") {
                    category.seriesType = data;
                }
                else if (field === "useSeasonFolders") {
                    category.useSeasonFolders = data;
                }

                categories[index] = category;
            }
        }

        var sonarr = {
            ...state.tvShows.sonarr,
            categories: categories,
        };

        dispatch(setSonarrClient({
            sonarr: sonarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setSonarrCategories(categories) {
    return (dispatch, getState) => {
        const state = getState();
        var sonarr = {
            ...state.tvShows.sonarr,
            categories: [...categories],
        };

        dispatch(setSonarrClient({
            sonarr: sonarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function setSonarrConnectionSettings(connectionSettings) {
    return (dispatch, getState) => {
        const state = getState();

        var sonarr = {
            ...state.tvShows.sonarr,
            hostname: connectionSettings.hostname,
            baseUrl: connectionSettings.baseUrl,
            port: connectionSettings.port,
            apiKey: connectionSettings.apiKey,
            useSSL: connectionSettings.useSSL,
            version: connectionSettings.version,
        };

        dispatch(setSonarrClient({
            sonarr: sonarr
        }));

        return new Promise((resolve, reject) => {
            return { ok: false };
        });
    };
};

export function loadSonarrRootPaths(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var sonarr = state.tvShows.sonarr;

        if ((!sonarr.hasLoadedPaths && !sonarr.isLoadingPaths) || forceReload) {
            dispatch(isLoadingSonarrPaths(true));

            return fetch("../api/tvshows/sonarr/rootpath", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": sonarr.hostname,
                    'BaseUrl': sonarr.baseUrl,
                    "Port": Number(sonarr.port),
                    "ApiKey": sonarr.apiKey,
                    "UseSSL": sonarr.useSSL,
                    "Version": sonarr.version,
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
                    dispatch(setSonarrPaths(data));

                    return {
                        ok: true,
                        paths: data
                    }
                })
                .catch(err => {
                    dispatch(setSonarrPaths([]));
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


export function loadSonarrProfiles(forceReload) {

    return (dispatch, getState) => {
        const state = getState();

        var sonarr = state.tvShows.sonarr;

        if ((!sonarr.hasLoadedProfiles && !sonarr.isLoadingProfiles) || forceReload) {

            dispatch(isLoadingSonarrProfiles(true));

            return fetch("../api/tvshows/sonarr/profile", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": sonarr.hostname,
                    'BaseUrl': sonarr.baseUrl,
                    "Port": Number(sonarr.port),
                    "ApiKey": sonarr.apiKey,
                    "UseSSL": sonarr.useSSL,
                    "Version": sonarr.version,
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
                    dispatch(setSonarrProfiles(data));

                    return {
                        ok: true,
                        profiles: data
                    }
                })
                .catch(err => {
                    dispatch(setSonarrProfiles([]));
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

export function loadSonarrTags(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var sonarr = state.tvShows.sonarr;

        if ((!sonarr.hasLoadedTags && !sonarr.isLoadingTags) || forceReload) {
            dispatch(isLoadingSonarrTags(true));

            return fetch("../api/tvshows/sonarr/tag", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": sonarr.hostname,
                    'BaseUrl': sonarr.baseUrl,
                    "Port": Number(sonarr.port),
                    "ApiKey": sonarr.apiKey,
                    "UseSSL": sonarr.useSSL,
                    "Version": sonarr.version,
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
                    dispatch(setSonarrTags({ ok: true, data: data }));

                    return {
                        ok: true,
                        tags: data
                    }
                })
                .catch(err => {
                    dispatch(setSonarrTags({ ok: false, data: [] }));
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

export function loadSonarrLanguages(forceReload) {
    return (dispatch, getState) => {
        const state = getState();

        var sonarr = state.tvShows.sonarr;

        if ((!sonarr.hasLoadedLanguages && !sonarr.isLoadingLanguages) || forceReload) {
            dispatch(isLoadingSonarrLanguages(true));

            return fetch("../api/tvshows/sonarr/language", {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${state.user.token}`
                },
                body: JSON.stringify({
                    "Hostname": sonarr.hostname,
                    'BaseUrl': sonarr.baseUrl,
                    "Port": Number(sonarr.port),
                    "ApiKey": sonarr.apiKey,
                    "UseSSL": sonarr.useSSL,
                    "Version": sonarr.version,
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
                    dispatch(setSonarrLanguages(data));

                    return {
                        ok: true,
                        languages: data
                    }
                })
                .catch(err => {
                    dispatch(setSonarrLanguages([]));
                    return { ok: false }
                })
        }
        else {
            return new Promise((resolve, reject) => {
                return { ok: false };
            });
        }
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
                dispatch(loadSonarrLanguages(true));
                dispatch(loadSonarrProfiles(true));
                dispatch(loadSonarrRootPaths(true));
                dispatch(loadSonarrTags(true));

                if (data.ok) {
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    };
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
                'Categories': state.tvShows.sonarr.categories,
                'UseSSL': saveModel.sonarr.useSSL,
                'SearchNewRequests': saveModel.sonarr.searchNewRequests,
                'MonitorNewRequests': saveModel.sonarr.monitorNewRequests,
                "Version": saveModel.sonarr.version,
                'Restrictions': saveModel.restrictions,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    var newSonarr = {
                        ...state.tvShows.sonarr,
                        hostname: saveModel.sonarr.hostname,
                        baseUrl: saveModel.sonarr.baseUrl,
                        port: saveModel.sonarr.port,
                        apiKey: saveModel.sonarr.apiKey,
                        categories: state.tvShows.sonarr.categories,
                        searchNewRequests: saveModel.sonarr.searchNewRequests,
                        monitorNewRequests: saveModel.sonarr.monitorNewRequests,
                        useSSL: saveModel.sonarr.useSSL,
                        version: saveModel.sonarr.version,
                        restrictions: saveModel.restrictions,
                    };

                    dispatch(setSonarrClient({
                        sonarr: newSonarr,
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};