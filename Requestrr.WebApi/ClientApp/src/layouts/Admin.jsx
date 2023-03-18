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
import { Route, Switch, Redirect } from "react-router-dom";
import { connect } from 'react-redux';
// reactstrap components
import { Container } from "reactstrap";
// core components
import AdminFooter from "../components/Footers/AdminFooter.jsx";
import Sidebar from "../components/Sidebar/Sidebar.jsx";
import { validateLogin } from "../store/actions/UserActions"

import routes from "../routes.js";
import requestrrLogo from "../assets/img/brand/requestrr_black.svg";


class Admin extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isLoading: true,
    };
  }

  componentDidMount() {
    this.props.validateLogin()
      .then(data => this.setState({ isLoading: false }));
  }

  componentDidUpdate(e) {
    document.documentElement.scrollTop = 0;
    document.scrollingElement.scrollTop = 0;

    if (!this.state.isLoading) {
      this.refs.mainContent.scrollTop = 0;
    }

    this.props.validateLogin();
  }

  getRoutes = routes => {
    return routes.map((prop, key) => {
      if (prop.layout === "/admin") {
        return (
          <Route
            path={prop.layout + prop.path}
            component={prop.component}
            key={key}
          />
        );
      } else {
        return null;
      }
    });
  };
  getBrandText = path => {
    for (let i = 0; i < routes.length; i++) {
      if (
        this.props.location.pathname.indexOf(
          routes[i].layout + routes[i].path
        ) !== -1
      ) {
        return routes[i].name;
      }
    }
    return "Brand";
  };
  render() {
    if (!this.state.isLoading) {
      return (
        <>
          <Sidebar
            {...this.props}
            routes={routes}
            logo={{
              innerLink: "/admin/",
              imgSrc: requestrrLogo,
              imgAlt: "Requestrr Logo"
            }}
          />
          <div className="main-content" ref="mainContent">
            <Switch>
            {
              !this.state.isLoading
                ? this.props.isLoggedIn
                  ? this.getRoutes(routes)
                  : null
                : null
            }
            {
              !this.state.isLoading ?
                this.props.isLoggedIn ?
                  <Redirect from="*" to="/admin/chatclients" />
                  : <Redirect from="*" to="/auth/" />
                : null
            }
            </Switch>
            <Container fluid>
              <AdminFooter />
            </Container>
          </div>
        </>
      );
    }

    return null;
  }
}

const mapPropsToState = state => {
  return {
    isLoggedIn: state.user.isLoggedIn
  }
};

const mapPropsToAction = {
  validateLogin: validateLogin
};

export default connect(mapPropsToState, mapPropsToAction)(Admin);