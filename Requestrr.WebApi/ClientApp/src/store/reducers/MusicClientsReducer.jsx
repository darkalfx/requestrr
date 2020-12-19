import { GET_SETTINGS } from "../actions/MusicClientsActions"
import { SET_DISABLED_CLIENT } from "../actions/MusicClientsActions"
import { SET_LIDARR_CLIENT } from "../actions/MusicClientsActions"

export default function MusicClientsReducer(state = {}, action) {
  if (action.type === GET_SETTINGS) {
    return {
      ...state,
      client: action.payload.client,
      lidarr: {
        hostname: action.payload.lidarr.hostname,
        baseUrl: action.payload.lidarr.baseUrl,
        port: action.payload.lidarr.port,
        apiKey: action.payload.lidarr.apiKey,
        musicPath: action.payload.lidarr.musicPath,
        musicProfile: action.payload.lidarr.musicProfile,
        musicMetadataProfile: action.payload.lidarr.musicMetadataProfile,
        musicTags: action.payload.lidarr.musicTags,
        musicLanguage: action.payload.lidarr.musicLanguage,
        musicUseAlbumFolders: action.payload.lidarr.musicUseAlbumFolders,
        searchNewRequests: action.payload.lidarr.searchNewRequests,
        monitorNewRequests: action.payload.lidarr.monitorNewRequests,
        useSSL: action.payload.lidarr.useSSL,
        version: action.payload.lidarr.version
      },
      command: action.payload.command,
      restrictions: action.payload.restrictions
    }
  }
  else if (action.type === SET_DISABLED_CLIENT) {
    return {
      ...state,
      client: "Disabled"
    }
  }
  else if (action.type === SET_LIDARR_CLIENT) {
    return {
      ...state,
      lidarr: action.payload.lidarr,
      command: action.payload.command,
      restrictions: action.payload.restrictions,
      client: "Lidarr"
    }
  }

  return { ...state };
}