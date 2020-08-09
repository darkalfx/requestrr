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

        return fetch("/api/settings", {
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

export function saveSettings(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("/api/settings", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Port': Number(saveModel.port),
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setSettings({
                        port: saveModel.port,
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};