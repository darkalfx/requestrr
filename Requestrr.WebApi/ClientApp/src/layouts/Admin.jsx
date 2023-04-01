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
import { useEffect, useRef, useState } from "react";
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


function Admin(props) {
  const [isLoading, setIsLoading] = useState(true);
  const mainContent = useRef(null);



  useEffect(() => {
    props.validateLogin()
      .then(data => setIsLoading(false));
  }, []);

  

  document.documentElement.scrollTop = 0;
  document.scrollingElement.scrollTop = 0;

  if (!isLoading) {
    mainContent.scrollTop = 0;
  }

  props.validateLogin();

  const getRoutes = routes => {
    return routes.map((prop, key) => {
      if (prop.layout === "/admin") {
        return (
          <Route
            path={prop.layout + prop.path}
            children={prop.component}
            key={key}
          />
        );
      } else {
        return null;
      }
    });
  };

  
  if (!isLoading) {
    return (
      <>
        <Sidebar
          {...props}
          routes={routes}
          logo={{
            innerLink: "/admin/",
            imgSrc: requestrrLogo,
            imgAlt: "Requestrr Logo"
          }}
        />
        <div className="main-content" ref={mainContent}>
          <Switch>
            {
              !isLoading
                ? props.isLoggedIn
                  ? getRoutes(routes)
                  : null
                : null
            }
            {
              !isLoading ?
                props.isLoggedIn ?
                  <Route path="*" render={() => <Redirect to="/admin/chatclients" />} />
                  : <Route path="*" render={() => <Redirect to="/auth/" />} />
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

const mapPropsToState = state => {
  return {
    isLoggedIn: state.user.isLoggedIn
  }
};

const mapPropsToAction = {
  validateLogin: validateLogin
};

export default connect(mapPropsToState, mapPropsToAction)(Admin);