export const GET_SETTINGS = "settings:get_settings";

export function setSettings(settings) {
    return {
        type: GET_SETTINGS,
        payload: settings
    };
};

export function getSettings() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/settings", {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        })
            .then(data => data.json())
            .then(data => {
                dispatch(setSettings(data));
            });
    };
};

export function saveSettings(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/settings", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Port': Number(saveModel.port),
                'BaseUrl' : saveModel.baseUrl,
                'DisableAuthentication': saveModel.disableAuthentication,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setSettings({
                        port: saveModel.port,
                        baseUrl: saveModel.baseUrl,
                        disableAuthentication: saveModel.disableAuthentication
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};