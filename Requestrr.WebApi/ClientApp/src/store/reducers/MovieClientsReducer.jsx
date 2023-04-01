import { GET_SETTINGS } from "../actions/MovieClientsActions"
import { SET_DISABLED_CLIENT } from "../actions/MovieClientsActions"
import { SET_OMBI_CLIENT } from "../actions/MovieClientsActions"

export default function MovieClientsReducer(state = {}, action) {
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
        defaultApiUserID: action.payload.overseerr.movies.defaultApiUserId,
        categories: action.payload.overseerr.movies.categories,
        useSSL: action.payload.overseerr.useSSL,
        version: action.payload.overseerr.version,
        radarrServiceSettings: { radarrServices: [] },
        isLoadinRadarrServiceSettings: false,
        hasLoadedRadarrServiceSettings: false,
        isRadarrServiceSettingsValid: false,
      },
      radarr: {
        hostname: action.payload.radarr.hostname,
        baseUrl: action.payload.radarr.baseUrl,
        port: action.payload.radarr.port,
        apiKey: action.payload.radarr.apiKey,
        useSSL: action.payload.radarr.useSSL,
        categories: action.payload.radarr.categories,
        searchNewRequests: action.payload.radarr.searchNewRequests,
        monitorNewRequests: action.payload.radarr.monitorNewRequests,
        version: action.payload.radarr.version,
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
      },
    };
  } else if (action.type === SET_DISABLED_CLIENT) {
    return {
      ...state,
      client: "Disabled"
    };
  } else if (action.type === SET_OMBI_CLIENT) {
    return {
      ...state,
      ombi: action.payload.ombi,
      client: "Ombi"
    };
  }

  return { ...state };
}