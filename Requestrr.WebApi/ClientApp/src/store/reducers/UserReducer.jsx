import { HAS_REGISTERED } from "../actions/UserActions"
import { LOGGED_IN } from "../actions/UserActions"
import { LOGGED_OUT } from "../actions/UserActions"


export default function UserReducer(state = {}, action) {
  if (action.type === HAS_REGISTERED) {
    return {
      ...state,
      hasRegistered: action.payload,
    };
  } else if (action.type === LOGGED_IN) {
    return {
      hasRegistered: true,
      token: action.payload,
      isLoggedIn: true,
    };
  } else if (action.type === LOGGED_OUT) {
    return {
      ...state,
      token: null,
      isLoggedIn: false,
    };
  }

  return { ...state };
}