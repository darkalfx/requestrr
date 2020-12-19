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
import Account from "./views/Account.jsx";
import Settings from "./views/Settings.jsx";
import ChatClients from "./views/ChatClients.jsx";
import Movies from "./views/Movies.jsx";
import TvShows from "./views/TvShows.jsx";
import Music from "./views/Music.jsx";
import Register from "./views/Register.jsx";
import Login from "./views/Login.jsx";
import Logout from "./views/Logout.jsx";

var routes = [
  {
    path: "/chatclients",
    name: "Chat clients",
    icon: "fas big fa-comments text-purple",
    component: ChatClients,
    layout: "/admin"
  },
  {
    path: "/movies",
    name: "Movies",
    icon: "fas big fa-film text-orange",
    component: Movies,
    layout: "/admin"
  },
  {
    path: "/tvshows",
    name: "TV Shows",
    icon: "fas fa-tv text-blue",
    component: TvShows,
    layout: "/admin"
  },
  {
    path: "/music",
    name: "Music",
    icon: "fas fa-music text-red",
    component: Music,
    layout: "/admin"
  },
  {
    path: "/account",
    name: "Account",
    icon: "fas big fa-user-shield text-green",
    component: Account,
    layout: "/admin"
  },
  {
    path: "/settings",
    name: "Settings",
    icon: "fas big fa-tools text-gray",
    component: Settings,
    layout: "/admin"
  },
  {
    path: "/login",
    name: "Login",
    icon: "ni ni-user-run text-info",
    component: Login,
    layout: "/auth"
  },
  {
    path: "/register",
    name: "Register",
    icon: "ni ni-single-02 text-orange",
    component: Register,
    layout: "/auth"
  },
  {
    path: "/logout",
    name: "Logout",
    icon: "fas big fa-sign-out-alt text-info",
    component: Logout,
    layout: "/admin"
  },
];
export default routes;
