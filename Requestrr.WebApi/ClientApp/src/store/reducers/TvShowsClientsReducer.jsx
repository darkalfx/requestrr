import { GET_SETTINGS } from "../actions/TvShowsClientsActions"
import { SET_DISABLED_CLIENT } from "../actions/TvShowsClientsActions"
import { SET_OMBI_CLIENT } from "../actions/TvShowsClientsActions"
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
      sonarr: {
        hostname: action.payload.sonarr.hostname,
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
        useSSL: action.payload.sonarr.useSSL,
        version: action.payload.sonarr.version
      },
      command: action.payload.command,
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
      command: action.payload.command,
      client: "Sonarr"
    }
  }
  else if (action.type === SET_OMBI_CLIENT) {
    return {
      ...state,
      ombi: action.payload.ombi,
      command: action.payload.command,
      client: "Ombi"
    }
  }

  return { ...state };
}