import { GET_SETTINGS } from "../actions/MovieClientsActions"
import { SET_DISABLED_CLIENT } from "../actions/MovieClientsActions"
import { SET_OMBI_CLIENT } from "../actions/MovieClientsActions"
import { SET_RADARR_CLIENT } from "../actions/MovieClientsActions"

export default function MovieClientsReducer(state = {}, action) {
  if (action.type === GET_SETTINGS) {
    return {
      ...state,
      client: action.payload.client,
      ombi: {
        hostname: action.payload.ombi.hostname,
        port: action.payload.ombi.port,
        apiKey: action.payload.ombi.apiKey,
        apiUsername: action.payload.ombi.apiUsername,
        useSSL: action.payload.ombi.useSSL,
        version: action.payload.ombi.version,
      },
      radarr: {
        hostname: action.payload.radarr.hostname,
        port: action.payload.radarr.port,
        apiKey: action.payload.radarr.apiKey,
        useSSL: action.payload.radarr.useSSL,
        moviePath: action.payload.radarr.moviePath,
        movieProfile: action.payload.radarr.movieProfile,
        movieMinAvailability: action.payload.radarr.movieMinAvailability,
        movieTags:  action.payload.radarr.movieTags,
        animePath: action.payload.radarr.animePath,
        animeProfile: action.payload.radarr.animeProfile,
        animeMinAvailability: action.payload.radarr.animeMinAvailability,
        animeTags:  action.payload.radarr.animeTags,
        version: action.payload.radarr.version,
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
  else if (action.type === SET_RADARR_CLIENT) {
    return {
      ...state,
      radarr: action.payload.radarr,
      command: action.payload.command,
      client: "Radarr"
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