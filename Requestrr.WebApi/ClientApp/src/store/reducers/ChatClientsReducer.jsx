import { GET_SETTINGS } from "../actions/ChatClientsActions"

export default function ChatClientsReducer(state = {}, action) {
  if (action.type === GET_SETTINGS) {
    return {
      chatClient: action.payload.client,
      clientId: action.payload.clientId,
      botToken: action.payload.botToken,
      commandPrefix: action.payload.commandPrefix,
      statusMessage: action.payload.statusMessage,
      monitoredChannels: action.payload.monitoredChannels,
      tvShowRoles: action.payload.tvShowRoles,
      movieRoles: action.payload.movieRoles,
      enableDirectMessageSupport: action.payload.enableDirectMessageSupport
    }
  }

  return { ...state };
}