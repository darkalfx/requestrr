/*!

=========================================================
* Argon Dashboard React - v1.0.0   -----   Updated packages to v1.2.2
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-react
* Copyright 2019 Creative Tim (https://www.creative-tim.com)
* Licensed under MIT (https://github.com/creativetimofficial/argon-dashboard-react/blob/master/LICENSE.md)

* Coded by Creative Tim

=========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

*/

import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter, Route, Switch, Redirect } from "react-router-dom";
import { Provider } from 'react-redux';
import { configureStore } from "@reduxjs/toolkit";

import "./assets/vendor/nucleo/css/nucleo.css";
import "./assets/vendor/@fortawesome/fontawesome-free/css/all.min.css";
import "./assets/scss/argon-dashboard-react.scss";

import AdminLayout from "./layouts/Admin.jsx";
import AuthLayout from "./layouts/Auth.jsx";
import UserReducer from './store/reducers/UserReducer';
import ChatClients from './store/reducers/ChatClientsReducer';
import SettingsReducer from './store/reducers/SettingsReducer';
import MovieClients from './store/reducers/MovieClientsReducer';
import RadarrClients from './store/reducers/RadarrClientsReducer';
import SonarrClients from './store/reducers/SonarrClientsReducer';
import OverseerrClients from './store/reducers/OverseerrClientsReducer';
import TvShowsClients from './store/reducers/TvShowsClientsReducer';

function combinedMovieClientsReducer(state = {}, action) {
  if (action.type.includes("radarr")) {
    return RadarrClients(state, action);
  }
  else if (action.type.includes("overseerr")) {
    return OverseerrClients(state, action);
  }
  else {
    return MovieClients(state, action);
  }
}

function combinedTvShowsClientsReducer(state = {}, action) {
  if (action.type.includes("sonarr")) {
    return SonarrClients(state, action);
  }
  else if (action.type.includes("overseerr")) {
    return OverseerrClients(state, action);
  }
  else {
    return TvShowsClients(state, action);
  }
}

const store = configureStore({
  middleware: (getDefaultMiddleware) => getDefaultMiddleware(),
  reducer: {
    user: UserReducer,
    chatClients: ChatClients,
    movies: combinedMovieClientsReducer,
    tvShows: combinedTvShowsClientsReducer,
    settings: SettingsReducer
  }
});

fetch("../api/settings", {
  method: 'GET',
  headers: {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  }
})
  .then(data => data.json())
  .then(data => {
    ReactDOM.render(
      <Provider store={store}>
        <BrowserRouter basename={data.baseUrl}>
          <Switch>
            <Route path="/admin" render={props => <AdminLayout {...props} />} />
            <Route path="/auth" render={props => <AuthLayout {...props} />} />
            <Redirect from="*" to="/auth/login" />
          </Switch>
        </BrowserRouter>
      </Provider>,
      document.getElementById("root")
    );
  });