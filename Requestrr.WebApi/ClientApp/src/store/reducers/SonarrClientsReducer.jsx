import { SONARR_SET_CLIENT } from "../actions/SonarrClientActions"
import { SONARR_LOAD_PATHS } from "../actions/SonarrClientActions"
import { SONARR_SET_PATHS } from "../actions/SonarrClientActions"
import { SONARR_LOAD_TAGS } from "../actions/SonarrClientActions"
import { SONARR_SET_TAGS } from "../actions/SonarrClientActions"
import { SONARR_LOAD_PROFILES } from "../actions/SonarrClientActions"
import { SONARR_SET_PROFILES } from "../actions/SonarrClientActions"
import { SONARR_LOAD_LANGUAGES } from "../actions/SonarrClientActions"
import { SONARR_SET_LANGUAGES } from "../actions/SonarrClientActions"

export default function SonarrClientsReducer(state = {}, action) {
  var newState;
  var newSonarr;

  if (action.type === SONARR_SET_CLIENT) {
    return {
      ...state,
      sonarr: action.payload.sonarr,
      client: "Sonarr"
    };
  } else if (action.type === SONARR_LOAD_PATHS) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingPaths = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_SET_PATHS) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingPaths = false;
    newSonarr.hasLoadedPaths = true;
    newSonarr.arePathsValid = action.payload.length > 0;
    newSonarr.paths = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_LOAD_PROFILES) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingProfiles = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_SET_PROFILES) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingProfiles = false;
    newSonarr.hasLoadedProfiles = true;
    newSonarr.areProfilesValid = action.payload.length > 0;
    newSonarr.profiles = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_LOAD_TAGS) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingTags = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_SET_TAGS) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingTags = false;
    newSonarr.hasLoadedTags = true;
    newSonarr.areTagsValid = action.payload.ok;
    newSonarr.tags = action.payload.data;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_LOAD_LANGUAGES) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingLanguages = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  } else if (action.type === SONARR_SET_LANGUAGES) {
    newState = { ...state };
    newSonarr = { ...newState.sonarr };

    newSonarr.isLoadingLanguages = false;
    newSonarr.hasLoadedLanguages = true;
    newSonarr.areLanguagesValid = action.payload.length > 0;
    newSonarr.languages = action.payload;

    newState.sonarr = newSonarr;

    return newState;
  }

  return { ...state };
}