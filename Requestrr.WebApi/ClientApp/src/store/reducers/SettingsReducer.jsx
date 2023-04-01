import { GET_SETTINGS } from "../actions/SettingsActions"

export default function SettingsReducer(state = {}, action) {
  if (action.type === GET_SETTINGS) {
    return {
      port: action.payload.port,
      baseUrl: action.payload.baseUrl,
      disableAuthentication: action.payload.disableAuthentication,
    };
  }

  return { ...state };
}