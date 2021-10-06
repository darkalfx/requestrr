import { RADARR_SET_CLIENT } from "../actions/RadarrClientActions"
import { RADARR_LOAD_PATHS } from "../actions/RadarrClientActions"
import { RADARR_SET_PATHS } from "../actions/RadarrClientActions"
import { RADARR_LOAD_TAGS } from "../actions/RadarrClientActions"
import { RADARR_SET_TAGS } from "../actions/RadarrClientActions"
import { RADARR_LOAD_PROFILES } from "../actions/RadarrClientActions"
import { RADARR_SET_PROFILES } from "../actions/RadarrClientActions"

export default function RadarrClientsReducer(state = {}, action) {
  if (action.type === RADARR_SET_CLIENT) {
    return {
      ...state,
      radarr: action.payload.radarr,
      client: "Radarr"
    }
  }
  else if (action.type === RADARR_LOAD_PATHS) {
    var newState = { ...state };
    var newRadarr = { ...newState.radarr };

    newRadarr.isLoadingPaths = action.payload;

    newState.radarr = newRadarr;

    return newState;
  }
  else if (action.type === RADARR_SET_PATHS) {
    var newState = { ...state };
    var newRadarr = { ...newState.radarr };

    newRadarr.isLoadingPaths = false;
    newRadarr.hasLoadedPaths = true;
    newRadarr.arePathsValid = action.payload.length > 0;
    newRadarr.paths = action.payload;

    newState.radarr = newRadarr;

    return newState;
  }
  else if (action.type === RADARR_LOAD_PROFILES) {
    var newState = { ...state };
    var newRadarr = { ...newState.radarr };

    newRadarr.isLoadingProfiles = action.payload;

    newState.radarr = newRadarr;

    return newState;
  }
  else if (action.type === RADARR_SET_PROFILES) {
    var newState = { ...state };
    var newRadarr = { ...newState.radarr };

    newRadarr.isLoadingProfiles = false;
    newRadarr.hasLoadedProfiles = true;
    newRadarr.areProfilesValid = action.payload.length > 0;
    newRadarr.profiles = action.payload;

    newState.radarr = newRadarr;

    return newState;
  }
  else if (action.type === RADARR_LOAD_TAGS) {
    var newState = { ...state };
    var newRadarr = { ...newState.radarr };

    newRadarr.isLoadingTags = action.payload;

    newState.radarr = newRadarr;

    return newState;
  }
  else if (action.type === RADARR_SET_TAGS) {
    var newState = { ...state };
    var newRadarr = { ...newState.radarr };

    newRadarr.isLoadingTags = false;
    newRadarr.hasLoadedTags = true;
    newRadarr.areTagsValid = action.payload.ok;
    newRadarr.tags = action.payload.data;

    newState.radarr = newRadarr;

    return newState;
  }

  return { ...state };
}