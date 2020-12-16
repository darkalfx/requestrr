/*!

=========================================================
* Argon Dashboard React - v1.0.0
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
import thunk from 'redux-thunk';
import { BrowserRouter, Route, Switch, Redirect } from "react-router-dom";
import { Provider } from 'react-redux';
import { createStore, applyMiddleware } from 'redux';
import { combineReducers } from 'redux'

import "./assets/vendor/nucleo/css/nucleo.css";
import "./assets/vendor/@fortawesome/fontawesome-free/css/all.min.css";
import "./assets/scss/argon-dashboard-react.scss";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css"

import AdminLayout from "./layouts/Admin.jsx";
import AuthLayout from "./layouts/Auth.jsx";
import UserReducer from './store/reducers/UserReducer';
import ChatClients from './store/reducers/ChatClientsReducer';
import SettingsReducer from './store/reducers/SettingsReducer';
import MovieClients from './store/reducers/MovieClientsReducer';
import TvShowsClients from './store/reducers/TvShowsClientsReducer';
import MusicClients from './store/reducers/MusicClientsReducer';

const store = createStore(combineReducers({
  user: UserReducer,
  chatClients: ChatClients,
  movies: MovieClients,
  tvShows: TvShowsClients,
  music: MusicClients,
  settings: SettingsReducer
}), applyMiddleware(thunk));

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