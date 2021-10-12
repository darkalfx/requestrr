export const OVERSEERR_SET_CLIENT = "downloadClients:set_overseerr_client";

export function setOverseerrClient(settings) {
    return {
        type: OVERSEERR_SET_CLIENT,
        payload: settings
    };
};