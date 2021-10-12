import { GET_SETTINGS } from "../actions/TvShowsClientsActions"
import { SET_DISABLED_CLIENT } from "../actions/TvShowsClientsActions"
import { SET_OMBI_CLIENT } from "../actions/TvShowsClientsActions"

export default function TvShowsClientsReducer(state = {}, action) {
  if (action.type === GET_SETTINGS) {
    return {
      ...state,
      client: action.payload.client,
      ombi: {
        hostname: action.payload.ombi.hostname,
        baseUrl: action.payload.ombi.baseUrl,
        port: action.payload.ombi.port,
        apiKey: action.payload.ombi.apiKey,
        apiUsername: action.payload.ombi.apiUsername,
        useSSL: action.payload.ombi.useSSL,
        version: action.payload.ombi.version,
      },
      overseerr: {
        hostname: action.payload.overseerr.hostname,
        port: action.payload.overseerr.port,
        apiKey: action.payload.overseerr.apiKey,
        defaultApiUserID: action.payload.overseerr.tvShows.defaultApiUserId,
        categories: action.payload.overseerr.tvShows.categories,
        useSSL: action.payload.overseerr.useSSL,
        version: action.payload.overseerr.version,
        sonarrServiceSettings: { sonarrServices: [] },
        isLoadinSonarrServiceSettings: false,
        hasLoadedSonarrServiceSettings: false,
        isSonarrServiceSettingsValid: false,
      },
      sonarr: {
        hostname: action.payload.sonarr.hostname,
        baseUrl: action.payload.sonarr.baseUrl,
        port: action.payload.sonarr.port,
        apiKey: action.payload.sonarr.apiKey,
        categories: action.payload.sonarr.categories,
        searchNewRequests: action.payload.sonarr.searchNewRequests,
        monitorNewRequests: action.payload.sonarr.monitorNewRequests,
        useSSL: action.payload.sonarr.useSSL,
        version: action.payload.sonarr.version,
        isLoadingPaths: false,
        hasLoadedPaths: false,
        arePathsValid: false,
        paths: [],
        isLoadingProfiles: false,
        hasLoadedProfiles: false,
        areProfilesValid: false,
        profiles: [],
        isLoadingTags: false,
        hasLoadedTags: false,
        areTagsValid: false,
        tags: [],
        isLoadingLanguages: false,
        hasLoadedLanguages: false,
        areLanguagesValid: false,
        languages: [],
      },
      restrictions: action.payload.restrictions
    }
  }
  else if (action.type === SET_DISABLED_CLIENT) {
    return {
      ...state,
      client: "Disabled"
    }
  }
  else if (action.type === SET_OMBI_CLIENT) {
    return {
      ...state,
      ombi: action.payload.ombi,
      restrictions: action.payload.restrictions,
      client: "Ombi"
    }
  }

  return { ...state };
}