import { GET_SETTINGS } from "../actions/TvShowsClientsActions"
import { SET_DISABLED_CLIENT } from "../actions/TvShowsClientsActions"
import { SET_OMBI_CLIENT } from "../actions/TvShowsClientsActions"
import { SET_OVERSEERR_CLIENT } from "../actions/TvShowsClientsActions"
import { SET_SONARR_CLIENT } from "../actions/TvShowsClientsActions"

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
        defaultApiUserID: action.payload.overseerr.defaultApiUserID,
        useSSL: action.payload.overseerr.useSSL,
        version: action.payload.overseerr.version,
      },
      sonarr: {
        hostname: action.payload.sonarr.hostname,
        baseUrl: action.payload.sonarr.baseUrl,
        port: action.payload.sonarr.port,
        apiKey: action.payload.sonarr.apiKey,
        tvPath: action.payload.sonarr.tvPath,
        tvProfile: action.payload.sonarr.tvProfile,
        tvTags: action.payload.sonarr.tvTags,
        tvLanguage: action.payload.sonarr.tvLanguage,
        tvUseSeasonFolders: action.payload.sonarr.tvUseSeasonFolders,
        animePath: action.payload.sonarr.animePath,
        animeProfile: action.payload.sonarr.animeProfile,
        animeTags: action.payload.sonarr.animeTags,
        animeLanguage: action.payload.sonarr.animeLanguage,
        animeUseSeasonFolders: action.payload.sonarr.animeUseSeasonFolders,
        searchNewRequests: action.payload.sonarr.searchNewRequests,
        monitorNewRequests: action.payload.sonarr.monitorNewRequests,
        useSSL: action.payload.sonarr.useSSL,
        version: action.payload.sonarr.version
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
  else if (action.type === SET_SONARR_CLIENT) {
    return {
      ...state,
      sonarr: action.payload.sonarr,
      restrictions: action.payload.restrictions,
      client: "Sonarr"
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
  else if (action.type === SET_OVERSEERR_CLIENT) {
    return {
      ...state,
      overseerr: action.payload.overseerr,
      restrictions: action.payload.restrictions,
      client: "Overseerr"
    }
  }

  return { ...state };
}