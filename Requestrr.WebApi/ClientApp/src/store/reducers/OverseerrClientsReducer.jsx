import { OVERSEERR_SET_CLIENT } from "../actions/OverseerrClientActions"
import { OVERSEERR_SET_RADARR_SERVICE_SETTINGS } from "../actions/OverseerrClientRadarrActions"
import { OVERSEERR_LOAD_RADARR_SERVICE_SETTINGS } from "../actions/OverseerrClientRadarrActions"
import { OVERSEERR_SET_SONARR_SERVICE_SETTINGS } from "../actions/OverseerrClientSonarrActions"
import { OVERSEERR_LOAD_SONARR_SERVICE_SETTINGS } from "../actions/OverseerrClientSonarrActions"

export default function OverseerrClientsReducer(state = {}, action) {
  if (action.type === OVERSEERR_SET_CLIENT) {
    return {
      ...state,
      overseerr: action.payload.overseerr,
      client: "Overseerr"
    }
  }
  else if (action.type === OVERSEERR_LOAD_RADARR_SERVICE_SETTINGS) {
    var newState = { ...state };
    var newOverseerr = { ...newState.overseerr };

    newOverseerr.isLoadinRadarrServiceSettings = action.payload;

    newState.overseerr = newOverseerr;

    return newState;
  }
  else if (action.type === OVERSEERR_SET_RADARR_SERVICE_SETTINGS) {
    var newState = { ...state };
    var newOverseerr = { ...newState.overseerr };

    newOverseerr.isLoadinRadarrServiceSettings = false;
    newOverseerr.hasLoadedRadarrServiceSettings = true;
    newOverseerr.isRadarrServiceSettingsValid = action.payload.ok;
    newOverseerr.radarrServiceSettings = action.payload.data;

    newState.overseerr = newOverseerr;

    return newState;
  }
  else if (action.type === OVERSEERR_LOAD_SONARR_SERVICE_SETTINGS) {
    var newState = { ...state };
    var newOverseerr = { ...newState.overseerr };

    newOverseerr.isLoadinSonarrServiceSettings = action.payload;

    newState.overseerr = newOverseerr;

    return newState;
  }
  else if (action.type === OVERSEERR_SET_SONARR_SERVICE_SETTINGS) {
    var newState = { ...state };
    var newOverseerr = { ...newState.overseerr };

    newOverseerr.isLoadinSonarrServiceSettings = false;
    newOverseerr.hasLoadedSonarrServiceSettings = true;
    newOverseerr.isSonarrServiceSettingsValid = action.payload.ok;
    newOverseerr.sonarrServiceSettings = action.payload.data;

    newState.overseerr = newOverseerr;

    return newState;
  }

  return { ...state };
}