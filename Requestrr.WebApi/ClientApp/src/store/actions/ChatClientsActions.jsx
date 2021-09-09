export const GET_SETTINGS = "chatclients:get_settings";

export function setSettings(settings) {
    return {
        type: GET_SETTINGS,
        payload: settings
    };
};

export function getSettings() {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/chatclients", {
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

export function testSettings(settings) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/chatclients/discord/test", {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                "BotToken": settings.botToken,
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

export function save(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/chatclients", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Client': saveModel.chatClient,
                'ClientId': saveModel.clientId,
                'BotToken': saveModel.botToken,
                'StatusMessage': saveModel.statusMessage,
                'CommandPrefix': saveModel.commandPrefix,
                'MonitoredChannels': saveModel.monitoredChannels,
                'TvShowRoles': saveModel.tvShowRoles,
                'MovieRoles': saveModel.movieRoles,
                'EnableRequestsThroughDirectMessages': saveModel.enableRequestsThroughDirectMessages,
                'AutomaticallyNotifyRequesters': saveModel.automaticallyNotifyRequesters,
                'NotificationMode': saveModel.notificationMode,
                'NotificationChannels': saveModel.notificationChannels,
                'AutomaticallyPurgeCommandMessages': saveModel.automaticallyPurgeCommandMessages,
                'DisplayHelpCommandInDMs': saveModel.displayHelpCommandInDMs,
                'Language': saveModel.language,
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setSettings({
                        chatClient: saveModel.client,
                        clientId: saveModel.clientId,
                        botToken: saveModel.botToken,
                        commandPrefix: saveModel.commandPrefix,
                        statusMessage: saveModel.statusMessage,
                        monitoredChannels: saveModel.monitoredChannels,
                        tvShowRoles: saveModel.tvShowRoles,
                        movieRoles: saveModel.movieRoles,
                        enableRequestsThroughDirectMessages: saveModel.enableRequestsThroughDirectMessages,
                        automaticallyNotifyRequesters: saveModel.automaticallyNotifyRequesters,
                        notificationMode: saveModel.notificationMode,
                        notificationChannels: saveModel.notificationChannels,
                        automaticallyPurgeCommandMessages: saveModel.automaticallyPurgeCommandMessages,
                        displayHelpCommandInDMs: saveModel.displayHelpCommandInDMs,
                        language: saveModel.language,
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data }
            });
    }
};